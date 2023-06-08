using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor( typeof( Layout ) )]
public class LayoutEditor : Editor
{
	private float _labelWidth;
	private int _rootWidth;
	private int _rootHeight;

	private void OnEnable()
	{
		GetRootSize();
	}

	public override void OnInspectorGUI()
	{
		//base.OnInspectorGUI();

		var layout = this.serializedObject.targetObject as Layout;
		GUI.tooltip = "Is this the root layout?";
		layout.root = EditorGUILayout.Toggle( "Root", layout.root );

		if( layout.root )
		{
			GUI.enabled = false;
		}
		layout.grow = EditorGUILayout.Toggle( "Grow", layout.grow );
		if( layout.grow )
		{
			layout.growFactor = EditorGUILayout.FloatField( "Grow Factor", layout.growFactor );
		}
		layout.shrink = EditorGUILayout.Toggle( "Shrink", layout.shrink );
		if( layout.root )
		{
			GUI.enabled = true;
		}

		layout.flexDirection = (Layout.FlexDirection)EditorGUILayout.EnumPopup( "Flex Direction", layout.flexDirection );
		{
			EditorGUILayout.LabelField( "Margin" );
			EditorGUI.indentLevel++;
			EditorGUILayout.BeginHorizontal();
			_labelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 50;

			int x = EditorGUILayout.IntField( "Top", (int)layout.margin.x );
			int y = EditorGUILayout.IntField( "Bottom", (int)layout.margin.y );
			int z = EditorGUILayout.IntField( "Left", (int)layout.margin.z );
			int w = EditorGUILayout.IntField( "Right", (int)layout.margin.w );
			layout.margin = new Vector4( x, y, z, w );

			EditorGUIUtility.labelWidth = _labelWidth;
			EditorGUILayout.EndHorizontal();
			EditorGUI.indentLevel--;
		}

		{
			EditorGUILayout.LabelField( "Padding" );
			EditorGUI.indentLevel++;
			EditorGUILayout.BeginHorizontal();
			_labelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 50;
			int x = EditorGUILayout.IntField( "Top", (int)layout.padding.x );
			int y = EditorGUILayout.IntField( "Bottom", (int)layout.padding.y );
			int z = EditorGUILayout.IntField( "Left", (int)layout.padding.z );
			int w = EditorGUILayout.IntField( "Right", (int)layout.padding.w );
			layout.padding = new Vector4( x, y, z, w );
			EditorGUIUtility.labelWidth = _labelWidth;
			EditorGUILayout.EndHorizontal();
			EditorGUI.indentLevel--;
		}
		if( layout.root )
		{
			GUI.enabled = false;
			EditorGUILayout.IntField( "Root Width", _rootWidth );
			EditorGUILayout.IntField( "Root Height", _rootHeight );
			GUI.enabled = true;
		}
		else
		{
			layout.widthType = (Layout.NumberType)EditorGUILayout.EnumPopup( "Width Type", layout.widthType );
			EditorGUI.indentLevel++;
			if( layout.widthType == Layout.NumberType.Number )
			{
				layout.width = EditorGUILayout.IntField( "Width", layout.width );
			}
			else if( layout.widthType == Layout.NumberType.Percent )
			{
				layout.widthPercent = EditorGUILayout.FloatField( "Percent", layout.widthPercent );
			}
			EditorGUI.indentLevel--;
			layout.heightType = (Layout.NumberType)EditorGUILayout.EnumPopup( "Height Type", layout.heightType );
			EditorGUI.indentLevel++;
			if( layout.heightType == Layout.NumberType.Number )
			{
				layout.height = EditorGUILayout.IntField( "Height", layout.height );
			}
			else if( layout.heightType == Layout.NumberType.Percent )
			{
				layout.heightPercent = EditorGUILayout.FloatField( "Percent", layout.heightPercent );
			}
			EditorGUI.indentLevel--;
		}
		GUI.enabled = false;
		EditorGUILayout.IntField( "Expect Width", serializedObject.FindProperty( "_expectWidth" ).intValue );
		EditorGUILayout.IntField( "Expect Height", serializedObject.FindProperty( "_expectHeight" ).intValue );
		EditorGUILayout.IntField( "Real Width", serializedObject.FindProperty( "_realWidth" ).intValue );
		EditorGUILayout.IntField( "Real Height", serializedObject.FindProperty( "_realHeight" ).intValue );
		GUI.enabled = true;

		if( GUILayout.Button( "Layout" ) )
		{
			layout.RootLayout();
		}

		this.serializedObject.ApplyModifiedProperties();
	}

	private void GetRootSize()
	{
		var layout = this.serializedObject.targetObject as Layout;
		if( !layout.root )
		{
			return;
		}
		var rectTransform = layout.GetComponent<RectTransform>();
		if( rectTransform == null )
		{
			return;
		}
		var canvas = layout.GetComponentInParent<Canvas>();
		var canvasTransform = canvas.GetComponent<RectTransform>();
		if( canvasTransform == null )
		{
			return;
		}
		if( rectTransform.anchorMin != rectTransform.anchorMax )
		{
			_rootWidth = (int)( rectTransform.sizeDelta.x + canvasTransform.sizeDelta.x * ( rectTransform.anchorMax.x - rectTransform.anchorMin.x ) );
			_rootHeight = (int)( rectTransform.sizeDelta.y + canvasTransform.sizeDelta.y * ( rectTransform.anchorMax.y - rectTransform.anchorMin.y ) );
		}
		else
		{
			_rootWidth = (int)rectTransform.sizeDelta.x;
			_rootHeight = (int)rectTransform.sizeDelta.y;
		}
	}
}
