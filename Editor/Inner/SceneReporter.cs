using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace UTJ.AssetsReporter
{

    public class SceneReporter
    {

        private List<string> excludeList = new List<string>();

        private SceneReporter(List<string> exList)
        {

            this.excludeList = exList;
        }

        private List<string> GetAllScenePath()
        {
            var guids = AssetDatabase.FindAssets("t:scene", null);
            List<string> allScenePath = new List<string>(guids.Length);
            foreach (var guid in guids)
            {
                allScenePath.Add(AssetDatabase.GUIDToAssetPath(guid));
            }
            return allScenePath;
        }

        public static void CreateReport(List<string> exList)
        {
            var reporter = new SceneReporter(exList);
            reporter.ReportScenes(AssetsReporterUtils.ResultDir + "report_scene.js");
        }
        public static void OpenReport()
        {
            AssetsReporterUtils.OpenURL(Path.Combine("AssetsReporter", "report_scene.html"));
        }

        public void ReportScenes(string reportPath)
        {
            try
            {
                var sb = new StringBuilder(1024 * 1024);
                sb.Append("g_scene_report = [");
                var scenePathList = GetAllScenePath();

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
        private void ReportSceneAtPath(StringBuilder sb, string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath);
            sb.Append("{");
            AssetsReporterUtils.AddJsonObject(sb, "buildIndex", scene.buildIndex).Append(",\n");
            AssetsReporterUtils.AddJsonObject(sb, "path", scenePath).Append(",\n");
            AssetsReporterUtils.AddJsonObject(sb, "sceneName", scene.name).Append(",\n");
            AssetsReporterUtils.AddJsonObject(sb, "rootCount", scene.rootCount).Append(",\n");

            ReportSceneDependAssets(sb, scenePath);
            sb.Append(",");

            var rootObjects = scene.GetRootGameObjects();

            int allGameObjectCount = 0;
            int allComponentCount = 0;
            int allMonoBehaviourCount = 0;
            var componentsCountDictionary = new Dictionary<string, int>(1024);
            foreach (var rootObj in rootObjects)
            {
                allGameObjectCount += CountChildTransform(rootObj.transform);
                var childComponents = rootObj.GetComponentsInChildren<Component>();
                var childMonoBehaviour = rootObj.GetComponentsInChildren<MonoBehaviour>();
                if (childComponents != null) { allComponentCount += childComponents.Length; }
                if (childMonoBehaviour != null) { allMonoBehaviourCount += childMonoBehaviour.Length; }
                AddToConpoenentCountDictionary(componentsCountDictionary, childComponents);
            }
            AssetsReporterUtils.AddJsonObject(sb, "allGameObjects", allGameObjectCount).Append(",");
            AssetsReporterUtils.AddJsonObject(sb, "allComponents", allComponentCount).Append(",");
            AssetsReporterUtils.AddJsonObject(sb, "allMonoBehaviour", allMonoBehaviourCount).Append(",");
            AssetsReporterUtils.AddJsonToContDictionary(sb, "componentCount", componentsCountDictionary);
            sb.Append("}");
        }

        private static void AddToConpoenentCountDictionary(Dictionary<string, int> cntDict, Component[] components)
        {
            if (components == null)
            {
                return;
            }
            foreach (var component in components)
            {
                if (component != null)
                {
                    AssetsReporterUtils.AddCountDictionary(cntDict, component.GetType().ToString());
                }
            }
        }

        private int CountChildTransform(Transform trans)
        {
            if (trans.childCount <= 0)
            {
                return 1;
            }
            int num = 1;
            foreach (Transform child in trans)
            {
                num += CountChildTransform(child);
            }
            return num;
        }

        private StringBuilder ReportSceneDependAssets(StringBuilder sb, string scenePath)
        {
            var depends = AssetDatabase.GetDependencies(scenePath, true);
            sb.Append("depends:[");
            if (depends != null)
            {
                bool firstFlag = true;
                foreach (var depend in depends)
                {
                    if (depend == null) { continue; }
                    if (firstFlag) { firstFlag = false; }
                    else { sb.Append(','); }
                    var importer = AssetImporter.GetAtPath(depend);
                    var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(importer.assetPath);
                    sb.Append("{");
                    AssetsReporterUtils.AddJsonObject(sb, "path", depend).Append(",");
                    AssetsReporterUtils.AddJsonObject(sb, "importer", importer.GetType().ToString());

                    if (obj != null)
                    {
                        sb.Append(",");
                        AssetsReporterUtils.AddJsonObject(sb, "type", obj.GetType().ToString());
                    }
                    else
                    {
                        Debug.Log("loaderror " + depend + "::" + importer.name);
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
}