using UnityEngine;
using System.Collections.Generic;
using Utility;
using BladeCast;

public class BoatUserControl : MonoBehaviour 
{
	public float speed;
	public float torque;
	
	private Rigidbody rb;

	private List<Player>         m_players = new List<Player> ();

	private Animator leftOarAnimator,
		rightOarAnimator;

	private Transform camera;

	void Start()
	{  
		rb = GetComponent<Rigidbody>();
		leftOarAnimator = transform.Find("LeftOar").GetComponent<Animator> ();
		rightOarAnimator = transform.Find("RightOar").GetComponent<Animator> ();
		camera = transform.parent.Find ("MainCamera");
		//BCMessenger.Instance.RegisterListener("connect", 0, this.gameObject, "HandleConnection");      
		//BCMessenger.Instance.RegisterListener("start_race", 0, this.gameObject, "HandleStartRace");  
	}
	
	// Update is called once per frame
	void Update () 
	{
		//float moveHorizontal = Input.GetAxis ("Horizontal");
		//float moveVertical = Input.GetAxis ("Vertical");
		bool leftPaddlePressed = Input.GetKeyDown (KeyCode.N);
		bool rightPaddlePressed = Input.GetKeyDown (KeyCode.M);
		float rowSpeed = 1.0f;

		if (leftOarAnimator.GetCurrentAnimatorStateInfo().IsName("Inactive")){

		}
		if (leftPaddlePressed) {

			//this.transform.Rotate (new Vector3 (0, -1, 0));
			rb.AddTorque(transform.up * torque * rowSpeed, ForceMode.Acceleration);
			leftOarAnimator.SetFloat ("RowSpeed", 0.2f);
		} else {
			leftOarAnimator.SetFloat ("RowSpeed", 0.0f);
		}

		if (rightPaddlePressed) {
			//this.transform.Rotate (new Vector3(0,1,0));nn
			rb.AddTorque(transform.up * -torque * rowSpeed, ForceMode.Acceleration);
			rightOarAnimator.SetFloat ("RowSpeed", 0.2f);
		} else {
			rightOarAnimator.SetFloat ("RowSpeed", 0.0f);
		}

		camera.transform.position = this.transform.position + new Vector3 (0f, 8f, -5.5f);
		//Vector3 movement = new Vector3 (moveHorizontal, 0.0f, moveVertical);

		//rb.AddRelativeForce (movement * speed);
	}
	
	// message handlers...
	private void HandleConnection(ControllerMessage msg)
	{
		// index of new hand
		int controllerIndex = msg.ControllerSource;     
		
		Player player = m_players.Find (x => x.ControllerIndex == controllerIndex);
		if (player == null) 
		{
			player = CreatePlayer (controllerIndex);
			SetPlayerPos(player);
		} 
	}
	
	private void HandleStartRace(ControllerMessage msg)
	{
	}

	private Player CreatePlayer(int controllerIndex)
	{
		Player racer = new Player();//(Player)Instantiate(m_racerPrototype, Vector3.zero, Quaternion.identity);
		racer.transform.SetParent (this.transform);
		racer.gameObject.name = "racer_player_" + controllerIndex.ToString ();
		racer.ControllerIndex = controllerIndex;
		
		//m_players.Add(Player);
		return racer;
	}

	private void SetPlayerPos(Player racer)
	{
		//Vector3 pos = k_firstRacerPos + k_racerOffset * (float)(racer.ControllerIndex - 1);
		//racer.transform.localPosition = pos;
	}
}
