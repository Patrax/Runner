using UnityEngine;
using System.Collections;

public class HideGUI : MonoBehaviour {

	public GameObject canvas;
	private bool activate = true;

	void Start()
	{
		canvas.SetActive(activate);
	}

	void Update()
	{
		if (Input.GetKeyDown (KeyCode.H)){
			ToggleVisibility();
		}
	}

	void ToggleVisibility()
	{
		activate = !activate;
		canvas.SetActive(activate);
	}

}