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

	private Animator leftOarAnimator, rightOarAnimator;

	private Transform camera;

	private PaddleInput paddleInput;
	private JumpInput jumpInput;

	private class PendingInput {
		public float pressTime;
		public PlayerRole initiatingPlayer;
		public bool active;
	}

	private class PaddleInput: PendingInput {
		public float paddleBuffer = 5.0f;
		public float paddleSpeed; 

		public PaddleInput(){
			active = false;
		}

		public void motionReceived(PlayerRole player, float speed) {
			if (player < PlayerRole.NAVIGATOR) {
				if (active) {
					if (player != initiatingPlayer) {
						paddleForward ();
						active = false;
					}
				} else {
					active = true;
					initiatingPlayer = player;
					pressTime = Time.time;
					paddleSpeed = speed;
				}
			}
		}

		public void update() {
			if (active) {
				if (pressTime < Time.time - paddleBuffer) {
					if (initiatingPlayer == PlayerRole.LEFTPADDLER) {
						paddleLeft(paddleSpeed);
					}
					else {
						paddleRight(paddleSpeed);
					}
				}
			}
		}

		public void paddleForward(float leftSpeed, float rightSpeed) {
			Vector3 movement = new Vector3 (0.0f, 0.0f, (leftSpeed + rightSpeed / 2));
			
			rb.AddRelativeForce (movement * speed);
			paddleLeft (float leftSpeed);
			paddleRight (float rightSpeed);
		}

		public void paddleLeft(float speed) {
			leftOarAnimator.SetFloat ("RowSpeed", speed);
		}

		public void paddleRight(float speed) {
			rightOarAnimator.SetFloat ("RowSpeed", speed);
		}
	}

	private class JumpInput: PendingInput {
	}

	//Animation Event Handlers
	public void leftOarApplyRotation(){
		rb.AddTorque(transform.up * torque * 1, ForceMode.Acceleration);
	}
	public void rightOarApplyRotation(){
		rb.AddTorque(transform.up * -torque * 1, ForceMode.Acceleration);
	}
	public void leftOarApplyForward(){

	}
	public void rightOarApplyForward(){

	}

	//Game Init
	void Start()
	{  
		rb = GetComponent<Rigidbody>();
		leftOarAnimator = transform.Find("LeftOar").GetComponent<Animator> ();
		rightOarAnimator = transform.Find("RightOar").GetComponent<Animator> ();
		camera = transform.parent.Find ("MainCamera");

		//Set up EAPathFinder listeners
		BCMessenger.Instance.RegisterListener("connect", 0, this.gameObject, "HandleConnection");      
		BCMessenger.Instance.RegisterListener("stroke", 0, this.gameObject, "HandlePaddleStoke");  
		BCMessenger.Instance.RegisterListener("jump", 0, this.gameObject, "HandleJump");  

		//Set up possible actions.
		paddleInput = new PaddleInput ();
		jumpInput = new JumpInput ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		paddleInput.update ();
		jumpInput.update ();

		camera.transform.position = this.transform.position + new Vector3 (0f, 8f, -5.5f);
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
		} 
	}
	
	private void HandlePaddleStroke(ControllerMessage msg)
	{
		int controllerIndex = msg.ControllerSource;     
		Player player = m_players.Find (x => x.ControllerIndex == controllerIndex);

		if (player != null) 
		{
			paddleInput.motionReceived(player.role)
		}
	}

	private Player CreatePlayer(int controllerIndex)
	{
		Player player = new Player();

		if (m_players.Find (x => x.role == PlayerRole.LEFTPADDLER) == null) {
			player.role = PlayerRole.LEFTPADDLER;
			m_players.Add(player);
		} else if (m_players.Find (x => x.role == PlayerRole.RIGHTPADDLER) == null) {
			player.role = PlayerRole.RIGHTPADDLER;
			m_players.Add(player);
		}

		return player;
	}
}
