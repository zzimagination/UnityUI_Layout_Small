using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Layout : MonoBehaviour
{
	public enum FlexDirection
	{
		Horizontal = 0b0001,
		ReHorizontal = 0b0101,
		Vertical = 0b0010,
		ReVertical = 0b0110,
	}

	public enum NumberType
	{
		Auto,
		Number,
		Percent,
	}

	public bool root;

	public bool grow = true;

	public float growFactor = 1;

	public bool shrink = true;

	public FlexDirection flexDirection;

	[SerializeField]
	public Vector4 padding;

	public Vector4 margin;

	public NumberType widthType;

	public int width;

	public float widthPercent;

	public NumberType heightType;

	public int height;

	public float heightPercent;

	[HideInInspector]
	public List<Layout> children = new List<Layout>();

	[SerializeField]
	private int _expectWidth;

	[SerializeField]
	private int _expectHeight;

	[SerializeField]
	private int _realWidth;

	[SerializeField]
	private int _realHeight;

	private int _enoughtLength = 0;

	private Vector4 _realMargin;

	private Vector4 _realPadding;

	private RectTransform _rectTransform;

	[SerializeField]
	private Vector2 _offset;

	private RectTransform _canvasTransform;

	private void SetFixup()
	{
		_rectTransform = GetComponent<RectTransform>();
		if( root )
		{
			ReFixup();
		}
		else
		{
			_rectTransform = GetComponent<RectTransform>();
			Transform parent = transform.parent;
			var layout = parent.GetComponent<Layout>();
			while( layout != null )
			{
				if( layout.root )
				{
					layout.SetFixup();
					return;
				}
				parent = parent.parent;
				layout = parent.GetComponent<Layout>();
			}
		}
	}


	// Start is called before the first frame update
	void Start()
	{
		_rectTransform = GetComponent<RectTransform>();
		if( root )
		{
			GetRootSize();
		}
	}

	public void ReFixup()
	{
		if( _rectTransform == null )
		{
			_rectTransform = GetComponent<RectTransform>();
		}
		if( root )
		{
			GetRootSize();
			Fixup();
		}
	}

	void GetRootSize()
	{
		if( _rectTransform.anchorMin != _rectTransform.anchorMax )
		{
			if( _canvasTransform == null )
			{
				var canvas = GetComponentInParent<Canvas>();
				var canvasRect = canvas.GetComponent<RectTransform>();
				_canvasTransform = canvasRect;
			}
			width = (int)( _rectTransform.sizeDelta.x + _canvasTransform.sizeDelta.x * ( _rectTransform.anchorMax.x - _rectTransform.anchorMin.x ) );
			height = (int)( _rectTransform.sizeDelta.y + _canvasTransform.sizeDelta.y * ( _rectTransform.anchorMax.y - _rectTransform.anchorMin.y ) );
		}
		else
		{
			width = (int)_rectTransform.sizeDelta.x;
			height = (int)_rectTransform.sizeDelta.y;
		}
	}

	void UpdateChildren()
	{
		children.Clear();
		for( int i = 0; i < transform.childCount; i++ )
		{
			var child = transform.GetChild( i );
			var layout = child.GetComponent<Layout>();
			if( layout && !layout.root )
			{
				children.Add( layout );
			}
		}
	}

	void Fixup()
	{
		Vector2Int offset = default;
		UpdateExpectLayout( out _expectWidth, out _expectHeight, FlexDirection.Horizontal );
		UpdateRealLayout( width, 1, height, ref offset, FlexDirection.Horizontal );
	}

	void UpdateExpectLayout( out int width, out int height, FlexDirection flexDir )
	{
		UpdateChildren();
		if( _rectTransform == null )
		{
			_rectTransform = GetComponent<RectTransform>();
		}
		if( !root )
		{
			_rectTransform.pivot = new Vector2( 0, 0 );
			_rectTransform.anchorMin = new Vector2( 0, 0 );
			_rectTransform.anchorMax = new Vector2( 0, 0 );
		}

		UpdateSafeSettings();

		int wTotal = 0;
		int hTotal = 0;
		for( int i = 0; i < children.Count; i++ )
		{
			children[i].UpdateExpectLayout( out int w, out int h, this.flexDirection );
			wTotal += w;
			hTotal += h;
		}
		if( ( flexDir & FlexDirection.Horizontal ) != 0 )
		{
			width = grow ? wTotal : this.width;
			height = this.height;
		}
		else
		{
			height = grow ? hTotal : this.height;
			width = this.width;
		}
		width += (int)_realMargin.z + (int)_realMargin.w;
		height += (int)_realMargin.x + (int)_realMargin.y;
		_expectWidth = width;
		_expectHeight = height;
	}


	void UpdateRealLayout( int space, float spaceCount, int limit, ref Vector2Int offset, FlexDirection flexDir )
	{
		int spaceLength = (int)( space / spaceCount * this.growFactor );
		bool isHor = ( flexDir & FlexDirection.Horizontal ) != 0;
		bool isRe = ( (int)flexDir & 0b0100 ) != 0;
		if( isHor )
		{
			_realWidth = grow ? spaceLength + _enoughtLength : _enoughtLength;
			if( isRe )
			{
				offset.x -= _realWidth;
				_offset.x = offset.x;
				_offset.y = offset.y;
			}
			else
			{

				_offset.x = offset.x;
				_offset.y = offset.y;
				offset.x += _realWidth;
			}

			_realHeight = limit;
		}
		else
		{
			_realHeight = grow ? spaceLength + _enoughtLength : _enoughtLength;
			if( isRe )
			{
				_offset.x = offset.x;
				_offset.y = offset.y;
				offset.y += _realHeight;
			}
			else
			{
				_offset.x = offset.x;
				offset.y -= _realHeight;
				_offset.y = offset.y;
			}

			_realWidth = limit;
		}

		bool isHorizontal = ( this.flexDirection & FlexDirection.Horizontal ) != 0;
		float noEnough = 0;
		int enougthLength = isHorizontal ? _realWidth : _realHeight;
		Vector2Int offsetLength = default;
		int limitChild = isHorizontal ? this._realHeight : this._realWidth;

		if( isHorizontal )
		{
			enougthLength -= (int)_realPadding.z + (int)_realPadding.w + (int)_realMargin.z + (int)_realMargin.w;

			limitChild -= (int)_realPadding.x + (int)_realPadding.y + (int)_realMargin.x + (int)_realMargin.y;
			offsetLength.x = this.flexDirection == FlexDirection.Horizontal ?
				(int)_realPadding.z + (int)_realMargin.z :
				_realWidth - (int)_realPadding.w - (int)_realMargin.w;
			offsetLength.y = (int)_realPadding.y + (int)_realMargin.y;
		}
		else
		{
			enougthLength -= (int)_realPadding.x + (int)_realPadding.y + (int)_realMargin.x + (int)_realMargin.y;

			limitChild -= (int)_realPadding.z + (int)_realPadding.w + (int)_realMargin.z + (int)_realMargin.w;
			offsetLength.x = (int)_realPadding.z + (int)_realMargin.z;
			offsetLength.y = this.flexDirection == FlexDirection.Vertical ?
				_realHeight - (int)_realPadding.x - (int)_realMargin.x :
				(int)_realPadding.x + (int)_realMargin.x;
		}

		int contentLength = isHorizontal ? _realWidth : _realHeight;
		for( int i = 0; i < children.Count; i++ )
		{
			children[i].UpdateEnoughLayout( ref enougthLength, ref noEnough, this.flexDirection, contentLength );
		}
		for( int i = 0; i < children.Count; i++ )
		{
			children[i].UpdateGrowLayout( ref enougthLength, ref noEnough, this.flexDirection );
		}

		for( int i = 0; i < children.Count; i++ )
		{
			children[i].UpdateRealLayout( enougthLength, noEnough, limitChild, ref offsetLength, this.flexDirection );
		}

		if( !root )
		{
			_rectTransform.anchoredPosition3D = new Vector3( _offset.x, _offset.y, 0 );
			_rectTransform.sizeDelta = new Vector2( _realWidth, _realHeight );
		}
	}

	void UpdateEnoughLayout( ref int enoughLength, ref float noEnough, FlexDirection flexDirection, int contentLength )
	{
		if( grow )
		{
			return;
		}

		if( ( flexDirection & FlexDirection.Horizontal ) != 0 )
		{
			if( widthType == NumberType.Percent )
			{
				_expectWidth = (int)( contentLength * widthPercent );
			}
			if( enoughLength >= _expectWidth )
			{
				_enoughtLength = _expectWidth;
				enoughLength -= _expectWidth;
			}
			else
			{
				if( shrink )
				{
					_enoughtLength = enoughLength;
				}
				else
				{
					_enoughtLength = _expectWidth;
				}
				enoughLength = 0;
			}
		}
		else
		{
			if( heightType == NumberType.Percent )
			{
				_expectHeight = (int)( contentLength * heightPercent );
			}
			if( enoughLength >= _expectHeight )
			{
				_enoughtLength = _expectHeight;
				enoughLength -= _expectHeight;
			}
			else
			{
				if( shrink )
				{
					_enoughtLength = enoughLength;
				}
				else
				{
					_enoughtLength = _expectHeight;
				}
				enoughLength = 0;
			}
		}
	}

	void UpdateGrowLayout( ref int enoughLength, ref float noEnough, FlexDirection flexDirection )
	{
		if( grow )
		{
			if( ( flexDirection & FlexDirection.Horizontal ) != 0 )
			{
				if( enoughLength >= _expectWidth )
				{
					_enoughtLength = _expectWidth;
					enoughLength -= _expectWidth;
				}
				else
				{
					if( shrink )
					{
						_enoughtLength = enoughLength;
					}
					else
					{
						_enoughtLength = _expectWidth;
					}
					enoughLength = 0;
				}
			}
			else
			{
				if( enoughLength >= _expectHeight )
				{
					_enoughtLength = _expectHeight;
					enoughLength -= _expectHeight;
				}
				else
				{
					if( shrink )
					{
						_enoughtLength = enoughLength;
					}
					else
					{
						_enoughtLength = _expectHeight;
					}
					enoughLength = 0;
				}
			}
			noEnough += this.growFactor;
			return;
		}
	}

	void UpdateSafeSettings()
	{
		this.width = ( this.width < 0 ) ? 0 : this.width;
		this.height = ( this.height < 0 ) ? 0 : this.height;
		this.widthPercent = ( this.widthPercent < 0 ) ? 0 : ( ( this.widthPercent > 1 ) ? 1 : this.widthPercent );
		this.heightPercent = ( this.heightPercent < 0 ) ? 0 : ( ( this.heightPercent > 1 ) ? 1 : this.heightPercent );
		this.growFactor = ( this.growFactor < 0 ) ? 0 : this.growFactor;

		int halfRealHeight = _realHeight / 2;
		int halfRealWidth = _realWidth / 2;
		_realMargin.x = margin.x < 0 ? 0 : ( margin.x > halfRealHeight ? halfRealHeight : margin.x );
		_realMargin.y = margin.y < 0 ? 0 : ( margin.y > halfRealHeight ? halfRealHeight : margin.y );
		_realMargin.z = margin.z < 0 ? 0 : ( margin.z > halfRealWidth ? halfRealWidth : margin.z );
		_realMargin.w = margin.w < 0 ? 0 : ( margin.w > halfRealWidth ? halfRealWidth : margin.w );
		_realPadding.x = padding.x < 0 ? 0 : ( padding.x > halfRealHeight ? halfRealHeight : padding.x );
		_realPadding.y = padding.y < 0 ? 0 : ( padding.y > halfRealHeight ? halfRealHeight : padding.y );
		_realPadding.z = padding.z < 0 ? 0 : ( padding.z > halfRealWidth ? halfRealWidth : padding.z );
		_realPadding.w = padding.w < 0 ? 0 : ( padding.w > halfRealWidth ? halfRealWidth : padding.w );
	}

	public void RootLayout()
	{
		_rectTransform = GetComponent<RectTransform>();
		if( root )
		{
			ReFixup();
		}
		else
		{
			_rectTransform = GetComponent<RectTransform>();
			Transform parent = transform.parent;
			var layout = parent.GetComponent<Layout>();
			while( layout != null )
			{
				if( layout.root )
				{
					layout.SetFixup();
					return;
				}
				parent = parent.parent;
				layout = parent.GetComponent<Layout>();
			}
		}
	}	
}
