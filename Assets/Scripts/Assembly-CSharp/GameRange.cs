using UnityEngine;

public class GameRange
{
	public float left;

	public float right;

	public GameRange(float l, float r)
	{
		left = l;
		right = r;
	}

	public bool Contains(float z)
	{
		return z >= left && z <= right;
	}

	public Vector3 getPushDirection(float z)
	{
		Vector3 result = new Vector3(0f, 0f, 0f);
		if (!Contains(z))
		{
			if (z < left)
			{
				result.z = 1f;
			}
			else if (z > right)
			{
				result.z = -1f;
			}
		}
		return result;
	}

	public void PivotAboutLeft()
	{
		float num = right - left;
		right = left;
		left = right - num;
	}
}
