using UnityEngine;

public class AutoScaler
{
	private Vector2 mGameResolution;

	private float mSizeScale = 1f;

	private Vector2 mCoordinateScale;

	private float mAspectRatioModifier;

	public Vector2 gameResolution
	{
		get
		{
			return mGameResolution;
		}
	}

	public float aspectRatioModifier
	{
		get
		{
			return mAspectRatioModifier;
		}
	}

	public AutoScaler(Vector2 gameResolution)
	{
		SetGameResolution(gameResolution);
	}

	public float toDevice(float v)
	{
		return v * mSizeScale;
	}

	public float toDeviceX(float x)
	{
		return x * mCoordinateScale.x;
	}

	public float toDeviceY(float y)
	{
		return y * mCoordinateScale.y;
	}

	public Vector2 toDevice(Vector2 v)
	{
		return new Vector2(v.x * mCoordinateScale.x, v.y * mCoordinateScale.y);
	}

	public Rect toDevice(Rect v)
	{
		return new Rect(v.xMin * mCoordinateScale.x, v.yMin * mCoordinateScale.y, v.width * mCoordinateScale.x, v.height * mCoordinateScale.y);
	}

	public float toGame(float v)
	{
		return v / mSizeScale;
	}

	public Vector2 toGame(Vector2 v)
	{
		return new Vector2(v.x / mCoordinateScale.x, v.y / mCoordinateScale.y);
	}

	public Rect toGame(Rect v)
	{
		return new Rect(v.xMin / mCoordinateScale.x, v.yMin / mCoordinateScale.y, v.width / mCoordinateScale.x, v.height / mCoordinateScale.y);
	}

	private void SetGameResolution(Vector2 nativeRez)
	{
		mGameResolution = nativeRez;
		mAspectRatioModifier = nativeRez.y / nativeRez.x / ((float)Screen.height / (float)Screen.width);
		mCoordinateScale = new Vector2((float)Screen.width / mGameResolution.x, (float)Screen.height / mGameResolution.y);
		if (mCoordinateScale.x < mCoordinateScale.y)
		{
			mSizeScale = mCoordinateScale.x;
		}
		else
		{
			mSizeScale = mCoordinateScale.y;
		}
	}
}
