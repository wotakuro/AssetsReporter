using UnityEngine;
using UnityEditor;
using System.Text;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class AssetsReporterWindow : EditorWindow {

    private class LanguageSetting
    {
        public string languageCode;
        public string languageOutput;

        public LanguageSetting(string code, string output)
        {
            this.languageCode = code;
            this.languageOutput = output;
        }
    }

	private const float Space = 10.0f;
	private const string excludeRulePath = "Assets/AssetsReporter/Editor/Data/excludeList.txt";

    private LanguageSetting[] languages = 
    {
        new LanguageSetting("en","English"),
        new LanguageSetting("jp","日本語"),
    };
    private int selectLanguageIdx = 0;
    private string[] selectLanguageList;

	private string[] targetList = 
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
        OnGUIAll();
        EditorGUILayout.Space();
		OnGUITexture();
		OnGUIAudio();
		OnGUIModel();
		OnGUIAssetBundle();
        OnGUIResources();
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
		LoadExcludeList();
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


    private void OnGUIAll()
    {
        EditorGUILayout.LabelField("All Report");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("", GUILayout.Width(Space));
        if (GUILayout.Button("Report", GUILayout.Width(100)))
        {
            SaveExcludeList();
            AssetsReporterUtils.WriteReportLanguage(languages[this.selectLanguageIdx].languageCode);
            TextureReporter.CreateReport(this.targetList[currentTarget], excludeList);
            ModelReporter.CreateReport( excludeList);
            AudioReporter.CreateReport(this.targetList[currentTarget], excludeList);
            AssetBundleReporter.CreateReport();
            ResourcesReporter.CreateReport();
            AssetsReporterUtils.OpenURL(Path.Combine("AssetsReporter", "index.html"));
        }
        if (GUILayout.Button("Open", GUILayout.Width(100)))
        {
            AssetsReporterUtils.OpenURL(Path.Combine("AssetsReporter","index.html"));
        }
        EditorGUILayout.EndHorizontal();
    }

    private void OnGUITexture()
    {
        EditorGUILayout.LabelField("Texture Report");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("", GUILayout.Width(Space));
        if (GUILayout.Button("Report", GUILayout.Width(100)))
        {
            SaveExcludeList();
            AssetsReporterUtils.WriteReportLanguage(languages[this.selectLanguageIdx].languageCode);
            TextureReporter.CreateReport(this.targetList[currentTarget], excludeList);
            TextureReporter.OpenReport();
        }
        if (GUILayout.Button("Open", GUILayout.Width(100)))
        {
            TextureReporter.OpenReport();
        }
        EditorGUILayout.EndHorizontal();
    }
	private void OnGUIAudio()
	{
		EditorGUILayout.LabelField("Audio Report");
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("", GUILayout.Width(Space));
		if (GUILayout.Button("Report", GUILayout.Width(100)))
		{
			SaveExcludeList();
            AssetsReporterUtils.WriteReportLanguage(languages[this.selectLanguageIdx].languageCode);
            AudioReporter.CreateReport(this.targetList[currentTarget], excludeList);
			AudioReporter.OpenReport();
		}
		if (GUILayout.Button("Open", GUILayout.Width(100)))
		{
			AudioReporter.OpenReport();
		}
		EditorGUILayout.EndHorizontal();
	}

	private void OnGUIModel()
	{
		EditorGUILayout.LabelField("Model Report");
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("", GUILayout.Width(Space));
		if (GUILayout.Button("Report", GUILayout.Width(100)))
		{
			SaveExcludeList();
            AssetsReporterUtils.WriteReportLanguage(languages[this.selectLanguageIdx].languageCode);
            ModelReporter.CreateReport(excludeList);
			ModelReporter.OpenReport();
		}
		if (GUILayout.Button("Open", GUILayout.Width(100)))
		{
			ModelReporter.OpenReport();
		}
		EditorGUILayout.EndHorizontal();
	}
	private void OnGUIAssetBundle()
	{
		EditorGUILayout.LabelField("AssetBundle Report");
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("", GUILayout.Width(Space));
		if (GUILayout.Button("Report", GUILayout.Width(100)))
		{
			SaveExcludeList();
            AssetsReporterUtils.WriteReportLanguage(languages[this.selectLanguageIdx].languageCode);
            AssetBundleReporter.CreateReport();
			AssetBundleReporter.OpenReport();
		}
		if (GUILayout.Button("Open", GUILayout.Width(100)))
		{
			AssetBundleReporter.OpenReport();
		}
		EditorGUILayout.EndHorizontal();
	}

    private void OnGUIResources()
    {
        EditorGUILayout.LabelField("Resources Report");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("", GUILayout.Width(Space));
        if (GUILayout.Button("Report", GUILayout.Width(100)))
        {
            SaveExcludeList();
            AssetsReporterUtils.WriteReportLanguage(languages[this.selectLanguageIdx].languageCode);
            ResourcesReporter.CreateReport();
            ResourcesReporter.OpenReport();
        }
        if (GUILayout.Button("Open", GUILayout.Width(100)))
        {
            ResourcesReporter.OpenReport();
        }
        EditorGUILayout.EndHorizontal();
    }

	private void SaveExcludeList()
	{
		File.WriteAllLines(excludeRulePath, excludeList.ToArray());
	}
	private void LoadExcludeList()
	{
		excludeList = new List<string>( File.ReadAllLines(excludeRulePath) );
	}
}
