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

    public static void CreateReport(string pl, List<string> exList)
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

        }
    }
    private void ReportSceneAtPath( StringBuilder sb,string scenePath)
    {
        sb.Append("{");
        AssetsReporterUtils.AddJsonObject(sb, "scenePath", scenePath ).Append(",");
        var scene = EditorSceneManager.OpenScene(scenePath);

        var rootObjects = scene.GetRootGameObjects();


        sb.Append("}");
    }

    private void ReportSceneDependAssets(StringBuilder sb , string scenePath){
        var depends = AssetDatabase.GetDependencies(scenePath, true);
        sb.Append("depends:[");
        if (depends != null) {
            foreach (var depend in depends)
            {
                if (depend == null) { continue; }
            }
        }       
        sb.Append("]");
    }


}
