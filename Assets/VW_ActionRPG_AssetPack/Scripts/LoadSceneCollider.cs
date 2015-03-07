using UnityEngine;
using System.Collections;

public class LoadSceneCollider : MonoBehaviour {

	public int level;

	void OnTriggerEnter (Collider other)
	{
		Debug.Log ("Loading Level");
		Application.LoadLevel(level);
	}
}
