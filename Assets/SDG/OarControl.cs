using UnityEngine;
using System.Collections;

public class OarControl : MonoBehaviour {
	public BoatUserControl boatControl;
	private ParticleSystem splashParticleSystem;

	public void Start ()
	{
		splashParticleSystem = this.transform.GetComponentInChildren<ParticleSystem> ();
	}

	public void CreateWaterSplash()
	{
		splashParticleSystem.Play ();
	}

	public void leftOarFinished()
	{
		boatControl.doNextLeftOarAction ();
	}

	public void rightOarFinished()
	{
		boatControl.doNextRightOarAction ();

	}
}
