using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Utility;
using BladeCast;

public class BoatUserControl : MonoBehaviour 
{
	//Vars set in Unity
	public float jumpForce = 50f;
	public float keyboardInputSpeed = 1.0f;
	public bool enableKeyboardInput;
	public UIController uiController;

	private List<Player>         m_players = new List<Player> ();
	private Animator leftOarAnimator, rightOarAnimator, boatAnimator;
	private Transform rutter;
	private ControlMode controlMode;
	private Rigidbody rigidbody;

	//Input Handlers
	private PaddleInput paddleInput;
	private JumpInput jumpInput;
	private NavigatorInput navigatorInput;

	private float nextLeftAction;
	private float nextRightAction;
	private float nextFullAction;
	private float nextNavigationAction;

	public float currentRotationSpeed;
	public float currentForwardSpeed;
	public float nextForwardSpeed; //For Lerping between the speeds on transition
	public float forwardTransitionStartTime;
	public float leftPaddleSpeed;
	public float rightPaddleSpeed;

	public float animationTransitionTime = 0.33f;

	private ControlMode currentMode;

	public enum ControlMode {
		TWOPADDLER = 1,
		NAVANDPADDLER = 2
	}

	public enum JumpState { INITIATOR, COMPLETE };
	
	private class PendingInput {
		public float pressTime;
		public PlayerRole initiatingPlayer;
		public bool active;
	}

	private class PaddleInput: PendingInput {
		private BoatUserControl userControl;

		public PaddleInput(BoatUserControl userControl){
			this.active = false;
			this.userControl = userControl;
		}

		public void motionReceived(PlayerRole player, float speed) {
			if (player == PlayerRole.LEFTPADDLER) {
				userControl.nextLeftAction = speed;
				userControl.notifyUIofPaddle(player, speed, 0.5f);
				if(userControl.leftPaddleSpeed == 0.0f) {
					userControl.doNextLeftOarAction();
				}
			} else if (player == PlayerRole.RIGHTPADDLER) {
				userControl.nextRightAction = speed;
				userControl.notifyUIofPaddle(player, speed, 0.5f);
				if(userControl.rightPaddleSpeed == 0.0f) {

					userControl.doNextRightOarAction();
				}
			} else if (player == PlayerRole.FULLPADDLER) {
				userControl.nextFullAction = speed;
				userControl.notifyUIofPaddle(player, speed, 0.5f);
				if(userControl.rightPaddleSpeed == 0.0f) {
					userControl.doNextFullOarAction();
				}
			}
		}
	}

	private class JumpInput: PendingInput {
		float jumpRecieveBuffer = 1f;

		private BoatUserControl userControl;

		public JumpInput(BoatUserControl userControl){
			this.active = false;
			this.userControl = userControl;

		}

		public void motionReceived(PlayerRole player, float speed) {
			if (active) { //jump is active
				if (player != initiatingPlayer) {
					if ( pressTime > Time.time - jumpRecieveBuffer) {
						//complete jump
						userControl.leftOarAnimator.SetTrigger("Jump");
						userControl.rightOarAnimator.SetTrigger("Jump");
						userControl.notifyUIofJump(player, JumpState.COMPLETE, jumpRecieveBuffer);
						active = false;
					}
					else {
						initiateJump(player);
					}
				} else {
					//same player has reinitiated a jump
					initiateJump(player);
				}
			} else {
				//first jump initiation
				initiateJump(player);
			}
		}
		private void initiateJump(PlayerRole player) {
			pressTime = Time.time;
			initiatingPlayer = player;
			userControl.notifyUIofJump(player, JumpState.INITIATOR, jumpRecieveBuffer);
			active = true;
			int sender = 1;
			int receiver = 2;
			if (player == PlayerRole.RIGHTPADDLER || player == PlayerRole.NAVIGATOR) {
				sender = 2;
				receiver = 1;
			}
			ControllerMessage jumpMsg = new ControllerMessage(sender, receiver, "jump_initiated", new JSONObject());
			BCMessenger.Instance.SendToListeners(jumpMsg);
		}
		public void update() {
			
		}
	}
	
	private class NavigatorInput: PendingInput {
		private BoatUserControl userControl;
		
