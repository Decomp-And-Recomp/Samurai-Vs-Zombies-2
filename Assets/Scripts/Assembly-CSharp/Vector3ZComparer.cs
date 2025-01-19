using System.Collections.Generic;
using UnityEngine;

public class Vector3ZComparer : IComparer<Vector3>
{
	public int Compare(Vector3 a, Vector3 b)
	{
		return Mathf.CeilToInt(a.z - b.z);
	}
}
