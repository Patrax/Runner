using UnityEngine;
using System.Collections;

public class LoadSceneButton : MonoBehaviour {
	
	public void LoadScene(int level)
	{
		Application.LoadLevel(level);
	}
}