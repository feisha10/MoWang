using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ConfigManager:Singleton<ConfigManager> {

	private Dictionary<string, Dictionary<int, ConfigBase>> _allConfigs = new Dictionary<string, Dictionary<int, ConfigBase>>();

    private string GetUrl(Type type)
    {
        return type.Name.Substring(6);
    }
    public T GetConfig<T>(int id) where T : ConfigBase, new()
    {
		string url = GetUrl(typeof(T));
		if(string.IsNullOrEmpty(url)==false)
		{

		}
		return null;
	}

}
