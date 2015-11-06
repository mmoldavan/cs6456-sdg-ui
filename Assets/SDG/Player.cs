using UnityEngine;
using System.Collections;
using System;
using BladeCast;

public enum PlayerRole {
	LEFTPADDLER = 1,
	RIGHTPADDLER = 2,
	NAVIGATOR = 3
}

public class Player
{
	public PlayerRole role;

	private int m_controllerIndex = -1;

	public int ControllerIndex
	{
		get
		{
			return m_controllerIndex;
		}
		
		set
		{
			// set index and color...
			if (m_controllerIndex != value)
			{
				m_controllerIndex = value;
			}
		}
	}
}
