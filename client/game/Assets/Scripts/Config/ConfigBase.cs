using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

public class ConfigBase {
	public int id;
	public string name;

	public void LoadConfigByLine(ConfigData data, ConfigSheetLine content)
    {
        if (data != null && content != null)
        {
            FieldInfo[] fields = GetType().GetFields();
            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo fi = fields[i];

                int fieldIndex = data.GetFieldByName(fi.Name);
                if (fieldIndex != -1)
                {
                    int defineType = data.GetDataType(fieldIndex);
					int typeindex = data.GetTypeIndex(fieldIndex);
                    object ovalue = content.GetData(typeindex, defineType);

                    switch (defineType)
                    {
                        case ConfigDataType.INT:
                        case ConfigDataType.ENUM:
                        case ConfigDataType.BOOL:
                        case ConfigDataType.FLOAT:
                        case ConfigDataType.STRING:
                            fi.SetValue(this, ovalue);
                            break;
                        case ConfigDataType.ARRAY:      
                            Type type = fi.FieldType;
                            Type stype = type.GetElementType();
                            if (type.IsArray && (stype == typeof(int) || stype == typeof(string) || stype == typeof(float)))
                                DecodeArrayValue(fi, ovalue, stype);
                            else
                                DecodeSpecialValue(fi, ovalue);
                            break;
                        case ConfigDataType.CUSTOM:
                            //交由各个具体的子列去实现解析
                            DecodeSpecialValue(fi, ovalue);
                            break;
                    }
                }
            }
        }
    }
	private void DecodeArrayValue(FieldInfo fi, object value, Type arrayType)
    {
        var svalue = (string[]) value;
        if (arrayType == typeof(int))
        {
            var ivalue = new int[svalue.Length];
            for (int i = 0; i < svalue.Length; i++)
            {
                ivalue[i] = int.Parse(svalue[i]);
            }
            fi.SetValue(this, ivalue);
        }
        else if (arrayType == typeof(string))
        {
            fi.SetValue(this, svalue);
        }
        else if (arrayType == typeof (float))
        {
            var fvalue = new float[svalue.Length];
            for (int i = 0; i < svalue.Length; i++)
            {
                fvalue[i] = float.Parse(svalue[i]);
            }
            fi.SetValue(this, fvalue);
        }
    }

	protected virtual void DecodeSpecialValue(FieldInfo fi, object value)
    {
        throw new Exception("需要子类实现特定数据的解析");
    }
}
