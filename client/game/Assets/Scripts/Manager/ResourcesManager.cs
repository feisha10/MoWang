using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesManager:Singleton<ResourcesManager> {

	public Object LoadAsset(string path)
	{
		return Resources.Load(path);
	}
}
