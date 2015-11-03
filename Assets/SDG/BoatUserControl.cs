using UnityEngine;
using System.Collections.Generic;
using Utility;
using BladeCast;

public class BoatUserControl : MonoBehaviour 
{
	public float speed;
	
	private Rigidbody rb;

	private List<Player>         m_players = new List<Player> ();

	void Start()
	{  
		rb = GetComponent<Rigidbody>();
		//BCMessenger.Instance.RegisterListener("connect", 0, this.gameObject, "HandleConnection");      
		//BCMessenger.Instance.RegisterListener("start_race", 0, this.gameObject, "HandleStartRace");  
	}
	
	// Update is called once per frame
	void Update () 
	{
		float moveHorizontal = Input.GetAxis ("Horizontal");
		float moveVertical = Input.GetAxis ("Vertical");
		bool leftPaddlePressed = Input.GetKeyDown (KeyCode.N);
		bool rightPaddlePressed = Input.GetKeyDown (KeyCode.M);

		if (leftPaddlePressed) {
			this.transform.Rotate (new Vector3(0,0,-1));
		}

		if (rightPaddlePressed) {
			this.transform.Rotate (new Vector3(0,0,1));
		}

		Vector3 movement = new Vector3 (moveHorizontal, -moveVertical, 0.0f);

		Debug.Log (movement);
		rb.AddRelativeForce (movement * speed);
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
