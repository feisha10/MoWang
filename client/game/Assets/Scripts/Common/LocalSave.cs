using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalSave:Singleton<LocalSave> {

	public void SetString(string key,string value)
	{
		PlayerPrefs.SetString(key,value);
		PlayerPrefs.Save();
	}

    public void SetInt(string key,int value)
	{
		PlayerPrefs.SetInt(key,value);
		PlayerPrefs.Save();
	}

    public void SetFloat(string key,float value)
	{
		PlayerPrefs.SetFloat(key,value);
		PlayerPrefs.Save();
	}

	public string GetString(string key)
	{
		return PlayerPrefs.GetString(key);
	}

    public int GetInt(string key)
	{
		return PlayerPrefs.GetInt(key);
	}

    public float GetFloat(string key)
	{
		return PlayerPrefs.GetFloat(key);
	}
}
