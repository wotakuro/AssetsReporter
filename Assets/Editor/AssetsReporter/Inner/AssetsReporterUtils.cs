using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class AssetsReporterUtils{
	public static StringBuilder AddJsonObject(StringBuilder sb, string key, string val)
	{
		sb.Append(key).Append(":\"").Append(val).Append('"');
		return sb;
	}
	public static StringBuilder AddJsonObject(StringBuilder sb, string key, bool val)
	{
		sb.Append(key).Append(":");
		if (val)
		{
			sb.Append("true");
		}
		else
		{
			sb.Append("false");
		}
		return sb;
	}
	public static StringBuilder AddJsonObject(StringBuilder sb, string key, int val)
	{
		sb.Append(key).Append(":").Append(val);
		return sb;
	}
	public static void AddCountVarObject<T>(StringBuilder sb, string varname, Dictionary<T, int> set)
	{
		sb.Append(varname).Append("=[");
		bool isFirst = true;
		foreach (var format in set)
		{
			if (isFirst)
			{
				isFirst = false;
			}
			else
			{
				sb.Append(',');
			}
			sb.Append("{val:");
			sb.Append('"').Append(format.Key.ToString()).Append('"');
			sb.Append(",cnt:").Append('"').Append(format.Value).Append('"');
			sb.Append("}");
		}
		sb.Append("];\n");
	}
	public static bool IsPathMatch(string path, List<string> list)
	{
		if (list == null)
		{
			return false;
		}
		foreach (var pattern in list)
		{
			Regex reg = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
			if (reg.IsMatch(path))
			{
				return true;
			}
		}
		return false;
	}

	public static void AddCountDictionary<T>(Dictionary<T, int> dict, T key)
	{
		if (!dict.ContainsKey(key))
		{
			dict.Add(key, 1);
			return;
		}
		dict[key] += 1;
	}
	public static void AddCurrenTimeVar(StringBuilder sb)
	{
		var now = DateTime.Now;
		sb.Append("g_report_at=\"");
		sb.Append(now.Year).Append("-").Append( string.Format("{0:D2}",now.Month)).Append("-").Append(string.Format("{0:D2}",now.Day)).Append(" ");
		sb.Append(string.Format("{0:D2}", now.Hour)).Append(":").Append(string.Format("{0:D2}", now.Minute)).Append(":").Append(string.Format("{0:D2}", now.Second));
		sb.Append("\";\n");
	}
	public static void AddPlatformVar(StringBuilder sb,string platform)
	{
		sb.Append("g_report_platform=\"");
		sb.Append(platform);
		sb.Append("\";\n");
	}

}

