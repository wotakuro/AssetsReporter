using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

public class AssetBundleReporter {

	#region INNER_CLASS
	private class AssetBundleIdentify
	{
		public string file { get; private set; }
		public string name { get; private set; }
		public string valiant { get; private set; }
		public string importerType { get; private set; }

		public AssetBundleIdentify(string f,string n, string v,string t)
		{
			this.file = f;
			this.name = n;
			this.valiant = v;
			this.importerType = t;
		}

		public override bool Equals(object obj)
		{
			var castObj = obj as AssetBundleIdentify;
			if (castObj == null) { return false; }
			return (castObj.file == this.file);
		}
		public override int GetHashCode()
		{
			return file.GetHashCode();
		}
	}
	#endregion INNER_CLASS

	private HashSet<AssetBundleIdentify> dependBundle;
    private bool createThumnailPreview;

    public static void CreateReport(bool thumnailFlag)
	{
		var reporter = new AssetBundleReporter();
        reporter.Report(thumnailFlag);
	}

	public static void OpenReport()
	{
		Application.OpenURL(Path.Combine("AssetsReporter", "report_ab.html"));
	}

    public void Report(bool thumnailFlag)
	{
		try
		{
            this.createThumnailPreview = thumnailFlag;
			int idx = 0;
			StringBuilder sb = new StringBuilder();

			string[] abnames = AssetDatabase.GetAllAssetBundleNames();
			bool isFirst = true;
			AssetsReporterUtils.AddCurrenTimeVar(sb); 
			AssetsReporterUtils.AddPlatformVar(sb, "");

			sb.Append("g_ab_report=[");
			foreach (var abname in abnames)
			{
				string[] paths = AssetDatabase.GetAssetPathsFromAssetBundle(abname);
				if (paths == null || paths.Length == 0) { continue; }
				if (isFirst) { isFirst = false; }
				else { sb.Append(","); }
				CreateAssetData(sb, abname , paths);
				++idx;
				EditorUtility.DisplayProgressBar("AssetBundleReport" ,abname, idx / (float)abnames.Length);
			}
			sb.Append("];");
            File.WriteAllText(AssetsReporterUtils.ResultDir + "report_ab.js", sb.ToString());
		}
		finally
		{
			EditorUtility.ClearProgressBar();
		}
	}

	private void CreateAssetData(StringBuilder sb, string abname, string[] paths)
	{
		bool isFirst = true;
		sb.Append("{");
		AssetsReporterUtils.AddJsonObject(sb, "abname", abname).Append(",");
		this.ClearDependsAssetBundle();
		sb.Append("files:[");
		foreach (var path in paths)
		{
			if (isFirst) { isFirst = false; }
			else { sb.Append(","); }
			sb.Append("{");
			AssetsReporterUtils.AddJsonObject(sb, "path", path).Append(",");

			var depends = AssetDatabase.GetDependencies(new string[] { path });
			System.Array.Sort(depends);
			this.AddDependsAssetBundle( depends);
			AssetsReporterUtils.AddJsonObjectArrayWithout(sb, "depends", depends , path);
			System.Array.Sort<string>(depends);

			UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
			if (obj != null)
			{
				sb.Append(",");
				AssetsReporterUtils.AddJsonObject(sb, "type", obj.GetType().Name);
                if (obj.GetType() != typeof(GameObject))
                {
                    Resources.UnloadAsset(obj);
                }
                // thumnail
                {
                    if (obj.GetType() == typeof(Texture2D) && !AssetsReporterUtils.IsVisibleInWebBrowserImage(path))
                    {
                        sb.Append(",");
                        string preview = AssetsReporterUtils.GetWebVisibleTexturePreview(TextureImporter.GetAtPath(path) as TextureImporter, obj as Texture2D, this.createThumnailPreview);
                        AssetsReporterUtils.AddJsonObject(sb, "preview", preview);
                    }
                    else if (obj.GetType() == typeof(GameObject))
                    {
                        sb.Append(",");
                        var assetImporter = AssetImporter.GetAtPath(path);
                        string preview = AssetsReporterUtils.GetAssetPreview( assetImporter, obj, this.createThumnailPreview || ((assetImporter as ModelImporter) == null) );
                        AssetsReporterUtils.AddJsonObject(sb, "preview", preview);
                    }
                }

                obj = null;
			}
			sb.Append("}");
		}
		sb.Append("],\n");
		this.RemoveDependsAssetBundle(paths);
		this.ReportDependsAssetBundle(sb);
		sb.Append("}\n");
	}

