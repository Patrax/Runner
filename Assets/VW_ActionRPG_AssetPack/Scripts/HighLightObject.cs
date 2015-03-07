using UnityEngine;
using System.Collections;

public class HighLightObject : MonoBehaviour {


	private Color startcolor;

	void OnMouseEnter()
	{
		startcolor = GetComponent<Renderer>().material.color;

		GetComponent<Renderer>().material.color = Color.white;
		int children = transform.childCount;
		for (int i = 0; i < children; ++i)
			transform.GetChild(i).GetComponent<Renderer>().material.color = Color.white;
			

	}
	void OnMouseExit()
	{
		GetComponent<Renderer>().material.color = startcolor;
		int children = transform.childCount;
		for (int i = 0; i < children; ++i)
			transform.GetChild(i).GetComponent<Renderer>().material.color = startcolor;
	}
	
}


