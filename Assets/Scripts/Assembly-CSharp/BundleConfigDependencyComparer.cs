using System.Collections.Generic;

public class BundleConfigDependencyComparer : IComparer<BundleConfigDependency>
{
	public int Compare(BundleConfigDependency a, BundleConfigDependency b)
	{
		return string.Compare(a.asset, b.asset);
	}
}
