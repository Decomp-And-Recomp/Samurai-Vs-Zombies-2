using UnityEngine;

public class iRange
{
	public int min;

	public int max;

	public iRange(int vMin, int vMax)
	{
		if (vMin <= vMax)
		{
			min = vMin;
			max = vMax;
		}
		else
		{
			max = vMin;
			min = vMax;
		}
	}

	public iRange()
	{
		min = 0;
		max = 0;
	}

	public int randInRange()
	{
		return Random.Range(min, max);
	}

	public bool isInRange(int val)
	{
		return min <= val && max > val;
	}
}