		public NavigatorInput(BoatUserControl userControl){
			this.userControl = userControl;
		}
		
		public void motionReceived(PlayerRole player, float speed) {
			if (player == PlayerRole.NAVIGATOR) {
				userControl.nextNavigationAction = speed;
				userControl.doNextRutterAction();
			}
		}
	}

	public void updateAnimators () {
		float absRotationSpeed = Mathf.Abs (currentRotationSpeed);
		float animationSpeed = Mathf.Max (currentForwardSpeed, absRotationSpeed);
		// Actual 0s on any of these values will cause Not a Number exceptions in the animator controllers.
		if (animationSpeed < 0.1f) {
			animationSpeed = 0.09f;
		}

		//boatAnimator.SetFloat ("ForwardSpeed", currentForwardSpeed);
		boatAnimator.SetFloat ("RotationSpeed", currentRotationSpeed);
		boatAnimator.SetFloat ("AnimationSpeed", animationSpeed);
		leftOarAnimator.SetFloat ("RowSpeed", leftPaddleSpeed);
		rightOarAnimator.SetFloat ("RowSpeed", rightPaddleSpeed);

		setRutterPosition ();
	}

	public void setRutterPosition () {
		if (currentMode == ControlMode.NAVANDPADDLER) {
			rutter.localEulerAngles = new Vector3 (0, currentRotationSpeed * 45, 0);
		}
	}

	public void jumpAfterWaterHit() {
		rigidbody.AddForce(Vector3.up * jumpForce,ForceMode.Impulse);
	}

	public void doNextLeftOarAction() {
		leftPaddleSpeed = nextLeftAction;
		nextLeftAction = 0f;
		updateBoatSpeeds ();
		updateAnimators ();
	}

	public void doNextRightOarAction() {
		rightPaddleSpeed = nextRightAction;
		nextRightAction = 0f;
		updateBoatSpeeds ();
		updateAnimators ();
	}

	public void doNextFullOarAction() {
		if (currentMode == ControlMode.NAVANDPADDLER) {
			rightPaddleSpeed = nextFullAction;
			leftPaddleSpeed = nextFullAction;
			nextFullAction = 0f;
			updateForwardSpeed ();
			updateAnimators ();
		}
	}

	public void doNextRutterAction() {
		currentRotationSpeed = nextNavigationAction;
		updateAnimators ();
	}

	public void updateBoatSpeeds () {
		updateForwardSpeed ();
		if(currentMode == ControlMode.TWOPADDLER) {
			currentRotationSpeed =  rightPaddleSpeed - leftPaddleSpeed;
		}
	}

	public void updateForwardSpeed() {
		currentForwardSpeed = boatAnimator.GetFloat ("ForwardSpeed");
		nextForwardSpeed = (leftPaddleSpeed + rightPaddleSpeed) / 2;
		//Debug.Log ("left speed:" + leftPaddleSpeed + "; right speed:" + rightPaddleSpeed + "; forwardNext:"+nextForwardSpeed +"; currentForward:"+currentForwardSpeed);
		forwardTransitionStartTime = Time.time;
	}

	public void lerpForwardSpeedTransition() {
		if (nextForwardSpeed > currentForwardSpeed) {
			//Acceleration should be fast
			lerpItForward(0.25f);
		}
		else if (nextForwardSpeed >= 0.0f) {
			//deceleration should coast a little.
			lerpItForward(animationTransitionTime);
		}
	}

	public void lerpItForward(float lerpTransitionTime) {
		float timeSinceTranstitionStart = Time.time - forwardTransitionStartTime;
		float lerpTime = timeSinceTranstitionStart / lerpTransitionTime;
		if(lerpTime >= 1.0f || currentForwardSpeed == nextForwardSpeed) {
			boatAnimator.SetFloat("ForwardSpeed", nextForwardSpeed);
			
			currentForwardSpeed = nextForwardSpeed;
			nextForwardSpeed = -1.0f;
		}
		else {
			float newForwardValue = Mathf.Lerp(currentForwardSpeed,nextForwardSpeed,lerpTime);
			float absRotationSpeed = Mathf.Abs (currentRotationSpeed);
			float animationSpeed = Mathf.Max (newForwardValue, absRotationSpeed);
			if (animationSpeed < 0.1f) {
				animationSpeed = 0.09f;
			}
			boatAnimator.SetFloat ("ForwardSpeed", newForwardValue);
			if (boatAnimator.GetFloat("AnimationSpeed") < animationSpeed) {
				boatAnimator.SetFloat ("AnimationSpeed", animationSpeed);
			}
		}
	}

