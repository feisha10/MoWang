using UnityEngine;
public static class UIPlugin  {

	#region Create UI Script
	static string controlUiDeclaraStr = "";
    static string controlUiGetStr = "";

	private static void GetUiPrefabContorlInfo(Transform trans,string fullpath)
	{
		string transName = trans.name;
		if(transName.EndsWith("cr"))
		{
			string startName=transName.Split('_')[0];
            startName = startName.ToLower();
			switch(startName)
			{
				case "go":
				    MakeControlUiStr(transName,fullpath,"GameObject");
				break;
                case "text":
				    MakeControlUiStr(transName,fullpath,"Text");
                break;
				case "btn":
				    MakeControlUiStr(transName,fullpath,"Button");
                break;
				case "image":
				    MakeControlUiStr(transName,fullpath,"Image");
                break;
			}
		}
	}

	private static void MakeControlUiStr(string transName,string fullpath,string typeName)
	{
		controlUiDeclaraStr += string.Format("{0}private {1} {2};{3}", "\t", typeName, transName, "\r\n");
		if(typeName=="GameObject")
		{
			controlUiGetStr += string.Format("{0}{0}{1} = cachedTransform.FindChild(\"{2}\").gameObject;{3}", "\t", transName, fullpath, "\r\n");
		}
		else
		{
			controlUiGetStr += string.Format("{0}{0}{1} = cachedTransform.FindChild(\"{2}\").GetComponent<{3}>();{4}", "\t", transName, fullpath,typeName, "\r\n");
		}
	}

	#endregion

}
