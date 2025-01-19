using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SDFTree
{
	private static int kNumSpacesForTabs = 4;

	private SDFTree()
	{
	}

	public static bool Save(SDFTreeNode root, string filename)
	{
		bool result = true;
		TextWriter textWriter = new StreamWriter(filename);
		try
		{
			SaveRecursively(textWriter, root, 0);
		}
		catch
		{
			result = false;
		}
		finally
		{
			textWriter.Close();
		}
		return result;
	}

	public static bool Save(SDFTreeNode root, Stream stream)
	{
		bool result = true;
		TextWriter textWriter = new StreamWriter(stream);
		try
		{
			SaveRecursively(textWriter, root, 0);
		}
		catch
		{
			result = false;
		}
		finally
		{
			textWriter.Close();
		}
		return result;
	}

	public static bool Save(SDFTreeNode root, TextWriter tw)
	{
		bool result = true;
		try
		{
			SaveRecursively(tw, root, 0);
		}
		catch
		{
			result = false;
		}
		return result;
	}

	public static SDFTreeNode LoadFromResources(string filename)
	{
		try
		{
			TextAsset textAsset = (TextAsset)Resources.Load(filename, typeof(TextAsset));
			if (textAsset == null)
			{
				return null;
			}
			SDFTreeNode sDFTreeNode = LoadFromSingleString(textAsset.text);
			sDFTreeNode.ExpandLinks();
			return sDFTreeNode;
		}
		catch
		{
			return null;
		}
	}

	public static SDFTreeNode LoadFromBundle(AssetBundle bundle, string filename)
	{
		TextAsset textAsset = (TextAsset)bundle.LoadAsset(filename, typeof(TextAsset));
		if (textAsset == null)
		{
			return null;
		}
		SDFTreeNode sDFTreeNode = LoadFromSingleString(textAsset.text);
		sDFTreeNode.ExpandLinks();
		return sDFTreeNode;
	}

	public static SDFTreeNode LoadFromFile(string filename)
	{
		TextReader textReader = null;
		try
		{
			textReader = new StreamReader(filename);
		}
		catch
		{
			return null;
		}
		SDFTreeNode sDFTreeNode;
		try
		{
			sDFTreeNode = LoadFromSingleString(textReader.ReadToEnd());
		}
		catch
		{
			return null;
		}
		finally
		{
			textReader.Close();
		}
		sDFTreeNode.ExpandLinks();
		return sDFTreeNode;
	}

	public static SDFTreeNode LoadFromStream(Stream stream)
	{
		TextReader textReader = null;
		try
		{
			textReader = new StreamReader(stream);
		}
		catch
		{
			return null;
		}
		SDFTreeNode sDFTreeNode;
		try
		{
			sDFTreeNode = LoadFromSingleString(textReader.ReadToEnd());
		}
		catch
		{
			return null;
		}
		finally
		{
			textReader.Close();
		}
		sDFTreeNode.ExpandLinks();
		return sDFTreeNode;
	}

	private static void SaveRecursively(TextWriter tw, SDFTreeNode node, int level)
	{
		string text = new string('\t', level);
		foreach (KeyValuePair<string, string> attribute in node.attributes)
		{
			tw.WriteLine(text + attribute.Key + " = " + attribute.Value);
		}
		foreach (KeyValuePair<string, SDFTreeNode> child in node.childs)
		{
			tw.WriteLine(text + "[" + child.Key + "]");
			SaveRecursively(tw, child.Value, level + 1);
		}
	}

	public static SDFTreeNode LoadFromSingleString(string entireFile)
	{
		char[] separator = new char[2] { '\r', '\n' };
		string[] source = entireFile.Split(separator, StringSplitOptions.RemoveEmptyEntries);
		SmarterStringIterator iter = new SmarterStringIterator(source);
		SDFTreeNode sDFTreeNode = new SDFTreeNode();
		LoadBlock(sDFTreeNode, 0, iter);
		return sDFTreeNode;
	}

	private static void LoadBlock(SDFTreeNode node, int level, SmarterStringIterator iter)
	{
		while (iter.Current != null)
		{
			int num = FindLineLevel(iter.Current);
			string text = iter.Current.Trim();
			if (text.Length == 0 || text[0] == '/')
			{
				iter.MoveNext();
			}
			else
			{
				if (num < level || num > level)
				{
					break;
				}
				iter.MoveNext();
				if (text[0] == '[')
				{
					string text2 = ExtractBlockTitle(text);
					if (text2 != string.Empty)
					{
						SDFTreeNode node2 = new SDFTreeNode();
						node.SetChild(text2, node2);
						LoadBlock(node2, level + 1, iter);
					}
				}
				else if (text[0] == '{')
				{
					node.AddFileLink(ExtractFileLink(text));
				}
				else
				{
					KeyValuePair<string, string> keyValuePair = ExtractAttribute(text);
					if (keyValuePair.Key == string.Empty)
					{
						if (!(keyValuePair.Value == string.Empty))
						{
							node[node.attributeCount] = keyValuePair.Value;
						}
					}
					else
					{
						node[keyValuePair.Key] = keyValuePair.Value;
					}
				}
			}
			if (iter.Current == null)
			{
				break;
			}
		}
	}

	private static int FindLineLevel(string line)
	{
		if (line == null || line.Length == 0)
		{
			return 0;
		}
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < line.Length; i++)
		{
			switch (line[i])
			{
			case '\t':
				num++;
				continue;
			case ' ':
				num2++;
				if (num2 == kNumSpacesForTabs)
				{
					num2 = 0;
					num++;
				}
				continue;
			}
			break;
		}
		return num;
	}

	private static string ExtractBlockTitle(string blockTitle)
	{
		if (blockTitle[blockTitle.Length - 1] != ']')
		{
			return string.Empty;
		}
		return blockTitle.Substring(1, blockTitle.Length - 2);
	}

	private static string ExtractFileLink(string line)
	{
		if (line[line.Length - 1] != '}')
		{
			return string.Empty;
		}
		return line.Substring(1, line.Length - 2);
	}

	private static KeyValuePair<string, string> ExtractAttribute(string attributeLine)
	{
		int num = attributeLine.IndexOf('=');
		if (num == -1)
		{
			return new KeyValuePair<string, string>(string.Empty, attributeLine.Trim());
		}
		string[] array = new string[2]
		{
			attributeLine.Substring(0, num - 1).Trim(),
			attributeLine.Substring(num + 1).Trim()
		};
		return new KeyValuePair<string, string>(array[0], array[1]);
	}
}
