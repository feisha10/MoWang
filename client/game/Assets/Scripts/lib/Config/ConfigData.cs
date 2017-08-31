using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigData :SOBase{
	public string[] fieldNames;
	public int[] fieldTypes;
	public int[] typeIndexs;
	public int intLen = 0;
	public int stringLen = 0;
	public int boolLen = 0;
	public int floatLen = 0;

	public Dictionary<int,ConfigSheetLine> _dicId;
	private Dictionary<string,ConfigSheetLine> _dicName;

	public void InitLine(int intlen, int stringlen,int boollen,int floatlen)
	{
		intLen = intlen;
		stringLen = stringlen;
		boolLen = boollen;
		floatLen = floatlen;
	}

	public void InitData(string type)
	{
		if(type=="int")
		{
			_dicId = new Dictionary<int,ConfigSheetLine>();
		}
		else
		{
			_dicName = new Dictionary<string,ConfigSheetLine>();
		}
	}

	public bool Add(ConfigSheetLine line)
	{
		#if UNITY_EDITOR
		if(_dicId!=null && _dicId.ContainsKey(line.GetId()))
		return false;
		if(_dicName!=null && _dicName.ContainsKey(line.GetKey()))
		return false;
		#endif
		if(_dicId!=null)
		_dicId[line.GetId()] = line;
		else
		_dicName[line.GetKey()] = line;
		return true;
	}

    private Dictionary<string,int> fieldNameDic;
	public int GetFieldByName(string fieldname)
	{
		if(fieldNameDic==null)
		{
			fieldNameDic = new Dictionary<string,int>(fieldNames.Length);
			for(int i=0;i<fieldNames.Length;i++)
			{
				fieldNameDic[fieldNames[i]] = i;
			}
		}
		int index;
		if(fieldNameDic.TryGetValue(fieldname,out index))
		return index;

		return -1;
	}

	public ConfigSheetLine GetDataById(int id)
	{
		if(_dicId.ContainsKey(id))
		return _dicId[id];

		return null;
	}

	public ConfigSheetLine GetDataByName(string name)
	{
		if(_dicName.ContainsKey(name))
		return _dicName[name];

		return null;
	}

	public int GetDataType(int field)
	{
		if(field>=0 && field<fieldTypes.Length)
		return fieldTypes[field];
		return 0;
	}

	public int GetTypeIndex(int field)
	{
		if(field>=0 && field<typeIndexs.Length)
		return typeIndexs[field];
		return 0;
	}
}
