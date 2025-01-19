using UnityEngine;

public class Projectile
{
	public const float kArcHeightDistPercent = 0.2f;

	public const float kMinZDistToArcShot = 3f;

	public const float kMaxYDistToNotArcShot = 0.5f;

	public string type;

	public Character shooter;

	public virtual bool isDone
	{
		get
		{
			return false;
		}
	}

	public GameObject gameObject { get; protected set; }

	public Transform transform { get; protected set; }

	public Vector3 cachedTargetPos { get; protected set; }

	public virtual void Update()
	{
	}

	public virtual void Destroy()
	{
	}
}
