using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Text;
using System.IO;
using System.Collections.Generic;

public class AudioReporter
{
	private string platform;
	private List<string> excludeList;
	private Dictionary<uint, int> ratingSet;
	private Dictionary<AudioCompressionFormat, int> compressSet;
	private Dictionary<AudioClipLoadType,int> loadTypeSet;

	public static void CreateReport(string pl, List<string> exList)
	{
		var reporter = new AudioReporter(pl,exList);
		reporter.ReportAudio("AssetsReporter/result/report_audio.js");
	}

	public AudioReporter(string pl, List<string> exList)
	{
		if (pl == "default") { pl = ""; }
		this.platform = pl;
		this.excludeList = exList;
	}

	public static void OpenReport() {
		Application.OpenURL(Path.Combine("AssetsReporter", "report_audio.html"));
	}

	public void ReportAudio(string reportPath)
	{
		try
		{
			this.ratingSet = new Dictionary<uint, int>();
			this.compressSet = new Dictionary<AudioCompressionFormat, int>();
			this.loadTypeSet = new Dictionary<AudioClipLoadType, int>();

			StringBuilder sb = new StringBuilder(1024*1024);
			AssetsReporterUtils.AddCurrenTimeVar(sb);
			int idx = 0;
			bool isFirst = true;
			var guids = AssetDatabase.FindAssets("t:audioclip");
			sb.Append("g_audio_report=[");
			foreach (var guid in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				var audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;
				if (audioImporter == null || AssetsReporterUtils.IsPathMatch(path, excludeList)) { continue; }
				if (isFirst)
				{
					isFirst = false;
				}
				else
				{
					sb.Append(',');
				}
				ReportAudio(sb, audioImporter);
				++idx;
				EditorUtility.DisplayProgressBar("Progress", path , (float)idx / (float)guids.Length);
			}
			sb.Append("];");
			AssetsReporterUtils.AddCountVarObject(sb, "g_audio_rating_list", this.ratingSet);
			AssetsReporterUtils.AddCountVarObject(sb, "g_audio_loadtype_list", this.loadTypeSet);
			AssetsReporterUtils.AddCountVarObject(sb, "g_audio_compress_list", this.compressSet);
			AssetsReporterUtils.AddPlatformVar(sb, this.platform);
			File.WriteAllText(reportPath, sb.ToString());
		}
		finally {
			EditorUtility.ClearProgressBar();
		}
	}
	private void ReportAudio(StringBuilder sb, AudioImporter importer)
	{
		/// common
		sb.Append("{");
		AssetsReporterUtils.AddJsonObject(sb, "path", importer.assetPath).Append(",");
		AssetsReporterUtils.AddJsonObject(sb, "forceToMono", importer.forceToMono).Append(",");
		AssetsReporterUtils.AddJsonObject(sb, "loadInBackground", importer.loadInBackground).Append(",");
		AssetsReporterUtils.AddJsonObject(sb, "preloadAudioData", importer.preloadAudioData).Append(",");
		// platform setting
		AudioImporterSampleSettings setting = importer.defaultSampleSettings;
		if ( !string.IsNullOrEmpty(platform) && importer.ContainsSampleSettingsOverride(platform))
		{
			setting = importer.GetOverrideSampleSettings(platform);
		}
		AssetsReporterUtils.AddJsonObject(sb, "loadType", setting.loadType.ToString() ).Append(",");
		AssetsReporterUtils.AddJsonObject(sb, "compressionFormat", setting.compressionFormat.ToString()).Append(",");
		AssetsReporterUtils.AddJsonObject(sb, "sampleRateSetting", setting.sampleRateSetting.ToString()).Append(",");
		AssetsReporterUtils.AddJsonObject(sb, "sampleRateOverride", setting.sampleRateOverride.ToString() );
		sb.Append("}");

		AssetsReporterUtils.AddCountDictionary(this.loadTypeSet, setting.loadType);
		AssetsReporterUtils.AddCountDictionary(this.ratingSet, setting.sampleRateOverride);
		AssetsReporterUtils.AddCountDictionary(this.compressSet, setting.compressionFormat);
	}
}
