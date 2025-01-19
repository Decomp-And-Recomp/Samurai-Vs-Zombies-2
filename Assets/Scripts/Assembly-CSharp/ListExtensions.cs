using System.Collections.Generic;
using UnityEngine;

public static class ListExtensions
{
	public static void Shuffle<T>(this IList<T> list)
	{
		int num = list.Count;
		while (num > 1)
		{
			num--;
			int index = Random.Range(0, num + 1);
			T value = list[index];
			list[index] = list[num];
			list[num] = value;
		}
	}

	public static void DeduplicateSortedList<T>(this List<T> list, IComparer<T> Comparer)
	{
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			if (i == 0 || Comparer.Compare(list[num - 1], list[i]) != 0)
			{
				list[num++] = list[i];
			}
		}
		list.RemoveRange(num, list.Count - num);
	}
}
