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
		public ActionTextFader(Text element, float fadeTime, float startTime) : base(element, fadeTime, startTime) {
			string[] possibleTextValues = {"Lets Go","Stroke It","Forward"};
	
			element.transform.eulerAngles = new Vector3(0f, 0f, Random.Range(-10f,10f));
			element.text = possibleTextValues[Random.Range(0,possibleTextValues.Length)];
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
	
	public void addActionTextFader(string textKey, float startTime, float fadeTime) {
		Text element = uiCanvas.transform.Find (textKey).GetComponent<Text> ();
		element.enabled = true;
		TextFader fader = new ActionTextFader (element, fadeTime, startTime);
		currentFaders.Add (fader);
	}

	public void addTextFader(string textKey, float startTime, float fadeTime) {
		Text element = uiCanvas.transform.Find (textKey).GetComponent<Text> ();
		element.enabled = true;
		TextFader fader = new TextFader (element, fadeTime, startTime);
		currentFaders.Add (fader);
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