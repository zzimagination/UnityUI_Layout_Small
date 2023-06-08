using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayoutController : MonoBehaviour
{
	private Layout _layout;

	// Start is called before the first frame update
	void Start()
	{
		var layout = GetComponent<Layout>();
		if( layout && layout.root )
		{
			_layout = layout;
		}
		else
		{
			_layout = null;
		}
	}

	// Update is called once per frame
	void Update()
	{
		if( _layout == null )
		{
			return;
		}
		_layout.ReFixup();
	}
}
