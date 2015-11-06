using UnityEngine;
using System.Collections.Generic;

public class RaceManager : MonoBehaviour {
	private static RaceManager myRaceManager;

	public List<Beacon> beacons;

	private int currentBeaconPos;
	public Beacon currentBeacon;

	private float lapStartTime;
	private float bestLapTime;

	// Use this for initialization
	void Start () {
		RaceManager.myRaceManager = this;
		initLap ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void targetNextBeacon() {
		if (currentBeaconPos >= beacons.Count - 1) {
			endLap ();
			initLap ();
		} else {
			currentBeaconPos++;
			currentBeacon = beacons[currentBeaconPos];
			currentBeacon.Activate ();
		}
	}

	public void initLap() {
		currentBeaconPos = 0;
		currentBeacon = beacons [currentBeaconPos];
		currentBeacon.Activate ();
		lapStartTime = Time.time;
	}

	public void endLap() {
		float finishTime = Time.time - lapStartTime;
		if (finishTime > bestLapTime) {
			bestLapTime = finishTime;
		}
	}

	public string getCurrentLapTime() {
		float time = Time.time - lapStartTime;
		int minutes = Mathf.FloorToInt(time / 60F);
		int seconds = Mathf.FloorToInt(time % 60);
		return string.Format("{0:00}:{1:00}", minutes, seconds);
	}

	public string getBestLapTime() {
		int minutes = Mathf.FloorToInt(bestLapTime / 60F);
		int seconds = Mathf.FloorToInt(bestLapTime % 60);
		return string.Format("{0:00}:{1:00}", minutes, seconds);
	}

	public static RaceManager getRaceManager() {
		return myRaceManager;
	}
}
