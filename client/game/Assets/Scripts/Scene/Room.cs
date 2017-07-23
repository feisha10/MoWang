using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room {

	ConfigRoom _configRoom;
	private int _currSatisfaction; //当前满意度
	private int _currRent; //当前房租

	public void Init(ConfigRoom config)
	{
		_configRoom = config;
	}

	public void Update()
	{
		
	}
}
