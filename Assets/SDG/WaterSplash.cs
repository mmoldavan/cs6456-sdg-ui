﻿using UnityEngine;
using System.Collections;

public class WaterSplash : MonoBehaviour {
	public BoatUserControl boatControl;
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void CreateWaterSplash()
	{
		
	}

	//Animation Event Handlers
	public void leftOarApplyRotation(){
		//rb.AddTorque(transform.right * boatControl.torque * 1, ForceMode.Acceleration);
	}
	public void rightOarApplyRotation(){
		//rb.AddTorque(transform.right * -boatControl.torque * 1, ForceMode.Acceleration);
	}
	public void leftOarApplyForward(){
		
	}
	public void rightOarApplyForward(){
		
	}
}