using UnityEngine;

public class fRange
{
	public float min;

	public float max;

	public fRange(float vMin, float vMax)
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

	public fRange()
	{
		min = 0f;
		max = 0f;
	}

	public float randInRange()
	{
		return Random.Range(min, max);
	}

	public bool isInRange(float val)
	{
		return min <= val && max > val;
	}
}
