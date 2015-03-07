using UnityEngine;
using System.Collections;

public class Doors : MonoBehaviour {

	public bool automaticDoor = true;


	void OnTriggerEnter (Collider other)
	{
		if (automaticDoor == true)
		{
		GetComponent<Animation>().Play ("Opening");
		GetComponent<AudioSource>().Play();
		}		
	}

	void OnTriggerExit (Collider other)
	{
		if (automaticDoor == true)
		{
		GetComponent<Animation>().Play ("Closing");
		}
	}


}

