using UnityEngine;
using UnityEditor;
using System;
using System.Text;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class AssetsReporterWindow : EditorWindow {

    public class LanguageSetting
    {
        public string languageCode;
        public string languageOutput;
        public string appLanguage;

        public LanguageSetting(string code, string output,string language)
        {
            this.languageCode = code;
            this.languageOutput = output;
            this.appLanguage = language;
        }
    }

	private const float Space = 10.0f;
	public const string excludeRulePath = "AssetsReporter/excludeList.txt";

    public static readonly LanguageSetting[] languages = 
    {
        new LanguageSetting("en","English","English"),
        new LanguageSetting("jp","日本語","Japanese"),
    };
    private int selectLanguageIdx = 0;
    private string[] selectLanguageList;

	public static readonly string[] targetList = 
	{
		"default",
		"Standalone",
		"iOS",
		"Android",
		"WebGL",
	};

	private int currentTarget;
	private Vector2 scrollPos;

	private List<string> excludeList = new List<string>();

	[MenuItem("Tools/AssetsReporter")]
	public static void Create()
	{
        Debug.Log(EditorUserBuildSettings.activeBuildTarget);
		EditorWindow.GetWindow<AssetsReporterWindow>();
	}


    void OnGUI()
	{
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);


        OnGUISelectLanguage();

		EditorGUILayout.LabelField("Platform Select");
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("", GUILayout.Width(Space));
		currentTarget = EditorGUILayout.Popup(currentTarget, targetList);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();
		OnGUIExcludeList();
		EditorGUILayout.Space();
        // all
        OnGUIReportGroup("All Report",
            () =>
            {
                TextureReporter.CreateReport(targetList[currentTarget], excludeList);
                ModelReporter.CreateReport(excludeList);
                AudioReporter.CreateReport(targetList[currentTarget], excludeList);
                AssetBundleReporter.CreateReport(false);
                ResourcesReporter.CreateReport();
                SceneReporter.CreateReport(excludeList);
            },
            () =>
            {
                AssetsReporterUtils.OpenURL(Path.Combine("AssetsReporter", "index.html"));
            });

        EditorGUILayout.Space();
        // Texture
        OnGUIReportGroup("Texture Report",
            () => { TextureReporter.CreateReport(targetList[currentTarget], excludeList); },
            TextureReporter.OpenReport);
        // Audios 
        OnGUIReportGroup("Audio Report",
            () => { AudioReporter.CreateReport(targetList[currentTarget],excludeList); },
            AudioReporter.OpenReport);
        // models
        OnGUIReportGroup("Model Report",
            () => { ModelReporter.CreateReport(excludeList); }, 
            ModelReporter.OpenReport);
        // AssetBundles
        OnGUIReportGroup("AssetBundle Report",
            () => { AssetBundleReporter.CreateReport(true); },
           AssetBundleReporter.OpenReport);
        // Resources
        OnGUIReportGroup("Resources Report", ResourcesReporter.CreateReport, ResourcesReporter.OpenReport);
        // Scenes
        OnGUIReportGroup("Scene Report",
            () => { SceneReporter.CreateReport(excludeList); },
            SceneReporter.OpenReport);

        EditorGUILayout.EndScrollView();
	}

    void OnGUISelectLanguage()
    {
        if (selectLanguageList == null)
        {
            selectLanguageList = new string[languages.Length];
            for (int i = 0; i < selectLanguageList.Length; ++i)
            {
                selectLanguageList[i] = languages[i].languageOutput;
            }
        }
        EditorGUILayout.LabelField("Language Select");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("", GUILayout.Width(Space));
        this.selectLanguageIdx = EditorGUILayout.Popup(this.selectLanguageIdx, selectLanguageList);
        EditorGUILayout.EndHorizontal();
    }

	void OnEnable()
	{
        this.selectLanguageIdx = GetActiveLanguage();
        this.currentTarget = GetCurrentTarget();
		LoadExcludeList();
	}

    public static int GetActiveLanguage()
    {
        string language = Application.systemLanguage.ToString();
        for (int i = 0; i < languages.Length; ++i)
        {
            if (languages[i].appLanguage == language)
            {
                return i;
            }
        }
        return 0;
    }

    public static int GetCurrentTarget()
    {
        switch (EditorUserBuildSettings.activeBuildTarget)
        {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
            case BuildTarget.StandaloneOSXUniversal:
            case BuildTarget.StandaloneOSXIntel64:
            case BuildTarget.StandaloneOSXIntel:
            case BuildTarget.StandaloneLinux:
            case BuildTarget.StandaloneLinux64:
            case BuildTarget.StandaloneLinuxUniversal:
                return 1;
            case BuildTarget.iOS:
                return 2;
            case BuildTarget.Android:
                return 3;
            case BuildTarget.WebGL:
                return 4;
        }
        return 0;
    }

	private void OnGUIExcludeList()
	{
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Exclude List( Regex )");
		if (GUILayout.Button("Save", GUILayout.Width(45)))
		{
			if (EditorUtility.DisplayDialog("Coinfirm", "Save Exclude List?", "ok", "cancel"))
			{
				SaveExcludeList();
			}
		}
		if (GUILayout.Button("Load", GUILayout.Width(45)))
		{
			if (EditorUtility.DisplayDialog("Confirm", "Load Exclude List？", "ok", "cancel"))
			{
				LoadExcludeList();
			}
		}
		EditorGUILayout.EndHorizontal();

		int length = excludeList.Count;
		int removeIdx = -1;
		for (int i = 0; i < length; ++i )
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("", GUILayout.Width(Space));
			excludeList[i] = GUILayout.TextArea(excludeList[i]);
			if (GUILayout.Button("x",GUILayout.Width(20)))
			{
				removeIdx = i;
			}
			EditorGUILayout.EndHorizontal();
		}
		if (removeIdx >= 0)
		{
			excludeList.RemoveAt(removeIdx);
		}
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("", GUILayout.Width(Space));
		if (GUILayout.Button("Add"))
		{
			excludeList.Add( "" );
		}
		EditorGUILayout.EndHorizontal();
	}


    private void OnGUIReportGroup(string groupName, Action reportAct, Action openAct)
    {
        EditorGUILayout.LabelField(groupName);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("", GUILayout.Width(Space));
        if (GUILayout.Button("Report", GUILayout.Width(100)))
        {
            SaveExcludeList();
            AssetsReporterUtils.WriteReportLanguage(languages[this.selectLanguageIdx].languageCode);
            reportAct();
            openAct();
        }
        if (GUILayout.Button("Open", GUILayout.Width(100)))
        {
            openAct();
        }
        EditorGUILayout.EndHorizontal();
    }


	private void SaveExcludeList()
	{
		File.WriteAllLines(excludeRulePath, excludeList.ToArray());
	}
	private void LoadExcludeList()
	{
        excludeList = ReadExculudeList();
	}
    private static List<string> ReadExculudeList()
    {
        var list = new List<string>(File.ReadAllLines(excludeRulePath));
        return list;
    }
}
