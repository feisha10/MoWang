using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster {

	private ConfigMonster _configMonster;
	public void Init(int id)
	{
		_configMonster = InitTestData(id);
	}

	ConfigMonster InitTestData(int id)
	{
		var result =new ConfigMonster();
		result.id = id;
		return result;
	}
}
