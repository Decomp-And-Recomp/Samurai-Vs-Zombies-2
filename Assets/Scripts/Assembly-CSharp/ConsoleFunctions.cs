using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class ConsoleFunctions
{
	public static void register()
	{
		GrConsole instance = Singleton<GrConsole>.Instance;
		instance.add("?", help);
		instance.add("help", help);
		instance.add("toggleconsole", toggleConsole);
		instance.add("disableconsole", disableConsole);
		instance.add("stats", stats);
		instance.add("res", ListResources);
		instance.add("clear", ClearConsole);
		instance.add("killsavegame", KillSaveGame);
		instance.add("printconfig", PrintConfig);
		instance.add("var", DesignerVariableChange);
		instance.SetHotkey(0, "Clear", "clear", false);
		GrConsole.Hotkey hotkey = instance.SetHotkey(1, "Stats", string.Empty, true);
		hotkey.SetChildHotkey(0, "Toggle", "stats toggle", false);
		hotkey.SetChildHotkey(1, "Basic", "stats basic", false);
		hotkey.SetChildHotkey(2, "Assets", "stats assets", false);
		hotkey.SetChildHotkey(4, "Print Config", "printconfig", false);
		GrConsole.Hotkey hotkey2 = hotkey.SetChildHotkey(8, "Kill Save", string.Empty, true);
		hotkey2.SetChildHotkey(4, "Confirm", "killsavegame", false);
		GrConsole.Hotkey hotkey3 = instance.SetHotkey(2, "Resources", string.Empty, true);
		hotkey3.SetChildHotkey(0, "Textures", "res tex", false);
		hotkey3.SetChildHotkey(1, "Meshes", "res mesh", false);
		hotkey3.SetChildHotkey(2, "Anims", "res anim", false);
		hotkey3.SetChildHotkey(3, "Sound", "res sound", false);
		hotkey3.SetChildHotkey(4, "Materials", "res material", false);
		hotkey3.SetChildHotkey(5, "Objects", "res gameobject", false);
		hotkey3.SetChildHotkey(6, "Renderer", "res renderer", false);
		hotkey3.SetChildHotkey(7, "Collider", "res collider", false);
		hotkey3.SetChildHotkey(9, "Heap", "res heap", false);
		hotkey3.SetChildHotkey(10, "Glui", "res glui", false);
		hotkey3.SetChildHotkey(11, "Unload", "res unload", false);
		instance.SetHotkey(10, "Help", "help", false);
		instance.SetHotkey(11, "Exit", "toggleConsole", false);
		instance.add("actions", Actions);
		instance.add("data", PersistentData);
		GrConsole.Hotkey hotkey4 = instance.SetHotkey(9, "Glui", string.Empty, true);
		hotkey4.SetChildHotkey(0, "Action Log", "actions", false);
		hotkey4.SetChildHotkey(2, "Data Log", "data log", false);
		hotkey4.SetChildHotkey(3, "Data", "data scan", false);
	}

	public static bool? getOnOffSingleArg(string[] args, bool? defaultValue)
	{
		if (args.Length == 2)
		{
			switch (args[1])
			{
			case "on":
				return true;
			case "off":
				return false;
			}
		}
		return defaultValue;
	}

	private static string ExitConsole(string[] args)
	{
		GrConsole instance = Singleton<GrConsole>.Instance;
		instance.Visible = false;
		return string.Empty;
	}

	private static string ClearConsole(string[] args)
	{
		GrConsole instance = Singleton<GrConsole>.Instance;
		instance.Clear();
		return string.Empty;
	}

	private static string ListResources(string[] args)
	{
		if (args.Length >= 2)
		{
			List<string> list = new List<string>();
			bool flag = false;
			int num = 10240;
			string textFilter = string.Empty;
			string sortBy = string.Empty;
			for (int i = 2; i < args.Length; i++)
			{
				string text2 = args[i];
				int result;
				if (int.TryParse(text2, out result))
				{
					num = result;
					flag = true;
					continue;
				}
				if (text2 == "size")
				{
					sortBy = text2;
					continue;
				}
				textFilter = text2;
				if (!flag)
				{
					num = 0;
				}
			}
			List<string> outputHighlightFilter = new List<string>();
			string text3 = args[1];
			switch (text3.ToLower())
			{
			case "unload":
			{
				GrConsole instance = Singleton<GrConsole>.Instance;
				instance.addMessage(GrConsole.eType.Message, "Running UnloadUnusedAssets...");
				Resources.UnloadUnusedAssets();
				return string.Empty;
			}
			case "tex":
			case "texture":
				list = MemoryScanner.ListAllResources<Texture2D>(num, textFilter, sortBy);
				outputHighlightFilter.Add("<NPOT>");
				break;
			case "mesh":
				list = MemoryScanner.ListAllResources<Mesh>(num, textFilter, sortBy);
				break;
			case "anim":
			case "animation":
				list = MemoryScanner.ListAllResources<AnimationClip>(num, textFilter, sortBy);
				break;
			case "sound":
			case "audio":
				list = MemoryScanner.ListAllResources<AudioClip>(num, textFilter, sortBy);
				break;
			case "material":
			case "materials":
				if (!flag)
				{
					num = 0;
				}
				list = MemoryScanner.ListAllResources<Material>(num, textFilter, sortBy);
				break;
			case "gameobject":
			case "gameobjects":
				list = MemoryScanner.ListAllResources<GameObject>(num, textFilter, sortBy);
				break;
			case "renderer":
			case "renderers":
				list = MemoryScanner.ListAllResources<Renderer>(num, textFilter, sortBy);
				break;
			case "collider":
			case "colliders":
				list = MemoryScanner.ListAllResources<Collider>(num, textFilter, sortBy);
				break;
			case "heap":
				list = MemoryScanner.GetHeapUsage();
				break;
			}
			if (list.Count > 0)
			{
				GrConsole console = Singleton<GrConsole>.Instance;
				StringBuilder sb = new StringBuilder();
				console.addMessage(GrConsole.eType.Execute, "==> Asset list " + args[1]);
				sb.AppendLine("==> Asset list " + args[1]);
				list.ForEach(delegate(string text)
				{
					if (text != null)
					{
						GrConsole.eType type = GrConsole.eType.Network;
						bool flag2 = text.Contains("x |");
						if (outputHighlightFilter.Exists((string filter) => text.ToLower().Contains(filter.ToLower().ToString())))
						{
							type = GrConsole.eType.SlightWarning;
						}
						else if (flag2)
						{
							type = GrConsole.eType.Highlight;
						}
						console.addMessage(type, text);
						sb.AppendLine(text.Trim());
					}
				});
			}
			return string.Empty;
		}
		return "Specify what kind of resource to view.";
	}

	private static string stats(string[] args)
	{
		bool? onOffSingleArg = getOnOffSingleArg(args, true);
		if (onOffSingleArg.HasValue)
		{
			DebugMain instance = SingletonSpawningMonoBehaviour<DebugMain>.Instance;
			if (args.Length == 2)
			{
				switch (args[1])
				{
				case "toggle":
					instance.ShowDebugStats = !instance.ShowDebugStats;
					break;
				case "assets":
					instance.DebugStatsDisplay = DebugStats.DebugStatsDisplay.Memory_Assets;
					instance.ShowDebugStats = true;
					break;
				case "basic":
					instance.DebugStatsDisplay = DebugStats.DebugStatsDisplay.Basic;
					instance.ShowDebugStats = true;
					break;
				default:
					instance.ShowDebugStats = onOffSingleArg.Value;
					break;
				}
			}
		}
		return string.Empty;
	}

	private static string toggleConsole(string[] args)
	{
		GrConsole instance = Singleton<GrConsole>.Instance;
		instance.Visible = !instance.Visible;
		return string.Empty;
	}

	private static string disableConsole(string[] args)
	{
		DebugMain instance = SingletonSpawningMonoBehaviour<DebugMain>.Instance;
		instance.Allow4TouchActivate = false;
		GrConsole instance2 = Singleton<GrConsole>.Instance;
		instance2.Visible = false;
		return string.Empty;
	}

	private static string help(string[] args)
	{
		GrConsole instance = Singleton<GrConsole>.Instance;
		instance.printAllCommands();
		return string.Empty;
	}

	private static string KillSaveGame(string[] args)
	{
		if (Directory.Exists(Application.persistentDataPath))
		{
			string[] files = Directory.GetFiles(Application.persistentDataPath);
			foreach (string path in files)
			{
				try
				{
					File.Delete(path);
				}
				catch
				{
				}
			}
		}
		PlayerPrefs.DeleteAll();
		GrConsole instance = Singleton<GrConsole>.Instance;
		instance.addMessage(GrConsole.eType.Highlight, "Save Files Cleared : Suggest Restarting Game Now");
		return string.Empty;
	}

	public static string DesignerVariableChange(string[] args)
	{
		if (args.Length == 2 && args[1] == "clearall")
		{
			SingletonSpawningMonoBehaviour<DesignerVariables>.Instance.DumpAllVariableNames();
			return string.Empty;
		}
		string fullVariableNameFromShortcutName = SingletonSpawningMonoBehaviour<DesignerVariables>.Instance.GetFullVariableNameFromShortcutName(args[1]);
		if (fullVariableNameFromShortcutName == string.Empty)
		{
			return string.Empty;
		}
		string a = string.Empty;
		string b = string.Empty;
		string c = string.Empty;
		if (args.Length >= 5)
		{
			c = args[4];
		}
		if (args.Length >= 4)
		{
			b = args[3];
		}
		if (args.Length >= 3)
		{
			a = args[2];
		}
		if (args.Length == 2)
		{
			DesignerVariables.Param variableParam = SingletonSpawningMonoBehaviour<DesignerVariables>.Instance.GetVariableParam(fullVariableNameFromShortcutName);
			a = variableParam.A;
			b = variableParam.B;
			c = variableParam.C;
			return string.Empty;
		}
		SingletonSpawningMonoBehaviour<DesignerVariables>.Instance.SetVariable(fullVariableNameFromShortcutName, a, b, c);
		return string.Empty;
	}

	private static void OutputGluiLog(GrConsole console, GluiLog_Base log)
	{
		console.addMessage(GrConsole.eType.Message, "Log is disabled on target to conserve memory.");
	}

	private static string Actions(string[] args)
	{
		GluiActionLog gluiActionLog = (GluiActionLog)GluiActionLog_Base.Instance;
		if (gluiActionLog != null)
		{
			GrConsole instance = Singleton<GrConsole>.Instance;
			OutputGluiLog(instance, gluiActionLog);
			return string.Empty;
		}
		return "No GluiActionLog filter found.";
	}

	private static string PersistentData(string[] args)
	{
		if (args.Length >= 2)
		{
			switch (args[1])
			{
			case "log":
			{
				GluiPersistentDataLog instance3 = GluiPersistentDataLog.Instance;
				if (instance3 != null)
				{
					GrConsole instance4 = Singleton<GrConsole>.Instance;
					OutputGluiLog(instance4, instance3);
				}
				break;
			}
			case "scan":
			{
				GluiPersistentDataCache instance = SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance;
				if (!(instance != null))
				{
					break;
				}
				List<string> strings = new List<string>();
				instance.Nodes.ForEach(delegate(GluiPersistentDataCache.PersistentData node)
				{
					strings.Add(node.ToString());
				});
				strings.Sort();
				GrConsole instance2 = Singleton<GrConsole>.Instance;
				foreach (string item in strings)
				{
					instance2.addMessage(GrConsole.eType.Network, item);
				}
				break;
			}
			}
			return string.Empty;
		}
		return "Persistent data commands : log scan.";
	}

	private static string PrintConfig(string[] args)
	{
		Singleton<GrConsole>.Instance.addMessage(GrConsole.eType.Message, ConfigSchema.AllEntriesToString());
		return string.Empty;
	}
}
