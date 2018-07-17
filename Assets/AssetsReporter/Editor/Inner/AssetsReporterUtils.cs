using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine;
using UnityEditor;
#if UNITY_2017_3_OR_NEWER
using UnityEngine.Rendering;
#endif


public class AssetsReporterUtils{
    public const string ResultDir = "AssetsReporter/result/";
    public const string PreviewDir = "AssetsReporter/result/preview/";

    public static void WriteReportLanguage(string lang)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("g_language=\"");
        sb.Append( lang );
        sb.Append("\";\n");

        File.WriteAllText(ResultDir + "report_language.js", sb.ToString());

    }

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

    public static StringBuilder AddJsonToContDictionary<T>(StringBuilder sb, string key, Dictionary<T, int> set)
    {
        sb.Append(key).Append(":");
        AddCountDictCore(sb, set);
        return sb;
    }

	public static void AddCountVarObject<T>(StringBuilder sb, string varname, Dictionary<T, int> set)
	{
		sb.Append(varname).Append("=");
        AddCountDictCore(sb,set);
		sb.Append(";\n");
	}
    private static void AddCountDictCore<T>(StringBuilder sb, Dictionary<T, int> set)
    {
        sb.Append("[");
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
        sb.Append("]");
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
#if UNITY_2017_3_OR_NEWER
        File.WriteAllBytes(path, ImageConversion.EncodeToPNG( tex ));
#else
        File.WriteAllBytes(path, tex.EncodeToPNG());
#endif
    }

    public static string GetAssetPreview(AssetImporter importer,UnityEngine.Object obj,bool createFlag)
	{
        if (importer == null || obj == null)
        {
            return "";
        }
		string guid = AssetDatabase.AssetPathToGUID(importer.assetPath);
		string file = guid + ".png";
        if (!createFlag)
        {
            return file;
        }

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
        if (tex != null)
        {
            AssetsReporterUtils.SaveTexture2d(AssetsReporterUtils.PreviewDir+file, tex);
            tex = null;
        }
		return file;
	}

    static int cnt = 0;
    public static string GetWebVisibleTexturePreview(TextureImporter importer, Texture2D tex,bool createFlag)
    {
        if (importer == null || tex == null)
        {
            return "";
        }
        string guid = AssetDatabase.AssetPathToGUID(importer.assetPath);
        string file = guid + ".png";
        if (!createFlag) { return file; }

#if UNITY_2017_3_OR_NEWER

        var backupActive = RenderTexture.active;
        RenderTexture renderTexture = new RenderTexture(tex.width, tex.height, 0);
        CommandBuffer cmd = new CommandBuffer();
        cmd.Blit(tex,renderTexture);
        Graphics.ExecuteCommandBuffer(cmd);
        cmd.Dispose();

        RenderTexture.active = renderTexture;
        var saveTex = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
        saveTex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        AssetsReporterUtils.SaveTexture2d(AssetsReporterUtils.PreviewDir + file, saveTex);

        RenderTexture.active = backupActive;
        renderTexture.Release();
#else
        var texCopy = new Texture2D(tex.width, tex.height, tex.format, tex.mipmapCount > 1);
        texCopy.LoadRawTextureData(tex.GetRawTextureData());
        texCopy.Apply();
        if (texCopy != null)
        {
            var saveTex = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32,false);
            saveTex.SetPixels(texCopy.GetPixels() );
            saveTex.Apply();
            AssetsReporterUtils.SaveTexture2d(AssetsReporterUtils.PreviewDir + file, saveTex);

            Texture2D.DestroyImmediate(saveTex);
            Texture2D.DestroyImmediate(texCopy);
            saveTex = texCopy = null;
        }
#endif
        return file;
    }

    public static bool IsVisibleInWebBrowserImage(string path)
    {
        path = path.ToLower();
        if (path.EndsWith(".png")) { return true; }
        if (path.EndsWith(".jpg")) { return true; }
        if (path.EndsWith(".bmp")) { return true; }
        if (path.EndsWith(".gif")) { return true; }
        return false;
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
