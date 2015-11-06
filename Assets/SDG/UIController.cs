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
	}
	
	public void Start() {
		uiCanvas = GameObject.Find ("Canvas").GetComponent<Canvas> ();
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
			float timeDiff = (Time.time - fader.startTime) / fader.fadeTime;
			if (timeDiff > fader.fadeTime) {
				Color color = fader.element.color;
				fader.element.color = new Color(color.r, color.g, color.b, 1.0f);
				fader.element.enabled = false;
				fader.active = false;
			}
			else {
				float alpha = (fader.fadeTime - timeDiff) / fader.fadeTime;
				Color color = fader.element.color;
				fader.element.color = new Color(color.r, color.g, color.b, alpha);

			}
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