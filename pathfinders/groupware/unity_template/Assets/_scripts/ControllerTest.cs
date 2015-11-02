using UnityEngine;
using System.Collections;
using BladeCast;
using System;

public class ControllerTest : MonoBehaviour {

    public float speed;
    private Rigidbody rb;
    private float moveVertical = 0;

	// Use this for initialization
	void Start () {
        BCMessenger.Instance.RegisterListener("connect", 0, this.gameObject, "HandleExample_ControllerInput");
        rb = GetComponent<Rigidbody>();
	}

    void FixedUpdate()
    {

        //BCMessenger.Instance.SendToListeners("set_powerup_button_time", "time", 103, -1);
        //float moveHorizontal = Input.GetAxis("Horizontal");
        //float moveVertical = Input.GetAxis("Vertical");
        //print(moveVertical);
        Vector3 movement = new Vector3(0.0f, 0.0f, moveVertical);
        rb.AddForce(movement * speed);
    }

    void HandleExample_ControllerInput(ControllerMessage msg)
    {
        print(msg);
        if (msg.Payload.HasField("forward"))
        {
            moveVertical = msg.Payload.GetField("forward").f;
            print("field found!" + moveVertical);
            FixedUpdate();
        }
    }
	
}
