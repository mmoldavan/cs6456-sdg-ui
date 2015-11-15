using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIController : MonoBehaviour {
	public float leftPaddleStart;
	public float rightPaddleStart;
	public Canvas uiCanvas;
	public List<TextFader> currentFaders = new List<TextFader> ();
	
	public class TextFader {
		public Text element;
		public float fadeTime;
		public float startTime;
		public bool active;

		public TextFader(Text element, float fadeTime, float startTime){
			this.active = true;
			this.element = element;
			this.fadeTime = fadeTime;
			this.startTime = startTime;
		}

		public void update() {
			float timeDiff = (Time.time - startTime) / fadeTime;
			if (timeDiff > fadeTime) {
				Color color = element.color;
				element.color = new Color(color.r, color.g, color.b, 1.0f);
				element.enabled = false;
				active = false;
			}
			else {
				float alpha = (fadeTime - timeDiff) / fadeTime;
				Color color = element.color;
				element.color = new Color(color.r, color.g, color.b, alpha);
			}
		}
	}

	public class ActionTextFader : TextFader {
		public ActionTextFader(Text element, string textValue, float fadeTime, float startTime) : base(element, fadeTime, startTime) {
	
			element.transform.eulerAngles = new Vector3(0f, 0f, Random.Range(-10f,10f));
			element.text = textValue;
		}


		public void update() {
			float timeDiff = (Time.time - startTime) / fadeTime;
			if (timeDiff > fadeTime) {
				Color color = element.color;
				element.color = new Color(color.r, color.g, color.b, 1.0f);
				element.enabled = false;
				active = false;
			}
			else {
				float alpha = (fadeTime - timeDiff) / fadeTime;
				Color color = element.color;
				element.color = new Color(color.r, color.g, color.b, alpha);
			}
		}
	}
	
	public void Start() {
		uiCanvas = GameObject.Find ("Canvas").GetComponent<Canvas> ();
	}
	
	public void addPaddleActionTextFader(string textKey, float paddleSpeed, float startTime, float fadeTime) {
		Text element = uiCanvas.transform.Find (textKey).GetComponent<Text> ();
		element.enabled = true;
		string textVal;
		if (paddleSpeed <= .5) {
			textVal = "slow";
		} else if (paddleSpeed > .5 && paddleSpeed <= .75) {
			textVal = "paddling";
		} else {
			textVal = "fast";
		}
	
		TextFader fader = new ActionTextFader (element, textVal, fadeTime, startTime);
		currentFaders.Add (fader);
	}
	public void addJumpActionTextFader(string textKey, string jumpState, float startTime, float fadeTime) {
		Text element = uiCanvas.transform.Find (textKey).GetComponent<Text> ();
		element.enabled = true;
		string otherPlayer = textKey == "LeftPlayer/LeftPaddle" ? "P2" : "P1";
		string textVal;
		if (jumpState.Equals("init")) {
			textVal = "jump started";
		} else if (jumpState.Equals("receive")) {
			textVal = otherPlayer + " jump";
		} else {
			textVal = "jumping!";
		}
		
		TextFader fader = new ActionTextFader (element, textVal, fadeTime, startTime);
		currentFaders.Add (fader);
	}

	public void addTextFader(string textKey, float startTime, float fadeTime) {
		Text element = uiCanvas.transform.Find (textKey).GetComponent<Text> ();
		element.enabled = true;
		TextFader fader = new TextFader (element, fadeTime, startTime);
		currentFaders.Add (fader);
	}

	public void changeRoleText(string textKey, string newRole) {
		Debug.Log (textKey + " " + newRole);
		uiCanvas.transform.Find (textKey+"/PlayerRole").GetComponent<Text> ().text = "Role: "+ newRole;
	}

	public void Update() {
		updateLapTimer ();

		foreach (TextFader fader in currentFaders.FindAll (x => x.active == true)) {
			fader.update ();
		}

		foreach (TextFader fader in currentFaders.FindAll (x => x.active == false)) {
			currentFaders.Remove(fader);
		}

	}

	public void updateLapTimer() {
		Text elapsedTimer = uiCanvas.transform.Find ("Timer/ElapsedTimer").GetComponent<Text> ();
		Text recordTimer = uiCanvas.transform.Find ("Timer/RecordTimer").GetComponent<Text> ();
		elapsedTimer.text = RaceManager.getRaceManager ().getCurrentLapTime ();
		recordTimer.text = RaceManager.getRaceManager ().getBestLapTime ();
	}
}