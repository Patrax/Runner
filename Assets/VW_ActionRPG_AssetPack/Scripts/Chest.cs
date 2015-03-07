using UnityEngine;
using System.Collections;

public class Chest : MonoBehaviour {

	public Transform player;
	public float minDistance;
	public ParticleSystem ChestParticles;

	private bool opened = false;
	
	void Start()
	{
		if (ChestParticles) {
			Instantiate (ChestParticles, this.transform.position, this.transform.rotation);
		}
	}
	
	void OnMouseDown()
	{
		if (player) {
			float dist = Vector3.Distance (player.position, transform.position);

			if (dist < minDistance && opened == false) {
				GetComponent<Animation>().Play ("Opening");
				GetComponent<AudioSource>().Play();
				opened = true;
			}
		}
		else {
			Debug.Log ("assing a Player to open the chest");
		}
	}
}

