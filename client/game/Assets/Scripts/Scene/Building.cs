using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building {

	private ConfigBuilding _configBuilding;
	private Room[] _rooms;
	private int _roomLen;

	private Master _master;

	public void Init()
	{
		_master = new Master();
		_configBuilding = InitTestData();
		InitRoom();
	}

	void InitRoom()
	{
		int len = _configBuilding.rooms.Length;
		_rooms = new Room[len];
		for(int i=0;i<len;i++)
		{
			var room = new Room();
			var config = new ConfigRoom();
			config.id = _configBuilding.rooms[i];
			room.Init(config);
			_rooms[i] = room;
		}
		_roomLen = len;
	}

	ConfigBuilding InitTestData()
	{
		var result = new ConfigBuilding();
		result.id = 1;
		result.name = "house one";
		result.lev = 1;
		result.levUpMoney = 100;
		result.rooms = new int[4]{1101,1102,1103,1104};
		return result;
	}

	public void Update()
	{
		if(_rooms!=null)
		{
			for(int i=0;i<_roomLen;i++)
			{
				_rooms[i].Update();
			}
		}
	}
}
