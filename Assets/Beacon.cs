using UnityEngine;
using System.Collections;

public class Beacon : MonoBehaviour {
	public RaceManager raceManager;
	
	void OnTriggerEnter (Collider other) {
		raceManager.targetNextBeacon ();
	}
}
