using UnityEngine;

public class RandomRangeInt
{
	public static int between(int min, int max)
	{
		if (min >= max)
		{
			return min;
		}
		if (min > max)
		{
			int num = max;
			max = min;
			min = num;
		}
		float f = (float)(max - min + 1) * Random.value;
		int value = Mathf.RoundToInt(f) + min;
		return Mathf.Clamp(value, min, max);
	}
}
