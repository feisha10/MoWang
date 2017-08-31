using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class ConfigManager:Singleton<ConfigManager> {

    const char SPLIT_FIELD='\t';

	private Dictionary<string, Dictionary<int, ConfigBase>> _allConfigs = new Dictionary<string, Dictionary<int, ConfigBase>>();
    private Dictionary<string, Dictionary<string, ConfigBase>> _allStringConfigs = new Dictionary<string, Dictionary<string, ConfigBase>>();

    private Dictionary<string, SOBase> _configs = new Dictionary<string, SOBase>();

    private ConfigData _commonData;

    private string GetUrl(Type type)
    {
        return type.Name.Substring(6);
    }

    public T GetConfig<T>(int id) where T : ConfigBase, new()
    {
		string url = GetUrl(typeof(T));
        if(_allConfigs.ContainsKey(url)==false)
        {
            _allConfigs[url] = new Dictionary<int,ConfigBase>();
        }
        Dictionary<int,ConfigBase> dic = _allConfigs[url];
        if(dic.ContainsKey(id))
        return dic[id] as T;

        var data = GetConfig(url) as ConfigData;
        if(data!=null)
        {
            var line = data.GetDataById(id);
            T t = new T();
            t.LoadConfigByLine(data,line);
            dic[id] = t;
            return t;
        }
		return null;
	}

    public T GetConfig<T>(string id) where T : ConfigBase, new()
    {
		string url = GetUrl(typeof(T));
        if(_allStringConfigs.ContainsKey(url)==false)
        {
            _allStringConfigs[url] = new Dictionary<string,ConfigBase>();
        }
        Dictionary<string,ConfigBase> dic = _allStringConfigs[url];
        if(dic.ContainsKey(id))
        return dic[id] as T;

        var data = GetConfig(url) as ConfigData;
        if(data!=null)
        {
            var line = data.GetDataByName(id);
            T t = new T();
            t.LoadConfigByLine(data,line);
            dic[id] = t;
            return t;
        }
		return null;
	}

    public T[] GetAllConfig<T>() where T:ConfigBase,new()
    {
		string url = GetUrl(typeof(T));
        var data = GetConfig(url) as ConfigData;
        if(data!=null)
        {
            int len = data._dicId.Count;
            T[] result = new T[len];

            if(_allConfigs.ContainsKey(url)==false)
            {
                _allConfigs[url] = new Dictionary<int,ConfigBase>();
            }

            Dictionary<int,ConfigBase> dic = _allConfigs[url];

            int i = 0;
            foreach(var line in data._dicId.Values)
            {
                int id = line.GetId();
                if(dic.ContainsKey(id))
                result[i] = dic[id] as T;
                else
                {
                    T t = new T();
                    t.LoadConfigByLine(data,line);
                    dic[id] = t;
                    result[i] = t;
                }
                i++;
            }

            return result;
        }
		return null;
    }

    public List<T> FindConfig<T>(Predicate<T> match) where T:ConfigBase, new()
    {
        List<T> result = new List<T>();
        T[] all = GetAllConfig<T>();
        for(int i=0;i<all.Length;i++)
        {
            if(match(all[i]))
            result.Add(all[i]);
        }
        return result;
    }

    public string GetCommmonValue(string key)
    {
        if(_commonData==null)
        _commonData = GetConfig("Common") as ConfigData;

        ConfigSheetLine line = _commonData.GetDataByName(key);
        if(line!=null)
        return (string)line.GetData(1,ConfigDataType.STRING);
        return null;
    }

    public SOBase GetConfig(string url)
   {
        if (_configs.ContainsKey(url)==false)
        {
            TextAsset asset = ResourcesManager.Instance.LoadAsset("Config/"+url) as TextAsset;

            using(var sr = new StringReader(asset.text))
            {

                var names = ReadLine(sr);
                var configData = ScriptableObject.CreateInstance<ConfigData>();
                configData.fieldNames = names;
                int column = names.Length;
                var types = ReadLine(sr);
                configData.fieldTypes = new int[column];
                configData.typeIndexs = new int[column];
                configData.InitData(types[0]);
                int intlen = 0;
                int stringlen = 0;
                int boollen = 0;
                int floatlen = 0;
                for(int i = 0;i < column; i++)
                {
                    string type = types[i].ToLower();
                    SetIndex(type,configData.typeIndexs,i,ref intlen,ref stringlen,ref boollen,ref floatlen);

                    switch(type)
                    {
                        case "int":
                        configData.fieldTypes[i] = ConfigDataType.INT;
                        break;
                        case "string":
                        configData.fieldTypes[i] = ConfigDataType.STRING;
                        break;
                        case "bool":
                        configData.fieldTypes[i] = ConfigDataType.BOOL;
                        break;
                        case "float":
                        configData.fieldTypes[i] = ConfigDataType.FLOAT;
                        break;
                        case "enum":
                        configData.fieldTypes[i] = ConfigDataType.ENUM;
                        break;
                        case "array":
                        configData.fieldTypes[i] = ConfigDataType.ARRAY;
                        break;
                        case "custom":
                        configData.fieldTypes[i] = ConfigDataType.CUSTOM;
                        break;

                    }
                }
                configData.InitLine(intlen,stringlen,boollen,floatlen);

                ReadLine(sr);

                string[] arr = null;
                int lineCount = 3;
                while((arr=ReadLine(sr))!=null)
                {
                    lineCount++;
                    var lineObj = new ConfigSheetLine();
                    lineObj.InitLine(intlen,stringlen,boollen,floatlen);
                    for(int i=0;i<column;i++)
                    {
                        try
                        {
                            lineObj.SetData(arr[i],configData.fieldTypes[i],configData.typeIndexs[i]);
                        }
                        catch (FormatException e)
                        {
                            throw new FormatException(url+" 数据格式转换错误：第"+lineCount+"行，第"+(i+1)+"列\r\n"+e.StackTrace);
                        }
                    }

                    bool b = configData.Add(lineObj);
                    if(b==false)
                    {
                        throw new Exception("重复的ID项：第"+lineCount+"行"+";"+url);
                    }
                }

                _configs[url] = configData;
            }
        }
        return _configs[url];
    }

    string[] ReadLine(StringReader sr)
    {
        string line = sr.ReadLine();
        if (!string.IsNullOrEmpty(line))
        {
            return line.Split(SPLIT_FIELD);
        }
        return null;
    }

    void SetIndex(string type, int[] typeIndexs, int i, ref int intlen, ref int stringlen,ref int boollen,ref int floatlen)
    {
        switch(type)
        {
            case "int":
            case "enum":
            typeIndexs[i] = intlen;
            intlen++;
            break;
            case "string":
            case "array":
            case "custom":
            typeIndexs[i] = stringlen;
            stringlen++;
            break;
            case "bool":
            typeIndexs[i] = boollen;
            boollen++;
            break;
            case "float":
            typeIndexs[i] = floatlen;
            floatlen++;
            break;
            default:
            typeIndexs[i] = stringlen;
            stringlen++;
            break;
        }
    }

}
