using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Utility;
using BladeCast;

public class BoatUserControl : MonoBehaviour 
{
	//Vars set in Unity
	public float speed;
	public float torque;
	public bool enableKeyboardInput;
	public UIController uiController;

	private List<Player>         m_players = new List<Player> ();
	private Animator leftOarAnimator, rightOarAnimator, boatAnimator;

	//Input Handlers
	private PaddleInput paddleInput;
	private JumpInput jumpInput;
	private NavigatorInput navigatorInput;

	private class PendingInput {
		public float pressTime;
		public PlayerRole initiatingPlayer;
		public bool active;
	}

	private class PaddleInput: PendingInput {
		public float paddleBuffer = 0.5f;
		public float paddleSpeed; 
		private Animator leftOarAnimator, rightOarAnimator, boatAnimator;
		private BoatUserControl userControl;

		public PaddleInput(BoatUserControl userControl){
			this.active = false;
			this.userControl = userControl;
			this.leftOarAnimator = userControl.leftOarAnimator;
			this.rightOarAnimator = userControl.rightOarAnimator;
			this.boatAnimator = userControl.boatAnimator;
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
		public JumpInput(BoatUserControl userControl){
		}

		public void motionReceived(PlayerRole player, float speed) {

		}
		public void update() {

		}
	}

	private class NavigatorInput: PendingInput {
		public NavigatorInput(BoatUserControl userControl){
		}

		public void motionReceived(PlayerRole player, float speed) {
			
		}
		public void update() {
			
		}
	}

	//Game Init
	void Start()
	{  
		//rb = GetComponent<Rigidbody>();
		leftOarAnimator = transform.Find("LeftOar").GetComponent<Animator> ();
		rightOarAnimator = transform.Find("RightOar").GetComponent<Animator> ();
		boatAnimator = GetComponent<Animator> ();
		//camera = transform.parent.Find ("MainCamera");

		//Set up EAPathFinder listeners
		BCMessenger.Instance.RegisterListener("connect", 0, this.gameObject, "HandleConnection");      
		BCMessenger.Instance.RegisterListener("stroke", 0, this.gameObject, "HandlePaddleStroke");  
		BCMessenger.Instance.RegisterListener("jump", 0, this.gameObject, "HandleJump");  

		//Set up possible actions.
		paddleInput = new PaddleInput (this);
		jumpInput = new JumpInput (this);
		navigatorInput = new NavigatorInput (this);
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

		//camera.transform.position = this.transform.position + new Vector3 (0f, 7f, -9f);
	}
	
	// message handlers...
	private void HandleConnection(ControllerMessage msg)
	{
		// index of new hand
		int controllerIndex = msg.ControllerSource; 
		Debug.Log (controllerIndex);

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

		Debug.Log (msg.Payload);
		Debug.Log (player);

		if (player != null) 
		{
			paddleInput.motionReceived(player.role, 1.0f);
		}
	}

	private void HandleJump(ControllerMessage msg)
	{
		int controllerIndex = msg.ControllerSource;     
		Player player = m_players.Find (x => x.ControllerIndex == controllerIndex);
		
		Debug.Log (msg.Payload);
		
		if (player != null) 
		{
			paddleInput.motionReceived(player.role, 1.0f);
		}
	}

	private Player CreatePlayer(int controllerIndex)
	{
		Player player = new Player();
		player.ControllerIndex = controllerIndex;
		Debug.Log (player);

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
