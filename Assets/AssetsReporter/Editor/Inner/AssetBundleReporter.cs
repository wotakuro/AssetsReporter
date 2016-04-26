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

	public static void CreateReport()
	{
		var reporter = new AssetBundleReporter();
		reporter.Report();
	}

	public static void OpenReport()
	{
		Application.OpenURL(Path.Combine("AssetsReporter", "report_ab.html"));
	}

	public void Report()
	{
		try
		{
			int idx = 0;
			StringBuilder sb = new StringBuilder();

			string[] abnames = AssetDatabase.GetAllAssetBundleNames();
			bool isFirst = true;
			AssetsReporterUtils.AddCurrenTimeVar(sb); 
			AssetsReporterUtils.AddPlatformVar(sb, "");

			var dependsAssetBundle = new HashSet<string>();
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
			File.WriteAllText("AssetsReporter/result/report_ab.js", sb.ToString());
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
			var importer = AssetImporter.GetAtPath(d);
			if (importer != null ) {
				importerName = importer.assetBundleName;
				importerValiant = importer.assetBundleVariant;
				importerType = importer.GetType().Name;
			}
			var assetBundleId = new AssetBundleIdentify(d, importerName, importerValiant, importerType);
			if( !this.dependBundle.Contains(assetBundleId) ){
				this.dependBundle.Add(assetBundleId);
			}
		}
	}
}
