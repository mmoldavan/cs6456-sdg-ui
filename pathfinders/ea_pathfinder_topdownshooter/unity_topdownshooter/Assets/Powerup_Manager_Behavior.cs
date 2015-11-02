/*
Copyright (C) 2015 Electronic Arts Inc.  All rights reserved.
 
This software is solely licensed pursuant to the Hackathon License Agreement,
Available at:  http://www.eapathfinders.com/license
All other use is strictly prohibited. 
*/
using UnityEngine;
using System.Collections;

public class Powerup_Manager_Behavior : MonoBehaviour {

	public GameObject powerup_Prefab;

	public float currentPowerupSpawnTime;
	public float maxPowerupSpawnTime;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		UpdateSpawnPowerup ();
	}

	void UpdateSpawnPowerup() {
		
		if (currentPowerupSpawnTime <= 0) {
			currentPowerupSpawnTime = maxPowerupSpawnTime;
			Instantiate (powerup_Prefab, transform.position, new Quaternion());
		} else {
			currentPowerupSpawnTime -= Time.deltaTime;
		}
		
	}
}
