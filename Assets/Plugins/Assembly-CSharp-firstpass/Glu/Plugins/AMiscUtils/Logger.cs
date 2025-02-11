using System;
using System.Text;
using UnityEngine;

namespace Glu.Plugins.AMiscUtils
{
	public class Logger
	{
		private static readonly string[] LevelNames = GetLevelNames();

		private StringBuilder sb;

		private int level;

		private bool printStackTrace;

		public string Name { get; private set; }

		public Logger(string name)
		{
		}

		public void D(string format, params object[] args)
		{
			Log(0, format, args);
		}

		public void I(string format, params object[] args)
		{
			Log(1, format, args);
		}

		public void W(string format, params object[] args)
		{
			Log(2, format, args);
		}

		public void E(string format, params object[] args)
		{
			Log(3, format, args);
		}

		private static string[] GetLevelNames()
		{
			return new string[4] { "D", "I", "W", "E" };
		}

		private string Format(int level, string format, object[] args)
		{
			lock (sb)
			{
				sb.Remove(0, sb.Length);
				sb.Append(LevelNames[level]);
				sb.Append('/');
				sb.Append(Name);
				sb.Append(' ');
				if (args.Length > 0)
				{
					sb.AppendFormat(format, args);
				}
				else
				{
					sb.Append(format);
				}
				return sb.ToString();
			}
		}

		private void Log(int lvl, string format, object[] args)
		{
			if (lvl >= level)
			{
				string text = Format(lvl, format, args);
				if (!printStackTrace)
				{
					Console.Write(text);
				}
				else if (lvl < 2)
				{
					Debug.Log(text);
				}
				else if (lvl < 3)
				{
					Debug.LogWarning(text);
				}
				else
				{
					Debug.LogError(text);
				}
			}
		}
	}
}