	public void notifyUIofPaddle(PlayerRole player, float paddleSpeed, float buffer) {
		if (player == PlayerRole.LEFTPADDLER) {
			uiController.addPaddleActionTextFader ("LeftPlayer/LeftPaddle", paddleSpeed, Time.time, buffer);
		} else {
			uiController.addPaddleActionTextFader ("RightPlayer/RightPaddle", paddleSpeed, Time.time, buffer);
		}
	}

	public void notifyUIofJump(PlayerRole player, JumpState jState, float buffer) {
		if (jState == JumpState.INITIATOR) {
			if (player == PlayerRole.LEFTPADDLER) {
				uiController.addJumpActionTextFader ("LeftPlayer/LeftPaddle", "init", Time.time, buffer);
				uiController.addJumpActionTextFader ("RightPlayer/RightPaddle", "receive", Time.time, buffer);
			} else {
				uiController.addJumpActionTextFader ("RightPlayer/RightPaddle", "init", Time.time, buffer);
				uiController.addJumpActionTextFader ("LeftPlayer/LeftPaddle", "receive", Time.time, buffer);
			}
		} else {
			uiController.addJumpActionTextFader("LeftPlayer/LeftPaddle", "complete", Time.time, buffer);
			uiController.addJumpActionTextFader ("RightPlayer/RightPaddle", "complete", Time.time, buffer);
		}
	}

	public void keepBoatUpright() {
		//Vector3 rotation = this.transform.rotation.eulerAngles;
		Quaternion rot = rigidbody.rotation;
		rot[0] = 0; //null rotation X
		rot[2] = 0; //null rotation Z
		rigidbody.rotation = rot;
		//this.transform.rotation.eulerAngles = rotation;

	}

	//Game Init
	void Start()
	{  
		//rb = GetComponent<Rigidbody>();
		leftOarAnimator = transform.Find("LeftOar").GetComponent<Animator> ();
		rightOarAnimator = transform.Find("RightOar").GetComponent<Animator> ();
		boatAnimator = GetComponent<Animator> ();
		rutter = transform.Find ("Rutter").transform;
		rigidbody = this.GetComponent<Rigidbody> ();

		//Set up EAPathFinder listeners
		BCMessenger.Instance.RegisterListener("connect", 0, this.gameObject, "HandleConnection");      
		BCMessenger.Instance.RegisterListener("stroke", 0, this.gameObject, "HandlePaddleStroke");  
		BCMessenger.Instance.RegisterListener("jump", 0, this.gameObject, "HandleJump");  
		BCMessenger.Instance.RegisterListener("direction", 0, this.gameObject, "HandleNavDirection");  
		BCMessenger.Instance.RegisterListener("role_change", 0, this.gameObject, "HandleRoleChange");  

		//Set up possible actions.
		paddleInput = new PaddleInput (this);
		jumpInput = new JumpInput (this);
		navigatorInput = new NavigatorInput (this);

		//init speeds
		currentForwardSpeed = 0.0f;
		currentRotationSpeed = 0.0f;
		leftPaddleSpeed = 0.0f;
		rightPaddleSpeed = 0.0f;
		nextLeftAction = 0.0f;
		nextRightAction = 0.0f;
		nextFullAction = 0.0f;
		nextNavigationAction = 0.0f;

		currentMode = ControlMode.TWOPADDLER;
	}
	
