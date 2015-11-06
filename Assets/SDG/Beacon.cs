using UnityEngine;
using System.Collections;

public class Beacon : MonoBehaviour {
	private ParticleSystem crossLine;
	private bool active;

	public void OnTriggerEnter (Collider other) {
		if (active) {
			Deactivate();
			RaceManager.getRaceManager ().targetNextBeacon ();
		}
	}

	public void Activate() {
		if (crossLine == null) {
			crossLine = transform.Find ("CrossLine").GetComponent<ParticleSystem> ();
		}
		active = true;
		crossLine.Play ();
	}

	public void Deactivate() {
		active = false;
		crossLine.Stop ();
	}
}
