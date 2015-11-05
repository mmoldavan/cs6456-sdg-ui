using UnityEngine;
using System.Collections.Generic;
using Utility;
using BladeCast;

public class BoatUserControl : MonoBehaviour 
{
	public float speed;
	public float torque;
	public bool enableKeyboardInput;
	
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
		public float paddleBuffer = 0.5f;
		public float paddleSpeed; 
		private Animator leftOarAnimator, rightOarAnimator;
		private Rigidbody rb;
		private float baseSpeed;

		public PaddleInput(Rigidbody rb, Animator leftOar, Animator rightOar, float speed){
			this.active = false;
			this.leftOarAnimator = leftOar;
			this.rightOarAnimator = rightOar;
			this.baseSpeed = speed;
			this.rb = rb;
		}

		public void motionReceived(PlayerRole player, float speed) {
			if (player < PlayerRole.NAVIGATOR) {
				if (active) {
					if (player != initiatingPlayer) {
						if(player == PlayerRole.LEFTPADDLER) {
							paddleForward (speed, paddleSpeed);
						} else {
							paddleForward (paddleSpeed, speed);
						}
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
					active = false;
				}
			}
		}

		public void paddleForward(float leftSpeed, float rightSpeed) {
			Vector3 movement = new Vector3 (0.0f, 0.0f, (leftSpeed + rightSpeed / 2));

			rb.AddRelativeForce (movement * baseSpeed);
			paddleLeft (leftSpeed);
			paddleRight (rightSpeed);
		}

		public void paddleLeft(float speed) {
			Debug.Log ("move left:");
			//leftOarAnimator.SetFloat ("RowSpeed", speed);
			leftOarAnimator.SetTrigger ("RowSlow");
		}

		public void paddleRight(float speed) {
			Debug.Log ("move right:");
			//rightOarAnimator.SetFloat ("RowSpeed", speed);
			rightOarAnimator.SetTrigger ("RowSlow");
		}
	}

	private class JumpInput: PendingInput {
		public void motionReceived(PlayerRole player, float speed) {

		}
		public void update() {

		}
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
		paddleInput = new PaddleInput (rb, leftOarAnimator, rightOarAnimator, speed);
		jumpInput = new JumpInput ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		paddleInput.update ();
		jumpInput.update ();

		if (enableKeyboardInput) {
			if(Input.GetKeyDown(KeyCode.N)) {
				paddleInput.motionReceived(PlayerRole.LEFTPADDLER, 0.3f);
			}
			if(Input.GetKeyDown(KeyCode.M)) {
				paddleInput.motionReceived(PlayerRole.RIGHTPADDLER, 0.3f);
			}
		}

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
			paddleInput.motionReceived(player.role, 0.3f);
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
