using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

public class TextureReporter {


	private List<string> excludeList = new List<string>();

	private Dictionary<TextureImporterType, int> textureTypeSet = new Dictionary<TextureImporterType, int>();
	private Dictionary<TextureImporterFormat, int> textureFormatSet = new Dictionary<TextureImporterFormat, int>();

	private string platform;
	/// <summary>
	/// Create Window
	/// </summary>
	public static void CreateReport(string pl,List<string>exList)
	{
		var reporter = new TextureReporter( pl , exList );
		reporter.ReportTexture("AssetsReporter/result/report_texture.js");
	}
	public static void OpenReport()
	{
		Application.OpenURL(Path.Combine("AssetsReporter", "report_texture.html"));
	}

	public TextureReporter(string pl, List<string> exList)
	{
		if (pl == "default") { pl = ""; }

		this.platform = pl;
		this.excludeList = exList;
	}

	public void ReportTexture(string reportPath)
	{
		try
		{
			this.textureTypeSet = new Dictionary<TextureImporterType,int>();
			this.textureFormatSet = new Dictionary<TextureImporterFormat,int>();
			var guids = AssetDatabase.FindAssets("t:texture2D", null);
			var sb = new StringBuilder(1024 * 1024);
			int idx = 0;
			bool isFirst = true;
			AssetsReporterUtils.AddCurrenTimeVar(sb);
			sb.Append("g_texture_report = [");
			foreach (var guid in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				var textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
				if (textureImporter == null || AssetsReporterUtils.IsPathMatch(path, excludeList))
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
				ReportTexture(sb, textureImporter);

				sb.Append("\n");
				EditorUtility.DisplayProgressBar("Progress",  path, (float)idx / (float)guids.Length);
				++idx;
			}
			sb.Append("];");
			AssetsReporterUtils.AddCountVarObject(sb, "g_texture_format_list", textureFormatSet);
			AssetsReporterUtils.AddCountVarObject(sb, "g_texture_type_list", textureTypeSet);
			AssetsReporterUtils.AddPlatformVar(sb, this.platform);
			File.WriteAllText(reportPath, sb.ToString());
		}
		finally
		{
			EditorUtility.ClearProgressBar();
		}
	}

	private void ReportTexture(StringBuilder sb , TextureImporter importer) {
		int w, h;
		var format = importer.textureFormat;
		var type = importer.textureType;
		int maxSize = importer.maxTextureSize;

		if (! importer.GetPlatformTextureSettings(this.platform, out maxSize, out format))
		{
			maxSize = importer.maxTextureSize;
			format = importer.textureFormat;
		}

		GetTextureSize(importer, out w, out h);
		sb.Append("{");
		AssetsReporterUtils.AddJsonObject(sb, "path", importer.assetPath.ToString()).Append(",");
		AssetsReporterUtils.AddJsonObject(sb, "textureType", type.ToString()).Append(",");
		AssetsReporterUtils.AddJsonObject(sb, "isReadable", importer.isReadable).Append(",");
		AssetsReporterUtils.AddJsonObject(sb, "wrapMode", importer.wrapMode.ToString()).Append(",");
		AssetsReporterUtils.AddJsonObject(sb, "mipmapEnabled", importer.mipmapEnabled).Append(",");
		AssetsReporterUtils.AddJsonObject(sb, "width", w).Append(",");
		AssetsReporterUtils.AddJsonObject(sb, "height", h).Append(",");
		AssetsReporterUtils.AddJsonObject(sb, "isPow2", IsPow2Size(w, h)).Append(",");
		AssetsReporterUtils.AddJsonObject(sb, "maxSize", maxSize ).Append(",");
		AssetsReporterUtils.AddJsonObject(sb, "textureFormat", format.ToString());
		sb.Append("}");

		AssetsReporterUtils.AddCountDictionary(this.textureFormatSet, format);
		AssetsReporterUtils.AddCountDictionary(this.textureTypeSet, type);
	}
	private bool IsPow2Size(int width , int height)
	{
		bool wFlag = false;
		bool hFlag = false;
		for (int i = 0; i < 31; ++i)
		{
			int tmp = (1 << i);
			wFlag |= (tmp == width);
			hFlag |= (tmp == height);
			if (tmp >= width && tmp >= height)
			{
				break;
			}
		}
		return (wFlag & hFlag);
	}

	private void GetTextureSize(TextureImporter import, out int w, out int h)
	{
		w = h = 0;
		byte[] data = File.ReadAllBytes(import.assetPath);
		if (IsPngData(data))
		{
			GetPngSize(data, out w, out h);
		}
	}

	#region PNG_DATA
	/// <summary>
	/// png判定用のヘッダー
	/// </summary>
	private static readonly byte[] PngHeaderData = new byte[] { (byte)0x89, (byte)0x50, (byte)0x4E, (byte)0x47, (byte)0x0D, (byte)0x0A, (byte)0x1A, (byte)0x0A };

	/// <summary>
	/// pngかどうかを判定します
	/// </summary>
	/// <param name="data"></param>
	/// <returns></returns>
	private static bool IsPngData(byte[] data)
	{
		if (data == null || data.Length < PngHeaderData.Length)
		{
			return false;
		}
		int length = PngHeaderData.Length;
		for (int i = 0; i < length; ++i)
		{
			if (data[i] != PngHeaderData[i])
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Pngのサイズをヘッダーから取得します
	/// </summary>
	/// <param name="data">PNGのデータ</param>
	/// <param name="width">幅を書き込みます</param>
	/// <param name="height">高さを書き込みます</param>
	private static void GetPngSize(byte[] data, out int width, out int height)
	{
		width = 0;
		height = 0;
		if (data == null || data.Length < 33)
		{
			return;
		}
		int idx = 16;
		width = (data[idx + 0] << 24) +
			(data[idx + 1] << 16) +
			(data[idx + 2] << 8) +
			(data[idx + 3] << 0);
		idx = 20;
		height = (data[idx + 0] << 24) +
			(data[idx + 1] << 16) +
			(data[idx + 2] << 8) +
			(data[idx + 3] << 0);
	}
	#endregion PNG_DATA
}
