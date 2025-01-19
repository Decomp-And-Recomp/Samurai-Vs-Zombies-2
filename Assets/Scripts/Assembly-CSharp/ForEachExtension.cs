using System;
using System.Collections.Generic;

public static class ForEachExtension
{
	public static void ForEachWithIndex<T>(this IEnumerable<T> enumerable, Action<T, int> handler)
	{
		int num = 0;
		foreach (T item in enumerable)
		{
			handler(item, num++);
		}
	}
}
