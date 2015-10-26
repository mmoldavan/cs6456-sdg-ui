/*
Copyright (C) 2015 Electronic Arts Inc.  All rights reserved.
 
This software is solely licensed pursuant to the Hackathon License Agreement,
Available at:  http://www.eapathfinders.com/license
All other use is strictly prohibited. 
*/
using UnityEngine;
using System.Collections;

public class Bullet_Behavior : MonoBehaviour {

	public float speed;
	public float lifeTime;

	// Use this for initialization
	void Start () {
		GetComponent<Rigidbody> ().velocity = this.transform.forward * speed;
	}
	
	// Update is called once per frame
	void Update () {
		UpdateLifetime ();
	}

	void UpdateLifetime() {
		lifeTime -= Time.deltaTime;
		if(lifeTime <= 0) {
			Destroy(this.gameObject);
		}
	}
}
