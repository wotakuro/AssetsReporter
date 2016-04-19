using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

public class AssetBundleReporter {

	public static void CreateReport()
	{
		var reporter = new AssetBundleReporter();
		reporter.Report();
	}

	public static void OpenReport()
	{
        AssetsReporterUtils.OpenURL(Path.Combine("AssetsReporter", "report_ab.html"));
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
		sb.Append("files:[");
		foreach (var path in paths)
		{
			if (isFirst) { isFirst = false; }
			else { sb.Append(","); }
			sb.Append("{");
			AssetsReporterUtils.AddJsonObject(sb, "path", path).Append(",");
			var depends = AssetDatabase.GetDependencies(new string[] { path });
			AssetsReporterUtils.AddJsonObjectArrayWithout(sb, "depends", depends , path);
			System.Array.Sort<string>(depends);

			UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
			if (obj != null)
			{
				sb.Append(",");
				AssetsReporterUtils.AddJsonObject(sb, "type", obj.GetType().Name);
			}
			sb.Append("}");
		}
		sb.Append("]");
		sb.Append("}\n");
	}
}