	private void ReportDependsAssetBundle(StringBuilder sb)
	{
		bool isFirst = true;
		sb.Append("depends:[");
		List<AssetBundleIdentify> list = new List<AssetBundleIdentify>(this.dependBundle);
		list.Sort((a, b) => {
			int tmp = a.importerType.CompareTo(b.importerType);
			if ( tmp != 0) { return tmp; }
			return a.file.CompareTo(b.file);
		});
		foreach (var d in list)
		{
			if (isFirst) { isFirst = false; }
			else { sb.Append(","); }
			sb.Append("{");
			AssetsReporterUtils.AddJsonObject(sb,"file", d.file).Append(",");
			AssetsReporterUtils.AddJsonObject(sb, "name", d.name).Append(",");
			AssetsReporterUtils.AddJsonObject(sb, "valiant", d.valiant).Append(",");
			AssetsReporterUtils.AddJsonObject(sb, "importerType", d.importerType.Replace("Importer",""));
			sb.Append("}");
		}
		sb.Append("]");
	}
	private void RemoveDependsAssetBundle(string[] paths)
	{
		if (paths == null) { return; }
		foreach (var path in paths)
		{
			this.dependBundle.Remove(new AssetBundleIdentify(path,"","",""));
		}
	}

	private void ClearDependsAssetBundle()
	{
		if (this.dependBundle == null)
		{
			this.dependBundle = new HashSet<AssetBundleIdentify>();
		}
		else
		{
			this.dependBundle.Clear();
		}
	}

	private void AddDependsAssetBundle(string[] depends)
	{
		foreach (string d in depends)
		{
			string importerName = "";
			string importerValiant = "";
			string importerType = "";
            var importer = GetAssetBundleNamedDirectoryImporter(d);
			if (importer != null ) {
                importerType = importer.GetType().Name;
                importerName = importer.assetBundleName;
				importerValiant = importer.assetBundleVariant;
#if UNITY_5_6_OR_NEWER
                if (string.IsNullOrEmpty(importerName))
                {
                    importerName = AssetDatabase.GetImplicitAssetBundleName( d );
                    importerValiant = AssetDatabase.GetImplicitAssetBundleVariantName(d);
                }
#endif
			}
			var assetBundleId = new AssetBundleIdentify(d, importerName, importerValiant, importerType);
			if( !this.dependBundle.Contains(assetBundleId) ){
				this.dependBundle.Add(assetBundleId);
			}
		}
	}

    private static AssetImporter GetAssetBundleNamedDirectoryImporter(string path)
    {
        var originImporter = AssetImporter.GetAtPath(path);
#if !UNITY_5_6_OR_NEWER
        if (!string.IsNullOrEmpty(originImporter.assetBundleName))
        {
            return originImporter;
        }

        int length = path.Length;
        StringBuilder sb = new StringBuilder(length);
        sb.Append(path);

        int idx = length-1;
        while (idx > 0 )
        {
            int next = path.LastIndexOf( '/' , idx -1  );
            if (next <= 0)
            {
                break;
            }
            sb.Length = next;

            var importer = AssetImporter.GetAtPath(sb.ToString());
            if ( !string.IsNullOrEmpty( importer.assetBundleName ) )
            {
                return importer;
            }
            idx = next;
        }
#endif
        return originImporter;
    }
}
