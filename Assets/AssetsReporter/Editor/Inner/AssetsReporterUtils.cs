using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine;
using UnityEditor;


public class AssetsReporterUtils{
	public static StringBuilder AddJsonObject(StringBuilder sb, string key, string val)
	{
		sb.Append(key).Append(":\"").Append(val).Append('"');
		return sb;
	}
	public static StringBuilder AddJsonObjectArray(StringBuilder sb, string key, string[] arr)
	{
		sb.Append(key).Append(":[");
		bool isFirst = true;
		if (arr != null)
		{
			foreach (var val in arr)
			{
				if (isFirst) { isFirst = false; }
				else { sb.Append(','); }
				sb.Append('"').Append(val).Append('"');
			}
		}
		sb.Append("]");
		return sb;
	}
	public static StringBuilder AddJsonObject(StringBuilder sb, string key, float val)
	{
		sb.Append(key).Append(":\"").Append(val).Append('"');
		return sb;
	}
	public static StringBuilder AddJsonObject(StringBuilder sb, string key, bool val)
	{
		sb.Append(key).Append(":");
		if (val)
		{
			sb.Append("true");
		}
		else
		{
			sb.Append("false");
		}
		return sb;
	}
	public static StringBuilder AddJsonObject(StringBuilder sb, string key, int val)
	{
		sb.Append(key).Append(":").Append(val);
		return sb;
	}
	public static void AddCountVarObject<T>(StringBuilder sb, string varname, Dictionary<T, int> set)
	{
		sb.Append(varname).Append("=[");
		bool isFirst = true;
		foreach (var format in set)
		{
			if (isFirst)
			{
				isFirst = false;
			}
			else
			{
				sb.Append(',');
			}
			sb.Append("{val:");
			sb.Append('"').Append(format.Key.ToString()).Append('"');
			sb.Append(",cnt:").Append('"').Append(format.Value).Append('"');
			sb.Append("}");
		}
		sb.Append("];\n");
	}
	public static bool IsPathMatch(string path, List<string> list)
	{
		if (list == null)
		{
			return false;
		}
		foreach (var pattern in list)
		{
			Regex reg = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
			if (reg.IsMatch(path))
			{
				return true;
			}
		}
		return false;
	}

	public static void AddCountDictionary<T>(Dictionary<T, int> dict, T key)
	{
		if (!dict.ContainsKey(key))
		{
			dict.Add(key, 1);
			return;
		}
		dict[key] += 1;
	}
	public static void AddCurrenTimeVar(StringBuilder sb)
	{
		var now = DateTime.Now;
		sb.Append("g_report_at=\"");
        sb.Append(now.ToString());
		sb.Append("\";\n");
	}
	public static void AddPlatformVar(StringBuilder sb,string platform)
	{
		sb.Append("g_report_platform=\"");
		sb.Append(platform);
		sb.Append("\";\n");
	}

	public static void SaveTexture2d(string path, Texture2D tex)
	{
		if (tex == null) { return; }
		File.WriteAllBytes(path, tex.EncodeToPNG());
	}

	public static string SaveModelPreview(ModelImporter importer,GameObject obj)
	{
		string guid = AssetDatabase.AssetPathToGUID(importer.assetPath);
		string file = guid + ".png";

		//					var gmo = GameObject.Instantiate(obj);
		var tex = AssetPreview.GetAssetPreview(obj);

		tex = AssetPreview.GetAssetPreview(obj);
		for (int i = 0; i < 100; ++i)
		{
			if (obj != null && tex == null)
			{
				tex = AssetPreview.GetAssetPreview(obj);
				System.Threading.Thread.Sleep(10);
			}
		}
		AssetsReporterUtils.SaveTexture2d("AssetsReporter/result/preview/" + file, tex);
		return file;
	}

	public static void CreatePreviewDir()
	{
		string dir = "AssetsReporter/result/preview";
		Directory.CreateDirectory(dir);
	}

	public static int GetPolygonNum(GameObject obj)
	{
		int num = 0;
		MeshFilter[] meshFilters = obj.GetComponentsInChildren<MeshFilter>(true);
		foreach (var meshFileter in meshFilters)
		{
			if (meshFileter.sharedMesh == null) { continue; }
			num += meshFileter.sharedMesh.vertexCount;
		}
		SkinnedMeshRenderer[] skinRenders = obj.GetComponentsInChildren<SkinnedMeshRenderer>(true);
		foreach (var skinRender in skinRenders)
		{
			if (skinRender.sharedMesh == null) { continue; }
			num += skinRender.sharedMesh.vertexCount;
		}
		return num;
	}

	public static StringBuilder AddJsonObjectArrayWithout(StringBuilder sb, string key, string[] arr,string without)
	{
		sb.Append(key).Append(":[");
		bool isFirst = true;
		if (arr != null)
		{
			foreach (var val in arr)
			{
				if (val == without) { continue; }
				if (isFirst) { isFirst = false; }
				else { sb.Append(','); }
				sb.Append('"').Append(val).Append('"');
			}
		}
		sb.Append("]");
		return sb;
	}

    public static void OpenURL(string url)
    {
#if UNITY_EDITOR_WIN
        Application.OpenURL(url);
#else
        System.Diagnostics.Process.Start( "file:///" + System.IO.Directory.GetCurrentDirectory() + "/" + url);        
#endif
    }

}

