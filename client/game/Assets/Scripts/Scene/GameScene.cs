using UnityEngine;
using System.Collections;

public class GameScene {

    GameTimer timer;

	Building building;

	Monster[] _allMonster;
	int _allMonsLen = 10;
	public void Init()
	{
		timer = new GameTimer();
		timer.Init();
		building = new Building();
		building.Init();
	}

	void InitTestMonster()
	{
		_allMonster = new Monster[_allMonsLen];
		for(int i=0;i<_allMonsLen;i++)
		{
			var monster = new Monster();
			int monsterid = i+1;
			monster.Init(monsterid);
			_allMonster[i] = monster;
		}
	}

	public void Update()
	{
		timer.Update();
		building.Update();
	}

}
