using UnityEngine;
using System.Collections;

public class ArrowTargeting : MonoBehaviour {
	public Transform targetOrienter;

	// Use this for initialization
	void Start () {
		targetOrienter = GameObject.Find ("Boat/Rowboat/TargetOrienter").transform;
	}
	
	// Update is called once per frame
	void Update () {
		Transform beacon = RaceManager.getRaceManager().currentBeacon.transform.Find ("CrossLine").transform;
		RectTransform arrow = this.GetComponent<RectTransform> ();

		//this.transform.rotation = Quaternion.LookRotation (position, Vector3.up);

		targetOrienter.LookAt (beacon.position);
		Debug.Log (targetOrienter.localEulerAngles);
		arrow.eulerAngles = new Vector3(0.0f, 0.0f, (360 - targetOrienter.localEulerAngles.y) - 90);
	}
}
