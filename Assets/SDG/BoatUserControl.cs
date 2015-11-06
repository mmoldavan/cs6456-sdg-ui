using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Utility;
using BladeCast;

public class BoatUserControl : MonoBehaviour 
{
	public float speed;
	public float torque;
	public bool enableKeyboardInput;
	public UIController uiController;
	
	private Rigidbody rb;

	private List<Player>         m_players = new List<Player> ();

	private Animator leftOarAnimator, rightOarAnimator, boatAnimator;

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
		private Animator leftOarAnimator, rightOarAnimator, boatAnimator;
		private Rigidbody rb;
		private float baseSpeed;
		private BoatUserControl userControl;

		public PaddleInput(BoatUserControl userControl, Rigidbody rb, Animator leftOar, Animator rightOar, Animator boat, float speed){
			this.active = false;
			this.userControl = userControl;
			this.leftOarAnimator = leftOar;
			this.rightOarAnimator = rightOar;
			this.boatAnimator = boat;
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
						userControl.notifyUIofPaddle(player, paddleBuffer);
					}
				} else {
					active = true;
					initiatingPlayer = player;
					pressTime = Time.time;
					paddleSpeed = speed;
					userControl.notifyUIofPaddle(player, paddleBuffer);
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
			boatAnimator.SetFloat ("RowSpeed", (leftSpeed + rightSpeed) / 2);
			boatAnimator.SetTrigger ("MoveForward");
			leftOarAnimator.SetFloat ("RowSpeed", leftSpeed);
			leftOarAnimator.SetTrigger ("RowSlow");
			rightOarAnimator.SetFloat ("RowSpeed", rightSpeed);
			rightOarAnimator.SetTrigger ("RowSlow");
		}

		public void paddleLeft(float speed) {
			leftOarAnimator.SetFloat ("RowSpeed", speed);
			leftOarAnimator.SetTrigger ("RowSlow");
			boatAnimator.SetFloat ("RowSpeed", speed);
			boatAnimator.SetTrigger ("MoveLeft");
		}

		public void paddleRight(float speed) {
			rightOarAnimator.SetFloat ("RowSpeed", speed);
			rightOarAnimator.SetTrigger ("RowSlow");
			boatAnimator.SetFloat ("RowSpeed", speed);
			boatAnimator.SetTrigger ("MoveRight");
		}
	}

	public void notifyUIofPaddle(PlayerRole player, float buffer) {
		if (player == PlayerRole.LEFTPADDLER) {
			uiController.addTextFader ("LeftPlayer/LeftPaddle", Time.time, buffer);
		} else {
			uiController.addTextFader ("RightPlayer/RightPaddle", Time.time, buffer);
		}
	}

	private class JumpInput: PendingInput {
		public void motionReceived(PlayerRole player, float speed) {

		}
		public void update() {

		}
	}

	//Game Init
	void Start()
	{  
		rb = GetComponent<Rigidbody>();
		leftOarAnimator = transform.Find("LeftOar").GetComponent<Animator> ();
		rightOarAnimator = transform.Find("RightOar").GetComponent<Animator> ();
		boatAnimator = GetComponent<Animator> ();
		camera = transform.parent.Find ("MainCamera");

		//Set up EAPathFinder listeners
		BCMessenger.Instance.RegisterListener("connect", 0, this.gameObject, "HandleConnection");      
		BCMessenger.Instance.RegisterListener("stroke", 0, this.gameObject, "HandlePaddleStoke");  
		BCMessenger.Instance.RegisterListener("jump", 0, this.gameObject, "HandleJump");  

		//Set up possible actions.
		paddleInput = new PaddleInput (this, rb, leftOarAnimator, rightOarAnimator, boatAnimator, speed);
		jumpInput = new JumpInput ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		paddleInput.update ();
		jumpInput.update ();

		if (enableKeyboardInput) {
			if(Input.GetKeyDown(KeyCode.N)) {
				paddleInput.motionReceived(PlayerRole.LEFTPADDLER, 1.0f);
			}
			if(Input.GetKeyDown(KeyCode.M)) {
				paddleInput.motionReceived(PlayerRole.RIGHTPADDLER, 1.0f);
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
