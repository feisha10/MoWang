using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTimer {

	private float _gamePassTime = 0;
	private float _delaySaveTime = 0;

	public void Init()
	{

		_gamePassTime = LocalSave.Instance.GetFloat(LocalKey.GameTimer);

	}

	public void Update()
	{
		_gamePassTime+=Time.deltaTime;
		_delaySaveTime+=Time.deltaTime;
		if(_delaySaveTime>=Const.SaveTime)
		{
			_delaySaveTime = 0;
			LocalSave.Instance.SetFloat(LocalKey.GameTimer,_gamePassTime);
		}
	}
}
