using System.Collections.Generic;
using UnityEngine;

public class RailManager : WeakGlobalInstance<RailManager>
{
	private enum ERailType
	{
		height = 0,
		backX = 1,
		frontX = 2
	}

	private List<Vector3> mHeightPoints = new List<Vector3>();

	private List<Vector3> mBackPoints = new List<Vector3>();

	private List<Vector3> mFrontPoints = new List<Vector3>();

	private Vector3[] mBetweens = new Vector3[2];

	public RailManager()
	{
		SetUniqueInstance(this);
		AcquirePoints();
	}

	public float GetY(float z)
	{
		FindBetweens(z, ERailType.height);
		if (mBetweens[0].z == mBetweens[1].z)
		{
			return mBetweens[0].y;
		}
		float num = (z - mBetweens[0].z) / (mBetweens[1].z - mBetweens[0].z);
		return (mBetweens[1].y - mBetweens[0].y) * num + mBetweens[0].y;
	}

	public float GetMinX(float z)
	{
		FindBetweens(z, ERailType.backX);
		if (mBetweens[0].z == mBetweens[1].z)
		{
			return mBetweens[0].x;
		}
		float num = (z - mBetweens[0].z) / (mBetweens[1].z - mBetweens[0].z);
		return (mBetweens[1].x - mBetweens[0].x) * num + mBetweens[0].x;
	}

	public float GetMaxX(float z)
	{
		FindBetweens(z, ERailType.frontX);
		if (mBetweens[0].z == mBetweens[1].z)
		{
			return mBetweens[0].x;
		}
		float num = (z - mBetweens[0].z) / (mBetweens[1].z - mBetweens[0].z);
		return (mBetweens[1].x - mBetweens[0].x) * num + mBetweens[0].x;
	}

	private void AcquirePoints()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("RailHeight");
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			mHeightPoints.Add(gameObject.transform.position);
		}
		mHeightPoints.Sort(new Vector3ZComparer());
		array = GameObject.FindGameObjectsWithTag("RailBack");
		GameObject[] array3 = array;
		foreach (GameObject gameObject2 in array3)
		{
			mBackPoints.Add(gameObject2.transform.position);
		}
		mBackPoints.Sort(new Vector3ZComparer());
		array = GameObject.FindGameObjectsWithTag("RailFront");
		GameObject[] array4 = array;
		foreach (GameObject gameObject3 in array4)
		{
			mFrontPoints.Add(gameObject3.transform.position);
		}
		mFrontPoints.Sort(new Vector3ZComparer());
	}

	private void FindBetweens(float z, ERailType type)
	{
		List<Vector3> list = mHeightPoints;
		if (type == ERailType.backX)
		{
			list = mBackPoints;
		}
		if (type == ERailType.frontX)
		{
			list = mFrontPoints;
		}
		int count = list.Count;
		if (count == 0)
		{
			return;
		}
		if (z <= list[0].z)
		{
			mBetweens[0] = list[0];
			mBetweens[1] = list[0];
			return;
		}
		if (z >= list[count - 1].z)
		{
			mBetweens[0] = list[count - 1];
			mBetweens[1] = list[count - 1];
			return;
		}
		foreach (Vector3 item in list)
		{
			mBetweens[0] = mBetweens[1];
			mBetweens[1] = item;
			if (item.z > z)
			{
				return;
			}
		}
		mBetweens[1] = mBetweens[0];
	}
}
