using UnityEngine;
using System.Collections;

public class Log :Singleton<Log>{
	public static void Debug(string msg)
	{
		UnityEngine.Debug.Log(msg);
	}
}
