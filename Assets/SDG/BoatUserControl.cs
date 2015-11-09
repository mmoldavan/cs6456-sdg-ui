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
	private ControlMode controlMode;

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

	public enum ControlMode {
		TWOPADDLER = 1,
		NAVANDPADDLER = 2
	}

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
				userControl.notifyUIofPaddle(player, 0.5f);
				if(userControl.leftPaddleSpeed == 0.0f) {
					userControl.doNextLeftOarAction();
				}
			} else if (player == PlayerRole.RIGHTPADDLER) {
				userControl.nextRightAction = speed;
				userControl.notifyUIofPaddle(player, 0.5f);
				if(userControl.rightPaddleSpeed == 0.0f) {

					userControl.doNextRightOarAction();
				}
			} else if (player == PlayerRole.FULLPADDLER) {
				userControl.nextFullAction = speed;
				userControl.notifyUIofPaddle(player, 0.5f);
				if(userControl.rightPaddleSpeed == 0.0f) {
					userControl.doNextFullOarAction();
				}
			}
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
		boatAnimator.SetFloat ("ForwardSpeed", currentForwardSpeed);
		boatAnimator.SetFloat ("RotationSpeed", currentRotationSpeed);
		boatAnimator.SetFloat ("AnimationSpeed", Mathf.Max (currentForwardSpeed, Mathf.Abs(currentRotationSpeed)));
		leftOarAnimator.SetFloat ("RowSpeed", leftPaddleSpeed);
		rightOarAnimator.SetFloat ("RowSpeed", rightPaddleSpeed);
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
		rightPaddleSpeed = nextFullAction;
		leftPaddleSpeed = nextFullAction;
		nextRightAction = 0f;
		updateForwardSpeed ();
		updateAnimators ();
	}

	public void doNextRutterAction() {
		currentRotationSpeed = nextNavigationAction;
		updateAnimators ();
	}

	public void updateBoatSpeeds () {
		updateForwardSpeed ();
		currentRotationSpeed =  rightPaddleSpeed - leftPaddleSpeed;
	}

	public void updateForwardSpeed() {
		nextForwardSpeed = (leftPaddleSpeed + rightPaddleSpeed) / 2;
		forwardTransitionStartTime = Time.time;
	}

	public void lerpForwardSpeedTransition() {
		if (nextForwardSpeed >= 0.0f) {
			float timeSinceTranstitionStart = Time.time - forwardTransitionStartTime;
			float lerpTime = timeSinceTranstitionStart / animationTransitionTime;
			if(lerpTime > 1.0f || currentForwardSpeed == nextForwardSpeed) {
				boatAnimator.SetFloat("ForwardSpeed", nextForwardSpeed);
				currentForwardSpeed = nextForwardSpeed;
				nextForwardSpeed = -1.0f;
			}
			else {
				float newForwardValue = Mathf.Lerp(currentForwardSpeed,nextForwardSpeed,lerpTime);
				boatAnimator.SetFloat("ForwardSpeed", Mathf.Lerp(currentForwardSpeed,nextForwardSpeed,lerpTime));
				boatAnimator.SetFloat ("AnimationSpeed", Mathf.Max (newForwardValue, Mathf.Abs(currentRotationSpeed)));
			}
		}
	}

	public void notifyUIofPaddle(PlayerRole player, float buffer) {
		if (player == PlayerRole.LEFTPADDLER) {
			uiController.addActionTextFader ("LeftPlayer/LeftPaddle", Time.time, buffer);
		} else {
			uiController.addActionTextFader ("RightPlayer/RightPaddle", Time.time, buffer);
		}
	}

	//Game Init
	void Start()
	{  
		//rb = GetComponent<Rigidbody>();
		leftOarAnimator = transform.Find("LeftOar").GetComponent<Animator> ();
		rightOarAnimator = transform.Find("RightOar").GetComponent<Animator> ();
		boatAnimator = GetComponent<Animator> ();

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
	}
	
	// Update is called once per frame
	void Update () 
	{
		jumpInput.update ();

		if (enableKeyboardInput) {
			if(Input.GetKeyDown(KeyCode.N)) {
				paddleInput.motionReceived(PlayerRole.LEFTPADDLER, 1.0f);
			}
			if(Input.GetKeyDown(KeyCode.M)) {
				paddleInput.motionReceived(PlayerRole.RIGHTPADDLER, 1.0f);
			}
			if(Input.GetKeyDown(KeyCode.R)) { //Fast Reload
				Application.LoadLevel (Application.loadedLevelName);
			}
		}

		lerpForwardSpeedTransition ();
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

	private void HandleRoleChange(ControllerMessage msg)
	{
		int controllerIndex = msg.ControllerSource;     
		Player player = m_players.Find (x => x.ControllerIndex == controllerIndex);
		
		Debug.Log (msg.Payload);
		
		if (player != null) 
		{
			player.role = PlayerRole.NAVIGATOR;

		}

	}

	private void HandleNavDirection(ControllerMessage msg)
	{
		int controllerIndex = msg.ControllerSource;     
		Player player = m_players.Find (x => x.ControllerIndex == controllerIndex);
		
		Debug.Log (msg.Payload);
		
		if (player != null) 
		{
			player.role = PlayerRole.NAVIGATOR;
			navigatorInput.motionReceived(player.role, 1.0f);
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
