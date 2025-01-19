using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public static class MemoryScanner
{
	public enum MemoryScannerFlags
	{
		MEMORY_SCANNER_FLAG_MERGE = 1,
		MEMORY_SCANNER_FLAG_DEBUG_DUMP = 2
	}

	[DllImport("mono")]
	public static extern long mono_gc_get_used_size();

	[DllImport("mono")]
	public static extern long mono_gc_get_heap_size();

	private static string PrependCount(string text, int count)
	{
		if (count > 1)
		{
			return count + "x | " + text;
		}
		return text;
	}

	private static List<string> MergeDuplicates(List<string> entries, bool onlyReturnDuplicates)
	{
		List<string> newList = new List<string>();
		int count = 1;
		string lastText = null;
		entries.ForEach(delegate(string text)
		{
			if (text != lastText)
			{
				if (!onlyReturnDuplicates || count > 1)
				{
					newList.Add(PrependCount(lastText, count));
				}
				count = 1;
				lastText = text;
			}
			else
			{
				count++;
			}
		});
		if (lastText != null)
		{
			newList.Add(PrependCount(lastText, count));
		}
		return newList;
	}

	private static bool IsPowerOfTwo(uint x)
	{
		return (x & (x - 1)) == 0;
	}

	private static int GetResourceSize<T>(T thisResource) where T : Object
	{
		int result = 0;
		switch (typeof(T).ToString())
		{
		case "UnityEngine.Texture2D":
		{
			Texture2D texture2D = (Texture2D)(object)thisResource;
			int num = texture2D.width * texture2D.height;
			switch (texture2D.format)
			{
			case TextureFormat.RGBA32:
			case TextureFormat.ARGB32:
			case TextureFormat.BGRA32:
				result = num * 4;
				break;
			case TextureFormat.RGB24:
				result = num * 3;
				break;
			case TextureFormat.ARGB4444:
			case TextureFormat.RGB565:
			case TextureFormat.RGBA4444:
				result = num * 2;
				break;
			case TextureFormat.Alpha8:
				result = num;
				break;
			case TextureFormat.PVRTC_RGB4:
			case TextureFormat.PVRTC_RGBA4:
				result = num / 2;
				break;
			case TextureFormat.PVRTC_RGB2:
			case TextureFormat.PVRTC_RGBA2:
				result = num / 4;
				break;
			}
			break;
		}
		default:
			result = 0;
			break;
		}
		return result;
	}

	public static void GetAllGenericResources<T>(out T[] allResources, out long[] resourceSizes, out long totalBytes) where T : Object
	{
		allResources = Resources.FindObjectsOfTypeAll(typeof(T)) as T[];
		resourceSizes = new long[allResources.Length];
		int num = 0;
		totalBytes = 0L;
		T[] array = allResources;
		foreach (T thisResource in array)
		{
			int resourceSize = GetResourceSize(thisResource);
			resourceSizes[num] = resourceSize;
			if (resourceSize > 0)
			{
				totalBytes += resourceSize;
			}
			num++;
		}
	}

	public static void GetAllGenericResources<T>(out T[] allResources, out long totalBytes) where T : Object
	{
		allResources = Resources.FindObjectsOfTypeAll(typeof(T)) as T[];
		int num = 0;
		totalBytes = 0L;
		T[] array = allResources;
		foreach (T thisResource in array)
		{
			int resourceSize = GetResourceSize(thisResource);
			if (resourceSize > 0)
			{
				totalBytes += resourceSize;
			}
			num++;
		}
	}

	private static string DocumentResource<T>(T thisResource, long bytes, string resourceName, string sortBy) where T : Object
	{
		string text = thisResource.name;
		if (text == string.Empty)
		{
			text = "[No Name]";
		}
		string text2;
		switch (sortBy)
		{
		case "size":
			text2 = string.Format("{1:0000000} {0} bytes | ", text, StringUtils.FormatAmountString(bytes));
			break;
		default:
			text2 = string.Format("{0} {1} bytes | ", text, StringUtils.FormatAmountString(bytes));
			break;
		}
		switch (resourceName)
		{
		case "UnityEngine.AudioClip":
		{
			AudioClip audioClip = (AudioClip)(object)thisResource;
			string text3 = text2;
			text2 = text3 + "  " + audioClip.length + " sec " + audioClip.samples + " samples";
			break;
		}
		case "UnityEngine.AnimationClip":
		{
			AnimationClip animationClip = (AnimationClip)(object)thisResource;
			text2 += string.Format(" length={0} wrap={1}", animationClip.length, animationClip.wrapMode);
			break;
		}
		case "UnityEngine.Mesh":
		{
			Mesh mesh = (Mesh)(object)thisResource;
			text2 += string.Format(" vertex#={0} tri#={1} normals?={2} tangents?={3}", mesh.vertexCount, mesh.triangles.Length, (mesh.normals.Length <= 0) ? "N" : "Y", (mesh.tangents.Length <= 0) ? "N" : "Y");
			break;
		}
		case "UnityEngine.Texture2D":
		{
			Texture2D texture2D = (Texture2D)(object)thisResource;
			string text3 = text2;
			text2 = text3 + " (" + texture2D.width + "x" + texture2D.height + ") " + texture2D.format.ToString();
			if (!IsPowerOfTwo((uint)texture2D.height) || !IsPowerOfTwo((uint)texture2D.width))
			{
				text2 += " <NPOT>";
			}
			break;
		}
		}
		text2 += " ";
		if (thisResource.GetType() != typeof(T))
		{
			text2 = text2.PadRight(80);
			text2 = text2 + "\t" + thisResource.GetType().ToString();
		}
		if (thisResource is Component)
		{
			text2 = text2.PadRight(80);
			text2 += " in ";
			Component component = thisResource as Component;
			if (!component.gameObject.activeInHierarchy)
			{
				text2 += "disabled ";
			}
			text2 = text2 + "object [" + component.gameObject.name + "] ";
		}
		return text2 + "\n";
	}

	private static List<string> FilterResources<T>(string resourceName, T[] allResources, string textFilter, long[] resourceSizes, long minSizeToList, string sortBy) where T : Object
	{
		List<string> list = new List<string>();
		for (int i = 0; i < allResources.Length; i++)
		{
			T thisResource = allResources[i];
			if (resourceSizes[i] == 0L || resourceSizes[i] > minSizeToList)
			{
				string text = DocumentResource(thisResource, resourceSizes[i], resourceName, sortBy);
				if (textFilter != string.Empty)
				{
					if (text.ToLower().Contains(textFilter.ToLower()))
					{
						list.Add(text);
					}
					else
					{
						list.Add(" . Does not match text filter [" + textFilter + "].");
					}
				}
				else
				{
					list.Add(text);
				}
			}
			else
			{
				list.Add(" . Does not match size filter [" + minSizeToList + "].");
			}
		}
		return list;
	}

	public static List<string> ListAllResources<T>(long minSizeToList = 1024, string textFilter = "", string sortBy = "") where T : Object
	{
		string text = typeof(T).ToString();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("-----************** List all " + text + " **************-----\n");
		T[] allResources;
		long[] resourceSizes;
		long totalBytes;
		GetAllGenericResources<T>(out allResources, out resourceSizes, out totalBytes);
		if (allResources.Length == 0)
		{
			stringBuilder.Append("---*** No resources ***---\n");
			return new List<string>();
		}
		List<string> list = FilterResources(text, allResources, textFilter, resourceSizes, minSizeToList, sortBy);
		float num = (float)((double)totalBytes / 1048576.0);
		num = (float)(int)(num * 100f) / 100f;
		list.Sort();
		list = MergeDuplicates(list, false);
		string item = string.Format("---*** [{0}] TOTAL MEMORY = {1} MB ***---\n", typeof(T).ToString(), num);
		list.Add(item);
		foreach (string item2 in list)
		{
			stringBuilder.Append(item2);
		}
		return list;
	}

	public static void ListAll()
	{
		ListHeapUsage();
		ListAllResources<Texture2D>(1024L, string.Empty, string.Empty);
		ListAllResources<Mesh>(1024L, string.Empty, string.Empty);
		ListAllResources<AnimationClip>(1024L, string.Empty, string.Empty);
		ListAllResources<AudioClip>(1024L, string.Empty, string.Empty);
	}

	public static List<string> GetHeapUsage()
	{
		List<string> list = new List<string>();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("-----************** About to list heap usage **************-----\n");
		double num = (double)mono_gc_get_used_size() / 1048576.0;
		double num2 = (double)mono_gc_get_heap_size() / 1048576.0;
		stringBuilder.AppendFormat("---*** Heap (MBs) - Used: {0:0.000}, Size: {1:0.000}, Remaining: {2:0.000} ***---\n", num, num2, num2 - num);
		list.Add(stringBuilder.ToString());
		if (Debug.isDebugBuild)
		{
			list.Add(string.Format("Heap used={0:0.000} mb, System mem={1:0.000} mb", (double)UnityEngine.Profiling.Profiler.usedHeapSize / 1048576.0, SystemInfo.systemMemorySize));
		}
		return list;
	}

	public static void ListHeapUsage()
	{
		DebugShowList(GetHeapUsage());
	}

	private static void DebugShowList(List<string> entries)
	{
		entries.ForEach(delegate
		{
		});
	}

	public static long HeapUsed()
	{
		return mono_gc_get_used_size();
	}

	public static long HeapSize()
	{
		return mono_gc_get_heap_size();
	}
}
