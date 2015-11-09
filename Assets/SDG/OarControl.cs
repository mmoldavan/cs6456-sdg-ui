using UnityEngine;
using System.Collections;

public class OarControl : MonoBehaviour {
	public BoatUserControl boatControl;
	
	public void CreateWaterSplash()
	{
		
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
