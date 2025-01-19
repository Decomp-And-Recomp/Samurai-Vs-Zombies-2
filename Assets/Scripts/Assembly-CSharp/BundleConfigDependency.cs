using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BundleConfigDependency
{
	[Serializable]
	public class Entry
	{
		public class BundleDependencyComparer : IComparer<Entry>
		{
			public int Compare(Entry a, Entry b)
			{
				return string.Compare(a.asset, b.asset);
			}
		}

		public string asset;

		public List<string> dependencies = new List<string>();
	}

	public string asset;

	public List<GameObject> bundleReferences = new List<GameObject>();

	public List<Entry> bundleDependencies = new List<Entry>();

	public bool selected;

	public BundleConfigDependency(string assetPath, GameObject bundleReference)
	{
		asset = assetPath;
		bundleReferences = new List<GameObject>();
		bundleReferences.Add(bundleReference);
		selected = false;
	}

	public List<GameObject> BundlesThisDependencyIsLocalTo()
	{
		List<GameObject> list = new List<GameObject>();
		foreach (GameObject bundleReference in bundleReferences)
		{
			bool flag = false;
			foreach (GameObject bundleReference2 in bundleReferences)
			{
				if (bundleReference != bundleReference2 && bundleReference.transform.IsChildOf(bundleReference2.transform))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.Add(bundleReference);
			}
		}
		return list;
	}
}
