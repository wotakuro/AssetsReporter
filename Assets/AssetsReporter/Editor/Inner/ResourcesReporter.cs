using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class ResourcesReporter {

    private HashSet<string> dependSet = new HashSet<string>();
    private List<string> resourceAssetList = new List<string>();
    private Dictionary<string, Type> fileTypeDict = new Dictionary<string, Type>();

    private Dictionary<Type, int> typeDict = new Dictionary<Type,int>();
    private Dictionary<string, int> parentDirDict = new Dictionary<string, int>();


    public static void CreateReport()
	{
        var reporter = new ResourcesReporter();
        reporter.ReportResources("AssetsReporter/result/report_resources.js");
	}

    public static void OpenReport()
    {
        AssetsReporterUtils.OpenURL(Path.Combine("AssetsReporter", "report_resources.html"));
    }

    public ResourcesReporter()
	{
	}

    public void ReportResources(string reportPath)
    {
        try
        {
            var sb = new StringBuilder(1024 * 1024);
            bool isFirst = true;
            AssetsReporterUtils.AddCurrenTimeVar(sb);
            sb.Append("g_resources_report = [");
            var allAssetPath = AssetDatabase.GetAllAssetPaths();
            foreach (var assetPath in allAssetPath)
            {
                if (!assetPath.Contains("/Resources/"))
                {
                    continue;
                }
                if (isFirst) { isFirst = false; }
                else { sb.Append(','); }
                sb.Append("\n");
                this.resourceAssetList.Add(assetPath);
                this.ReportOneResource(sb, assetPath);
                this.SetParentDictionarySet(assetPath);
            }
            sb.Append("];\n");
            this.ReportDependsAssets(sb);

            AssetsReporterUtils.AddCountVarObject(sb, "g_resources_type_list", this.typeDict);
            AssetsReporterUtils.AddCountVarObject(sb, "g_resources_parent_dir_list", this.parentDirDict);
            AssetsReporterUtils.AddPlatformVar(sb, "");
            File.WriteAllText(reportPath, sb.ToString());
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private void ReportDependsAssets(StringBuilder sb) {
        foreach (var asset in this.resourceAssetList) {
            this.dependSet.Remove(asset);
        }
        bool isFirst = true;
        sb.Append("g_resources_depends=[");
        foreach (var depend in dependSet)
        {
            if (isFirst) { isFirst = false; }
            else { sb.Append(","); }
            sb.Append("\n");
            sb.Append("{");
            AssetsReporterUtils.AddJsonObject(sb, "path", depend).Append(",");
            AssetsReporterUtils.AddJsonObject(sb, "type",this.GetFileType(depend).ToString() );
            sb.Append("}");
        }
        sb.Append("];");
    }

    private void ReportOneResource(StringBuilder sb, string assetPath)
    {
        var depends = AssetDatabase.GetDependencies( new string[]{assetPath});

        Type type = this.GetFileType(assetPath);
        AssetsReporterUtils.AddCountDictionary(this.typeDict, type);

        sb.Append("{");
        AssetsReporterUtils.AddJsonObject(sb, "path", assetPath).Append(",");
        AssetsReporterUtils.AddJsonObject(sb, "type", type.ToString() ).Append(",");
        AssetsReporterUtils.AddJsonObject(sb, "parentDir", this.GetParentDirectory(assetPath)).Append(",");

        sb.Append("depends:[");
        bool isFirst = true;
        foreach (var d in depends)
        {
            if (d == assetPath) { continue; }
            if (isFirst) { isFirst = false; }
            else { sb.Append(","); }
            sb.Append("{");
            AssetsReporterUtils.AddJsonObject(sb,"path", d).Append(",");
            AssetsReporterUtils.AddJsonObject(sb, "type", this.GetFileType(d).ToString() );
            sb.Append("}");
        }
        sb.Append("]");
        foreach (var depend in depends)
        {
            if (!this.dependSet.Contains(depend))
            {
                this.dependSet.Add(depend);
            }
        }
        sb.Append("}");
    }

    private Type GetFileType(string path)
    {
        if (fileTypeDict.ContainsKey(path))
        {
            return fileTypeDict[path];
        }
        UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath < UnityEngine.Object>(path);
        if (obj == null)
        {
            return typeof(System.Object);
        }
        var type = obj.GetType();
        if ( type != typeof(GameObject))
        {
            Resources.UnloadAsset(obj);
        }
        obj = null;
        this.fileTypeDict.Add(path, type);
        return type;
    }

    private string GetParentDirectory(string path)
    {
        int idx = path.IndexOf("/Resources/");
        if (idx < 0)
        {
            return "";
        }
        string parent = path.Substring(0, idx);
        return parent;
    }

    private void SetParentDictionarySet(string path)
    {
        string parent = this.GetParentDirectory(path);
        AssetsReporterUtils.AddCountDictionary(this.parentDirDict, parent);
    }

}
