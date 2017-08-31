using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class ConfigSheetLine {
	public int[] lineInt;
    public string[] lineString;
    public bool[] lineBool;
    public float[] lineFloat;

    public void InitLine(int intlen,int stringlen,int boollen,int floatlen)
    {
        lineInt = new int[intlen];
        lineString = new string[stringlen];
        if(boollen>0)
        lineBool = new bool[boollen];
        if(floatlen>0)
        lineFloat = new float[floatlen];
    }

    public void SetData(string value, int type, int index)
    {
        if(type==ConfigDataType.INT || type==ConfigDataType.ENUM)
        lineInt[index] = string.IsNullOrEmpty(value)?0:int.Parse(value);
        else if(type==ConfigDataType.STRING)
        lineString[index] = value;
        else if(type==ConfigDataType.FLOAT)
        lineFloat[index] = string.IsNullOrEmpty(value)?0f:float.Parse(value);
        else if(type==ConfigDataType.BOOL)
        lineBool[index] = string.IsNullOrEmpty(value)?false:bool.Parse(value);
        else
        lineString[index] = value;
    }

    public object GetData(int field,int type)
    {
        if(field>=0)
        {
            if(type==ConfigDataType.INT || type==ConfigDataType.ENUM)
            {
                if(field < lineInt.Length)
                return lineInt[field];
            }
            else if(type==ConfigDataType.STRING)
            {
                if(field < lineString.Length)
                return lineString[field];
            }
            else if(type==ConfigDataType.BOOL)
            {
                if(field < lineBool.Length)
                return lineBool[field];
            }
            else if(type==ConfigDataType.FLOAT)
            {
                if(field < lineFloat.Length)
                return lineFloat[field];
            }
            else if(type==ConfigDataType.ARRAY)
            {
                if(field < lineString.Length)
                {
                    string temp = lineString[field];
                    string[] arr = string.IsNullOrEmpty(temp)? new string[0]:temp.Split(';');
                    return arr;
                }
            }
            else
            {
                if(field < lineString.Length)
                return lineString[field];
            }
        }
        return null;
    }

    public string GetKey()
    {
        return lineString[0];
    }

    public int GetId()
    {
        return lineInt[0];
    }
}
