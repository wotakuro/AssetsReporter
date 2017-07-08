using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SceneReporter {
    private List<string> GetBuildScenesList()
    {
        List<string> buildScenes = new List<string>();
        var scenes = EditorBuildSettings.scenes;
        foreach (var scene in scenes)
        {
            if (!scene.enabled) { continue; }
            buildScenes.Add(scene.path);
        }
        return buildScenes;
    }

    public static void CreateReport()
    {
        var reporter = new SceneReporter();
        reporter.ReportScenes(AssetsReporterUtils.ResultDir + "report_scene.js");
    }
    public static void OpenReport()
    {
        AssetsReporterUtils.OpenURL(Path.Combine("AssetsReporter", "report_scenes.html"));
    }

    public void ReportScenes(string reportPath)
    {
        try
        {
            var sb = new StringBuilder(1024 * 1024);
            sb.Append("g_scenes_report = [");
            var scenePathList = GetBuildScenesList();

            bool isFirst = true;
            foreach (var scenePath in scenePathList)
            {
                if (isFirst) { isFirst = false; }
                else { sb.Append(","); }
                ReportSceneAtPath(sb, scenePath);
            }

            sb.Append("];");
            File.WriteAllText(reportPath, sb.ToString());
            AssetsReporterUtils.AddCurrenTimeVar(sb);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    private void ReportSceneAtPath( StringBuilder sb,string scenePath)
    {
        sb.Append("{");
        AssetsReporterUtils.AddJsonObject(sb, "scenePath", scenePath ).Append(",\n");

        ReportSceneDependAssets(sb, scenePath);
        sb.Append(",");

        var scene = EditorSceneManager.OpenScene(scenePath);
        var rootObjects = scene.GetRootGameObjects();

        int allGameObjectCount = 0;
        int allComponentCount = 0;
        int allMonoBehaviourCount = 0;
        foreach (var rootObj in rootObjects)
        {
            allGameObjectCount += CountChildTransform(rootObj.transform);
            var childComponents = rootObj.GetComponentsInChildren<Component>();
            var childMonoBehaviour = rootObj.GetComponentsInChildren<MonoBehaviour>();
            if (childComponents != null) { allComponentCount += childComponents.Length; }
            if (childMonoBehaviour != null) { allMonoBehaviourCount += childMonoBehaviour.Length; }
        }
        AssetsReporterUtils.AddJsonObject(sb, "allGameObjects", allGameObjectCount).Append(",");
        AssetsReporterUtils.AddJsonObject(sb, "allComponents", allComponentCount).Append(",");
        AssetsReporterUtils.AddJsonObject(sb, "allMonoBehaviour", allMonoBehaviourCount);

        sb.Append("}");
    }

    private int CountChildTransform(Transform trans)
    {
        if(trans.childCount == 0 ){
            return 1;
        }
        int num = 0;
        foreach (Transform child in trans)
        {
            num = num + CountChildTransform( child  );
        }
        return num;
    }

    private StringBuilder ReportSceneDependAssets(StringBuilder sb , string scenePath){
        var depends = AssetDatabase.GetDependencies(scenePath, true);
        sb.Append("depends:[");
        if (depends != null) {
            bool firstFlag = true;
            foreach (var depend in depends)
            {
                if (depend == null) { continue; }
                if (firstFlag) { firstFlag = false; }
                else { sb.Append(','); }
                var importer = AssetImporter.GetAtPath(depend);
                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(depend);
                sb.Append("{");
                AssetsReporterUtils.AddJsonObject(sb, "path", depend).Append(",");
                AssetsReporterUtils.AddJsonObject(sb, "importer", importer.GetType().ToString());

                if (obj != null)
                {
                    sb.Append(",");
                    AssetsReporterUtils.AddJsonObject(sb, "type", obj.GetType().ToString());
                }
                obj = null;
                sb.Append("}\n");
            }
        }
        Resources.UnloadUnusedAssets();
        sb.Append("]\n");
        return sb;
    }


}
