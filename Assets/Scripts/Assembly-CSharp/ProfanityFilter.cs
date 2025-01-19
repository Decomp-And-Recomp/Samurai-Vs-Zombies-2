using System;
using UnityEngine;

public class ProfanityFilter : MonoBehaviour
{
	private static int FindSurroundingWord(string testString, int index, string badWord, out string subWord)
	{
		subWord = string.Empty;
		index = testString.IndexOf(badWord, index);
		if (index < 0)
		{
			return index;
		}
		int num = index;
		int i = index + badWord.Length;
		while (num > 0 && testString[num - 1] != ' ')
		{
			num--;
		}
		for (; i < testString.Length && testString[i] != ' '; i++)
		{
		}
		subWord = testString.Substring(num, i - num);
		return index;
	}

	public static bool IsStringAcceptable(string testString)
	{
		testString = testString.ToLower();
		TextAsset textAsset = Resources.Load("Profanity/SortedBlackList") as TextAsset;
		string[] array = textAsset.text.Split('\r');
		string[] array2 = null;
		string[] array3 = array;
		foreach (string text in array3)
		{
			if (!testString.Contains(text))
			{
				continue;
			}
			if (array2 == null)
			{
				TextAsset textAsset2 = Resources.Load("Profanity/SortedWhiteList") as TextAsset;
				array2 = textAsset2.text.Split('\r');
			}
			int num = 0;
			while (num >= 0 && num < testString.Length)
			{
				string subWord;
				num = FindSurroundingWord(testString, num, text, out subWord);
				if (num < 0)
				{
					continue;
				}
				num += text.Length;
				if (text.Length > 2 || subWord.Length == text.Length)
				{
					int num2 = Array.BinarySearch(array2, subWord);
					if (num2 < 0)
					{
						return false;
					}
				}
			}
		}
		return true;
	}
}
