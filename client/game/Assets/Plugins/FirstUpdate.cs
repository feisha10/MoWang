using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Xml;
using System.Reflection;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class FirstUpdate : MonoBehaviour
{
    private string localUpdateDllVersion;
    private string urlUpdateDll;
    private string updateDllUrl;
    private bool webUpdate = false;
    XmlDocument doc; //把xml 库引用进来

    private string packVersion;
    private string apkid;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

#if UNITY_IOS
        SceneManager.LoadScene("IosStart", LoadSceneMode.Additive);
#else
        ReadTxt();
#endif
    }

    private void ReadTxt()
    {
        StartCoroutine(InitInfo());
    }

    IEnumerator  InitInfo()
    {
        string path = GetPath("version.txt");
        WWW www = new WWW(path);
        yield return www;
        string version = getWWWTxt(www);
        www = null;

        path = GetPath("gameDefine.txt");
        www = new WWW(path);
        yield return www;
        string gameDefine = getWWWTxt(www);
        www = null;

        path = GetStreamingAssetsPathNoTarget() + "version.txt";
        www = new WWW(path);
        yield return www;
        string localVersion = getWWWTxt(www);
        www = null;

        path = GetStreamingAssetsPathNoTarget() + "gameDefine.txt";
        www = new WWW(path);
        yield return www;
        string localGameDefine = getWWWTxt(www);
        www = null;

#if !UNITY_EDITOR
        string[] arr = ReadAllLines(version);
        foreach (string line in arr)
        {
            string[] row = line.Split('=');
            if (row.Length < 2)
                continue;
            string value = row[1].Trim();
            switch (row[0].Trim())
            {
                case "SecondVersion":
                    localUpdateDllVersion = value;
                    break;
            }
        }

        arr = ReadAllLines(localVersion);
        foreach (string line in arr)
        {
            string[] row = line.Split('=');
            if (row.Length < 2)
                continue;
            string value = row[1].Trim();
            switch (row[0].Trim())
            {
                case "version":
                    packVersion = value;
                    break;
            }
        }

        arr = ReadAllLines(localGameDefine);
        foreach (string line in arr)
        {
            string[] row = line.Split('=');
            if (row.Length < 2)
                continue;
            string value = row[1].Trim();
            switch (row[0].Trim())
            {
                case "id":
                    apkid = value;
                    break;
            }
        }

        arr = ReadAllLines(gameDefine);
        foreach (string line in arr)
        {
            string[] row = line.Split('=');
            if (row.Length < 2)
                continue;
            string value = row[1].Trim();
            switch (row[0].Trim())
            {
                case "update_url":
                    urlUpdateDll = value;
                    break;
            }
        }
#endif

        updateDllUrl = GetLocalUpdateDllPath();
        
#if !UNITY_EDITOR
        if (string.IsNullOrEmpty(urlUpdateDll))
#endif
        {
            DellUpdateDll();
        }
 #if !UNITY_EDITOR
        else
        {
            StartCoroutine(WorkerFunction());
        }
#endif
    }

    string getWWWTxt(WWW www)
    {
        string result = null;
        if (www.isDone && string.IsNullOrEmpty(www.error))
        {
            result = www.text;

            //Debug.Log("WWW Value:" + result);
        }
        else
        {
            result = "";
            Debug.LogError("loadError:" + www.error);
        }

        www.Dispose();

        return result;
    }

   Dictionary<string, string> UpdateUrlDic = new Dictionary<string, string>();

    IEnumerator WorkerFunction()
    {
        WWW www = new WWW(string.Format("{0}check.txt?id={1}", urlUpdateDll, DateTime.Now.ToFileTime() / 10));
        yield return www;

        if (www.isDone && string.IsNullOrEmpty(www.error))
        {
            string txt = www.text;
            if (string.IsNullOrEmpty(txt)==false)
            {
                InitCheck(txt);

                string key = apkid + "_" + packVersion;
                if (UpdateUrlDic.ContainsKey(key))
                    urlUpdateDll = UpdateUrlDic[key];
            }
        }
        else
        {
            Debug.LogError("loadError：" + www.error);
        }
        www.Dispose();

        www = new WWW(string.Format("{0}SecondVersion.txt?id={1}", urlUpdateDll, DateTime.Now.ToFileTime() / 10));
        yield return www;
        if (www.isDone && string.IsNullOrEmpty(www.error))
        {
            string serverVersion = www.text;
            Debug.Log("serverVersion" + serverVersion);
            if (serverVersion != localUpdateDllVersion && serverVersion != "0")
            {
                updateDllUrl = string.Format("{0}updatework.dll?id={1}", urlUpdateDll, DateTime.Now.ToFileTime() / 10);
                webUpdate = true;
            }
        }
        else
        {
            Debug.LogError("loadError：" + www.error);
        }
        www.Dispose();
        www = null;
        DellUpdateDll();
    }

    void InitCheck(string txt)
    {
        string[] allLines = ReadAllLines(txt);
        foreach (string line in allLines)
        {
            string[] row = line.Split(';');

            if (row.Length >= 2)
                UpdateUrlDic[row[0]] = row[1];
        }
    }

    private void DellUpdateDll()
    {
        StartCoroutine(DellUpdateDllRead());
    }

    IEnumerator DellUpdateDllRead()
    {
        Debug.Log("url:" + updateDllUrl);
        WWW www = new WWW(updateDllUrl);
        yield return www;
        if (www.isDone && string.IsNullOrEmpty(www.error))
        {
            if (webUpdate)
            {
                if (!Directory.Exists(OutDir + "Managed"))
                {
                    try
                    {
                        Directory.CreateDirectory(OutDir + "Managed");
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);

                        RemoveDir(OutDir);

                        Directory.CreateDirectory(OutDir + "Managed");
                    }
                }

                File.WriteAllBytes(Path.Combine(OutDir, "Managed/updatework.dll"), www.bytes);
            }

            byte[] asset = null;

            byte[] bytes = www.bytes;
            _MaskData(bytes);
            AssetBundle assetBundle = AssetBundle.LoadFromMemory(bytes);

            Object o = assetBundle.LoadAsset("updatework.dll.bytes");
            asset = (o as TextAsset).bytes;

            Assembly assembly = System.Reflection.Assembly.Load(asset);
            System.Type script = assembly.GetType("UpdateWork.UpdateWork");
            Debug.Log("Load Game Script Complete...");

            assetBundle.Unload(false);
            assetBundle = null;

            www.Dispose();
            www = null;

            gameObject.AddComponent(script);
        }
        else
        {
            Debug.LogError("DellUpdateDllRead" + www.error);
        }
    }

    private string GetPath(string fileName)
    {
        string path = Path.Combine(OutDir, fileName);
        if (!File.Exists(path))
        {
            path = GetStreamingAssetsPathNoTarget() + fileName;
        }
        else
        {
            path = "file:///" + path;
        }
        return path;
    }

    private string GetLocalUpdateDllPath()
    {
#if !UNITY_EDITOR
        string outPath = Path.Combine(OutDir, "Managed/updatework.dll");
        if (File.Exists(outPath))
        {
            outPath = "file:///" + outPath;
            return outPath;
        }
        else
#endif
        {
            return GetStreamingAssetsPathNoTarget() + "Managed/updatework.dll";
        }
    }

    void RemoveDir(string path)
    {
#if !UNITY_WEBPLAYER
        try
        {

            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }
        catch (IOException e)
        {
            Debug.LogException(e);
        }
#endif
    }

    private string OutDir
    {
        get
        {
            return Path.Combine(Application.persistentDataPath, "out/");
        }
    }

    void _MaskData(byte[] data)
    {
        uint mask = BitConverter.ToUInt32(data, 0);
        for (int i = 4; i < data.Length; i++)
        {
            int m = i % 4;
            if (m == 0)
                mask = mask * 1103515245 + 12345;
            data[i] ^= (byte)(mask >> (m * 8));
        }
    }

    private string[] ReadAllLines(string txt)
    {
        if (string.IsNullOrEmpty(txt))
            return new string[0];
        return txt.Split("\r\n".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
    }

    private string GetStreamingAssetsPathNoTarget()
    {
        string s;
#if UNITY_EDITOR
        s = string.Format("file://{0}/StreamingAssets/", Application.dataPath);
#elif UNITY_IOS  
        s = string.Format("file://{0}/Raw/", Application.dataPath);
#elif UNITY_ANDROID
        s = string.Format("jar:file://{0}!/assets/", Application.dataPath);
#else
        s = string.Format("file://{0}/StreamingAssets/", Application.dataPath);
#endif
        return s;
    }

}
