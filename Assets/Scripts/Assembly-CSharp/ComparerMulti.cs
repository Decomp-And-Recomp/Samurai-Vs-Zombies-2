using System.Collections.Generic;

public class ComparerMulti<T> : IComparer<T>
{
	private readonly List<IComparer<T>> comparers;

	public ComparerMulti(List<IComparer<T>> comparersToUse)
	{
		comparers = comparersToUse;
	}

	public int Compare(T a, T b)
	{
		int num = 0;
		foreach (IComparer<T> comparer in comparers)
		{
			num = comparer.Compare(a, b);
			if (num != 0)
			{
				break;
			}
		}
		return num;
	}
}
