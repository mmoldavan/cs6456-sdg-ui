using UnityEngine;
using System.Collections;

public class Beacon : MonoBehaviour {
	private ParticleSystem crossLine;
	private bool active;

	public void OnTriggerEnter (Collider other) {
		if (active) {
			Debug.Log ("triggered");
			Deactivate();
			RaceManager.getRaceManager ().targetNextBeacon ();
		}
	}

	public void Activate() {
		Debug.Log ("Activated");
		if (crossLine == null) {
			crossLine = transform.Find ("CrossLine").GetComponent<ParticleSystem> ();
		}
		Debug.Log (crossLine.transform.position);
		active = true;
		crossLine.Play ();
	}

	public void Deactivate() {
		active = false;
		crossLine.Stop ();
	}
}