	// Update is called once per rendered frame
	void Update () 
	{
		jumpInput.update ();

		if (enableKeyboardInput) {
			if(Input.GetKeyDown(KeyCode.N)) {
				paddleInput.motionReceived(PlayerRole.LEFTPADDLER, keyboardInputSpeed);
			}
			if(Input.GetKeyDown(KeyCode.M)) {
				paddleInput.motionReceived(PlayerRole.RIGHTPADDLER, keyboardInputSpeed);
			}
			if(Input.GetKeyDown(KeyCode.J)) {
				jumpInput.motionReceived(PlayerRole.LEFTPADDLER, keyboardInputSpeed);
			}
			if(Input.GetKeyDown(KeyCode.K)) {
				jumpInput.motionReceived(PlayerRole.RIGHTPADDLER, keyboardInputSpeed);
			}
			if(Input.GetKeyDown(KeyCode.B)) {
				paddleInput.motionReceived(PlayerRole.FULLPADDLER, keyboardInputSpeed);
			}
			if(Input.GetKeyDown(KeyCode.H)) {
				jumpInput.motionReceived(PlayerRole.FULLPADDLER, keyboardInputSpeed);
			}
			if(Input.GetKeyDown(KeyCode.R)) { //Fast Reload
				Application.LoadLevel (Application.loadedLevelName);
			}
		}
	}

	// Physics update at fixed intervals
	public void FixedUpdate () {
		lerpForwardSpeedTransition ();
		keepBoatUpright ();
	}
	
	// message handlers...
	private void HandleConnection(ControllerMessage msg)
	{
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

		//Debug.Log (msg.Payload);
		//Debug.Log (player);
		
		float speed = msg.Payload.GetField ("value").f;

		if (player != null) 
		{
			paddleInput.motionReceived(player.role, speed);
		}
	}

	private void HandleJump(ControllerMessage msg)
	{
		int controllerIndex = msg.ControllerSource;     
		Player player = m_players.Find (x => x.ControllerIndex == controllerIndex);
		
		Debug.Log (msg.Payload);
		string jumpAction = msg.Payload.GetField ("value").str;
		
		if (player != null && jumpAction == "start") 
		{
			jumpInput.motionReceived(player.role, 1.0f);
		}
	}

	private void HandleRoleChange(ControllerMessage msg)
	{
		int controllerIndex = msg.ControllerSource;     
		Player player = m_players.Find (x => x.ControllerIndex == controllerIndex);
		
		//Debug.Log (msg.Payload);

		string newRole = "";
		newRole = msg.Payload.GetField ("value").str;
		if (newRole == "navigator") {
			currentMode = ControlMode.NAVANDPADDLER;
			player.role = PlayerRole.NAVIGATOR;
			Player player2 = m_players.Find (x => x.role <= PlayerRole.FULLPADDLER);
			if(player2 != null) {
				player2.role = PlayerRole.FULLPADDLER;
				uiController.changeRoleText("RightPlayer", "navigator");
				uiController.changeRoleText("LeftPlayer", "paddler");
			}
		} else {
			currentMode = ControlMode.TWOPADDLER;
			player.role = PlayerRole.LEFTPADDLER;
			Player player2 = m_players.Find (x => x.role <= PlayerRole.FULLPADDLER);
			if(player2 != null) {
				player2.role = PlayerRole.RIGHTPADDLER;
			}
		}

	}

	private void HandleNavDirection(ControllerMessage msg)
	{
		int controllerIndex = msg.ControllerSource;     
		Player player = m_players.Find (x => x.ControllerIndex == controllerIndex);

		float rotation = msg.Payload.GetField("value").f;

		if (player != null) 
		{
			navigatorInput.motionReceived(player.role, rotation);
		}
	}

	private Player CreatePlayer(int controllerIndex)
	{
		Player player = new Player ();
		player.ControllerIndex = controllerIndex;
		Debug.Log (player);
		if (currentMode == ControlMode.TWOPADDLER) {
			if (m_players.Find (x => x.role == PlayerRole.LEFTPADDLER) == null) {
				player.role = PlayerRole.LEFTPADDLER;
				m_players.Add (player);
			} else if (m_players.Find (x => x.role == PlayerRole.RIGHTPADDLER) == null) {
				player.role = PlayerRole.RIGHTPADDLER;
				m_players.Add (player);
			}
		} else {
			if (m_players.Find (x => x.role == PlayerRole.FULLPADDLER) == null) {
				player.role = PlayerRole.FULLPADDLER;
				m_players.Add (player);
			} else if (m_players.Find (x => x.role == PlayerRole.NAVIGATOR) == null) {
				player.role = PlayerRole.NAVIGATOR;
				m_players.Add (player);
			}
		}
		return player;
	}
}
