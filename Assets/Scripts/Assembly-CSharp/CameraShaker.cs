using UnityEngine;

public class CameraShaker : SingletonSpawningMonoBehaviour<CameraShaker>
{
	private const float kCameraShakeReductionRate = 5f;

	private const float kMinCameraShakeRolloff = 5f;

	private const float kMaxCameraShakeRolloff = 10f;

	private static float mLastValidDeltaTime;

	private Transform mCameraTransform;

	private float mIntensity;

	private bool mFlipBit;

	public static void RequestShake(Vector3 shakeOrigin, float shakeIntensity)
	{
		if (shakeIntensity <= 0f)
		{
			return;
		}
		if (WeakGlobalMonoBehavior<InGameImpl>.Exists && (!SingletonSpawningMonoBehaviour<CameraShaker>.Exists || SingletonSpawningMonoBehaviour<CameraShaker>.Instance.mCameraTransform != WeakGlobalMonoBehavior<InGameImpl>.Instance.gameCamera.transform))
		{
			SingletonSpawningMonoBehaviour<CameraShaker>.Instance.mCameraTransform = WeakGlobalMonoBehavior<InGameImpl>.Instance.gameCamera.transform;
		}
		if (!(SingletonSpawningMonoBehaviour<CameraShaker>.Instance.mIntensity > 0f) && !(SingletonSpawningMonoBehaviour<CameraShaker>.Instance.mCameraTransform == null))
		{
			SingletonSpawningMonoBehaviour<CameraShaker>.Instance.StartShake(shakeOrigin, shakeIntensity);
			if (mLastValidDeltaTime == 0f)
			{
				mLastValidDeltaTime = Time.maximumDeltaTime;
			}
		}
	}

	private void StartShake(Vector3 shakeOrigin, float shakeIntensity)
	{
		float z = shakeOrigin.z;
		float z2 = mCameraTransform.position.z;
		float num = Mathf.Abs(z - z2);
		if (num <= 5f)
		{
			mIntensity = Mathf.Max(shakeIntensity, mIntensity);
		}
		else if (num < 10f)
		{
			mIntensity = Mathf.Max(mIntensity, shakeIntensity * (10f - num) / 5f);
		}
	}

	private void LateUpdate()
	{
		if (mIntensity <= 0f || mCameraTransform == null)
		{
			return;
		}
		Vector3 eulerAngles = mCameraTransform.eulerAngles;
		if (Mathf.Abs(eulerAngles.z) < 0.001f)
		{
			mFlipBit = !mFlipBit;
			if (mFlipBit)
			{
				eulerAngles.z = mIntensity;
			}
			else
			{
				eulerAngles.z = 0f - mIntensity;
			}
		}
		else
		{
			eulerAngles.z = 0f;
		}
		float num = Time.deltaTime;
		if (Time.timeScale == 0f)
		{
			num = mLastValidDeltaTime;
		}
		else if (Time.timeScale != 1f)
		{
			num /= Time.timeScale;
		}
		mLastValidDeltaTime = num;
		mIntensity -= Mathf.Max(5f * num, mIntensity * 5f * num);
		if (mIntensity <= 0f)
		{
			eulerAngles.z = 0f;
		}
		mCameraTransform.eulerAngles = eulerAngles;
	}
}
