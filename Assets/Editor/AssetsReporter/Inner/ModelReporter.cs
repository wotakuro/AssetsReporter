using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

public class ModelReporter {


	private List<string> excludeList = new List<string>();
	private bool isPreviewImage = true;

	private Dictionary<float, int> scaleSet;
	private Dictionary<ModelImporterAnimationType, int> rigSet;
	private Dictionary<string, int> sourceAvatarSet;

//	private Dictionary<TextureImporterType, int> textureTypeSet = new Dictionary<TextureImporterType, int>();

	/// <summary>
	/// Create Window
	/// </summary>
	public static void CreateReport(List<string>exList)
	{
		var reporter = new ModelReporter( exList );
		reporter.ReportModel("AssetsReporter/result/report_model.js");
	}
	public static void OpenReport()
	{
		Application.OpenURL(Path.Combine("AssetsReporter", "report_model.html"));
	}

	public ModelReporter( List<string> exList)
	{
		this.excludeList = exList;
	}

	public void ReportModel(string reportPath)
	{
		try
		{
			scaleSet = new Dictionary<float, int>();
			rigSet = new Dictionary<ModelImporterAnimationType, int>();
			sourceAvatarSet = new Dictionary<string, int>();
			if (isPreviewImage)
			{
				AssetsReporterUtils.CreatePreviewDir();
			}

			var guids = AssetDatabase.FindAssets("t:model", null);
			var sb = new StringBuilder(1024 * 1024);
			int idx = 0;
			bool isFirst = true;
			AssetsReporterUtils.AddCurrenTimeVar(sb);
			sb.Append("g_model_report = [");
			foreach (var guid in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				var modelImporter = AssetImporter.GetAtPath(path) as ModelImporter;
				if (modelImporter == null || AssetsReporterUtils.IsPathMatch(path, excludeList))
				{
					continue;
				}
				if (isFirst)
				{
					isFirst = false;
				}
				else
				{
					sb.Append(",");
				}
				ReportModel(sb, modelImporter);

				sb.Append("\n");
				EditorUtility.DisplayProgressBar("Progress",  path, (float)idx / (float)guids.Length);
				++idx;
			}
			sb.Append("];");
			AssetsReporterUtils.AddCountVarObject(sb, "g_model_rig_list", rigSet);
			AssetsReporterUtils.AddCountVarObject(sb, "g_model_scale_list", scaleSet);
			AssetsReporterUtils.AddCountVarObject(sb, "g_model_avatar_list", sourceAvatarSet);
			AssetsReporterUtils.AddPlatformVar(sb, "");
			File.WriteAllText(reportPath, sb.ToString());
		}
		finally
		{
			EditorUtility.ClearProgressBar();
		}
	}

	private void ReportModel(StringBuilder sb, ModelImporter importer)
	{
		sb.Append("{");
		AssetsReporterUtils.AddJsonObject(sb, "path", importer.assetPath).Append(",");
		if (this.isPreviewImage)
		{
			var obj = AssetDatabase.LoadAssetAtPath<GameObject>(importer.assetPath);

			string preview = AssetsReporterUtils.SaveModelPreview(importer, obj);
			AssetsReporterUtils.AddJsonObject(sb, "preview", preview).Append(",");
			
		AssetsReporterUtils.AddJsonObject(sb, "polygonNum", AssetsReporterUtils.GetPolygonNum(obj) ).Append(",");
		}
		AssetsReporterUtils.AddJsonObject(sb, "isReadable", importer.isReadable).Append(",");
		AssetsReporterUtils.AddJsonObject(sb, "scaleFactor", importer.globalScale).Append(",");
		AssetsReporterUtils.AddJsonObject(sb, "meshCompression", importer.meshCompression.ToString()).Append(",");
		AssetsReporterUtils.AddJsonObject(sb, "optimizeMesh", importer.optimizeMesh).Append(",");
		AssetsReporterUtils.AddJsonObject(sb, "generateCollider", importer.addCollider).Append(",");
		AssetsReporterUtils.AddJsonObject(sb, "importMaterials", importer.importMaterials).Append(",");
		AssetsReporterUtils.AddJsonObject(sb, "normalMode", importer.normalImportMode.ToString()).Append(",");
		AssetsReporterUtils.AddJsonObject(sb, "tangentMode", importer.tangentImportMode.ToString()).Append(",");
		AssetsReporterUtils.AddJsonObject(sb, "importBlendShapes", importer.importBlendShapes).Append(",");
		AssetsReporterUtils.AddJsonObject(sb, "objectNum", importer.transformPaths.Length).Append(",");
		// rig
		AssetsReporterUtils.AddJsonObject(sb, "animationType", importer.animationType.ToString()).Append(",");
		AssetsReporterUtils.AddJsonObject(sb, "generateAnimations", importer.generateAnimations.ToString()).Append(",");
		string srcAvatar = GetAvatarName(importer.animationType,importer.sourceAvatar);
		AssetsReporterUtils.AddJsonObject(sb, "sourceAvatar",srcAvatar).Append(",");
		// animation
		AssetsReporterUtils.AddJsonObject(sb, "importAnimation", importer.importAnimation).Append(",");
		AssetsReporterUtils.AddJsonObject(sb, "animationCompression", importer.animationCompression.ToString()).Append(",");
		if (importer.clipAnimations != null && importer.clipAnimations.Length > 0)
		{
			ReportModelAnimationClips(sb, importer.clipAnimations);
		}
		else
		{
			ReportModelAnimationClips(sb, importer.defaultClipAnimations);
		}
		//
		AssetsReporterUtils.AddCountDictionary(rigSet, importer.animationType);
		AssetsReporterUtils.AddCountDictionary(scaleSet, importer.globalScale);
		if (!string.IsNullOrEmpty(srcAvatar))
		{
			AssetsReporterUtils.AddCountDictionary(sourceAvatarSet, srcAvatar);
		}
		sb.Append("}");
	}
	private string GetAvatarName( ModelImporterAnimationType type, Avatar avatar)
	{
		if (type == ModelImporterAnimationType.None || type == ModelImporterAnimationType.Legacy)
		{
			return "No Avatar";
		}
		if (avatar == null) { return "Create From This Model"; }
		StringBuilder sb = new StringBuilder( avatar.name.Length + 32);
		sb.Append(avatar.name).Append("(").Append(avatar.GetInstanceID()).Append(")");
		return sb.ToString();
	}

	private void ReportModelAnimationClips(StringBuilder sb, ModelImporterClipAnimation[] clipAnimations)
	{
		if (clipAnimations == null)
		{
			AssetsReporterUtils.AddJsonObjectArray(sb, "animationClips", null);
		}
		bool isFirst = true;
		sb.Append("animationClips:[");
		for (int i = 0; i < clipAnimations.Length; ++i)
		{
			if (isFirst) { isFirst = false; }
			else { sb.Append(","); }
			sb.Append("{");
			AssetsReporterUtils.AddJsonObject(sb, "name", clipAnimations[i].name).Append(",");
			AssetsReporterUtils.AddJsonObject(sb, "firstFrame", clipAnimations[i].firstFrame).Append(",");
			AssetsReporterUtils.AddJsonObject(sb, "lastFrame", clipAnimations[i].lastFrame).Append(",");
			AssetsReporterUtils.AddJsonObject(sb, "wrapMode", clipAnimations[i].wrapMode.ToString()).Append(",");
			AssetsReporterUtils.AddJsonObject(sb, "loop", clipAnimations[i].loop);
			sb.Append("}");
		}
		sb.Append("]");
	}
}
