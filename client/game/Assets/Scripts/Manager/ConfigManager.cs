using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class ConfigManager:Singleton<ConfigManager> {

	private Dictionary<string, Dictionary<int, ConfigBase>> _allConfigs = new Dictionary<string, Dictionary<int, ConfigBase>>();

    private Dictionary<string, string> _commonValue;

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

    public string GetCommmonValue(string key)
    {
//         if (_commonValue == null)
//         {
//             //ConfigData configData;

//             ConfigData configData = GetConfig("Common") as ConfigData;
// //            if (configs.ContainsKey("Common"))
// //            {
// //                configData = configs["Common"] as ConfigData;
// //                _commonValue = new Dictionary<string, string>();
// //            }
// //            else
// //            {
// //                return null;
// //            }

//             if (configData == null)
//                 return null;

//             _commonValue = new Dictionary<string, string>();

//             string tKey;
//             string tValue;
//             foreach (ConfigSheetLine line in configData.data)
//             {
//                 tKey = (string)line.GetData(1, ConfigDataType.STRING);
//                 tValue = (string)line.GetData(2, ConfigDataType.STRING);
//                 _commonValue.Add(tKey, tValue);              
//             }
//         }

//         if (_commonValue.ContainsKey(key))
//             return _commonValue[key];

        return null;
    }

//	public SOBase GetConfig(string url)
  //  {
        // if (configs.ContainsKey(url)==false)
        // {
        //     if (ConfigDicSource.ContainsKey(url) == false)
        //         return null;

        //     if (url == "WordFilter" || url == "WordFilterChat")
        //     {
        //         var asset = ConfigControl.ImportNormal(null, ConfigDicSource[url]);
        //         configs.Add(url, asset);
        //     }
        //     else if (url == "Guide" || url == "FightNovice")
        //     {
        //         var asset = ConfigControl.ImportXml(null, ConfigDicSource[url]);
        //         configs.Add(url, asset);
        //     }
        //     else
        //     {
        //         var asset = ConfigControl.ImportCsv(null, true, ConfigDicSource[url]);
        //         configs.Add(url, asset);
        //     }
        //     ConfigDicSource.Remove(url);
        // }
        // return configs[url];
    //}

	/// <summary>
    /// 增加对Excel的“Unicode 文本”格式中换行和双引号的解析支持
    /// </summary>
    /// <param name="sr"></param>
    /// <returns></returns>
    public static string[] ReadLine(StreamReader sr)
    {
		//StringReader rr;
		
        string line = sr.ReadLine();

        if (!string.IsNullOrEmpty(line))
        {
            bool end = false;
            while (end == false && line != null)
            {
                if (end && line == "")
                    line += "\r\n";
                else
                {
                    if (IsFullLine(line))
                        end = true;
                    else
                    {
                        string temp = sr.ReadLine();
                        line += "\r\n" + (temp ?? "");
                    }
                }
            }
            string[] columns = line.Split('\t');
            for (int i = 0; i < columns.Length; i++)
            {
                string temp = columns[i];
                if (temp.StartsWith("\""))
                {
                    temp = temp.Substring(1, temp.Length - 2);
                    temp = temp.Replace("\"\"", "\"");
                }
                columns[i] = temp;
            }
            return columns;
        }
        return null;
    }

	/// <summary>
    /// 判断一行是否还有后续行
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    private static bool IsFullLine(string line)
    {
        int index = line.LastIndexOf('\t') + 1;
        char[] chars = line.ToCharArray();
        if (index >= chars.Length || chars[index] != '\"')
            return true;
        else
        {
            int i = index + 1;
            while (i < chars.Length)
            {
                if (chars[i] == '\"')
                {
                    if (i + 1 < chars.Length && chars[i + 1] == '\"')
                        i++;
                    else
                        return true;
                }
                i++;
            }
            return false;
        }
    }

	    //ifCheckID判断是否不是逻辑的配置文件,false表示是逻辑的,true表示不是
    // public static SOBase ImportCsv(FileInfo file=null, bool ifCheckID = true, TextAsset textAsset = null)
    // {

    //     StreamReader sr = null;
    //     FileStream fs = null;
    //     MemoryStream textS = null;
    //     try
    //     {
	// 		string fileName = ""; 
    //         string text = "";
    //         fileName = textAsset.name;

    //             try
    //             {
    //                 if (!ifCheckID)
    //                 {
    //                     textS = new MemoryStream(textAsset.bytes);
    //                     sr = new StreamReader(textS, Encoding.UTF8);
    //                     text = sr.ReadToEnd();
    //                 }

    //                 textS = new MemoryStream(textAsset.bytes);
    //                 sr = new StreamReader(textS, Encoding.UTF8);
    //             }
    //             catch (IOException ex)
    //             {
    //                 throw new IOException("读取文件出错：" + fileName + "\r\n" + ex.StackTrace);
    //             }

            

    //         #region 解析列表名称和数据类型，忽略描述

    //         //列名
    //         ConfigData config = ScriptableObject.CreateInstance<ConfigData>();

    //         config.fieldNames = ReadLine(sr);
    //         config.text = text;
    //         int column = config.fieldNames.Length;

    //         Type classType=null;
    //         PropertyInfo[] propertyInfos=null;
    //         if (ifCheckID == false)
    //         {
    //             string className = "ConfigSerData" + fileName;
    //             classType = Type.GetType(className);
    //             propertyInfos = new PropertyInfo[column];
    //         }
    //         else
    //         {
    //             string className = "Config" + fileName;
    //             classType = Type.GetType(className);
    //             propertyInfos = new PropertyInfo[column];
    //         }

    //         //列类型
    //         string[] types = ReadLine(sr);
    //         config.fieldTypes = new int[column];
    //         for (int i = 0; i < column; i++)
    //         {
    //             if (i < types.Length)
    //             {
    //                 switch (types[i].ToLower())
    //                 {
    //                     case "int":
    //                         config.fieldTypes[i] = ConfigDataType.INT;
    //                         break;
    //                     case "bool":
    //                         config.fieldTypes[i] = ConfigDataType.BOOL;
    //                         break;
    //                     case "float":
    //                         config.fieldTypes[i] = ConfigDataType.FLOAT;
    //                         break;
    //                     case "Float":
    //                         config.fieldTypes[i] = ConfigDataType.FLOAT;
    //                         break;
    //                     case "long":
    //                         config.fieldTypes[i] = ConfigDataType.LONG;
    //                         break;
    //                     case "double":
    //                         config.fieldTypes[i] = ConfigDataType.DOUBLE;
    //                         break;
    //                     case "string":
    //                         config.fieldTypes[i] = ConfigDataType.STRING;
    //                         break;
    //                     case "enum":
    //                         config.fieldTypes[i] = ConfigDataType.ENUM;
    //                         break;
    //                     case "enumstring":
    //                         config.fieldTypes[i] = ConfigDataType.ENUMString;
    //                         propertyInfos[i] = classType.GetProperty(config.fieldNames[i]);
    //                         break;
    //                     case "array":
    //                         config.fieldTypes[i] = ConfigDataType.ARRAY;
    //                         break;
    //                     case "date":
    //                         config.fieldTypes[i] = ConfigDataType.DATETIME;
    //                         break;
    //                     case "custom":
    //                         config.fieldTypes[i] = ConfigDataType.CUSTOM;
    //                         break;
    //                     case "serverstring":
    //                         config.fieldTypes[i] = ConfigDataType.ServerString;
    //                         break;
    //                     case "serverarray":
    //                         config.fieldTypes[i] = ConfigDataType.ServerArray;
    //                         break;
    //                     default:
    //                         if (types[i].Contains("["))
    //                         {
    //                             config.fieldTypes[i] = ConfigDataType.ARRAY;
    //                         }
    //                         else
    //                         {
    //                             if (!fileName.Equals("SkillLevel"))
    //                             {
    //                                 Log.Error(fileName + "未识别的数据类型格式：\"" + types[i] + "\" 在第\"" + (i + 1) + "\"列");
    //                             }
    //                             config.fieldTypes[i] = ConfigDataType.UNKNOWN;
    //                         }
    //                         break;
    //                 }
    //             }
    //             else
    //             {
    //                 throw new Exception("列表错误，列数不对应。");
    //             }
    //         }

    //         //列描述，可忽略
    //         ReadLine(sr);

    //         #endregion

    //         #region 解析所有行数据

    //         //数据
    //         var list = new List<ConfigSheetLine>();
    //         string[] sArray = null;
    //         int lineCount = 3;
    //         while ((sArray = ReadLine(sr)) != null)
    //         {
    //             lineCount++;
    //             var lineObj = new ConfigSheetLine();
    //            // lineObj.line = new string[column];
    //             lineObj.InitLine(column);
    //             for (int i = 0; i < column; i++)
    //             {
    //                 try
    //                 {
    //                     lineObj.SetData(sArray[i], config.fieldTypes[i], i, config, propertyInfos, fileName, lineCount, i + 1);
    //                 }
    //                 catch (FormatException ex)
    //                 {
    //                     throw new FormatException(fileName + " 数据格式转换错误：第" + lineCount + "行，第" + (i + 1) + "列\r\n" +
    //                                               ex.StackTrace);
    //                 }
    //             }
    //             list.Add(lineObj);
    //         }
    //         config.data = list.ToArray();

    //         #endregion

    //         #region 检查是否有重复的id

    //         if (ifCheckID)
    //         {
    //             CheckRepeatedId(config, fileName);
    //         }

    //         #endregion

    //         return ConfigParse.transSysConfig(config, fileName);
    //     }
    //     finally
    //     {
    //         try
    //         {
    //             if (fs!=null)
    //               fs.Close();
    //             sr.Close();
    //             if (textS!= null)
    //                textS.Close();
    //         }
    //         catch
    //         {
    //         }
    //     }
    // }

}
