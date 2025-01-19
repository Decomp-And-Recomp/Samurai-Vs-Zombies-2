using System;
using UnityEngine;

public class MathUtil
{
	public class PosRot
	{
		public Vector3 pos;

		public Quaternion rot;

		public PosRot()
		{
			pos = Vector3.zero;
			rot = Quaternion.identity;
		}

		public PosRot(Vector3 pos, Quaternion rot)
		{
			this.pos = pos;
			this.rot = rot;
		}
	}

	public enum EInterpType
	{
		INTERP_TYPE_LINEAR = 0,
		INTERP_TYPE_EASE_TO = 1,
		INTERP_TYPE_EASE_FROM = 2,
		INTERP_TYPE_EASE_BETWEEN = 3
	}

	public class InterpFloat
	{
		private float m_start;

		private float m_end;

		private float m_current;

		private float m_time;

		private float m_timer;

		private EInterpType m_interpType;

		public InterpFloat()
		{
		}

		public InterpFloat(float value)
		{
			m_current = value;
		}

		public void InterpTo(float end, float time, EInterpType interpType)
		{
			m_start = m_current;
			m_end = end;
			m_timer = 0f;
			m_time = time;
			m_interpType = interpType;
			if (time <= 0f)
			{
				m_current = end;
			}
		}

		public float Get()
		{
			return m_current;
		}

		public void Set(float value)
		{
			m_current = value;
			m_time = (m_timer = 0f);
		}

		public void Update(float dt)
		{
			if (m_timer < m_time)
			{
				m_timer += dt;
				float num = EaseAsType(m_interpType, m_timer / m_time);
				m_current = m_start + (m_end - m_start) * num;
			}
		}

		public override string ToString()
		{
			return m_current.ToString();
		}

		public static implicit operator float(InterpFloat interpFloat)
		{
			return interpFloat.m_current;
		}
	}

	public class TriBool
	{
		private int m_value;

		public static TriBool NotSet = new TriBool();

		public static TriBool True = new TriBool(true);

		public static TriBool False = new TriBool(false);

		public TriBool()
		{
			m_value = -1;
		}

		public TriBool(bool b)
		{
			m_value = (b ? 1 : 0);
		}

		public TriBool(TriBool other)
		{
			m_value = other.m_value;
		}

		public bool IsSet()
		{
			return m_value != -1;
		}

		public override string ToString()
		{
			return (m_value == -1) ? "NotSet" : ((m_value != 0) ? "true" : "false");
		}

		public static implicit operator bool(TriBool option)
		{
			return option.m_value == 1;
		}

		public static bool operator !(TriBool option)
		{
			return option.m_value != 1;
		}
	}

	public static float WrapDegrees(float a)
	{
		return (!(a < 0f)) ? ((a + 180f) % 360f - 180f) : ((a - 180f) % 360f + 180f);
	}

	public static float RotationDir(float a, float b)
	{
		return WrapDegrees(b - a);
	}

	public static float Heading(Vector3 dir)
	{
		if (dir == Vector3.zero)
		{
			return 0f;
		}
		return Mathf.Atan2(dir.x, dir.z) * 57.29578f;
	}

	public static float Pitch(Vector3 dir)
	{
		if (dir == Vector3.zero)
		{
			return 0f;
		}
		return Mathf.Asin(Mathf.Clamp(dir.y / dir.magnitude, -1f, 1f)) * 57.29578f;
	}

	public static float Roll(Vector3 vForward, Vector3 vRolledUp)
	{
		if (vForward.y > -0.0001f && vForward.y < 0.0001f)
		{
			return 0f;
		}
		Vector3 vector = Vector3.Cross(Vector3.Cross(vForward, Vector3.up), vForward);
		float num = Vector3.Angle(vRolledUp, vector);
		if (Vector3.Dot(Vector3.Cross(vRolledUp, vector), vForward) < 0f)
		{
			num = 0f - num;
		}
		return num;
	}

	public static Vector3 HeadingToVector3(float heading)
	{
		float x = Mathf.Sin(heading * ((float)Math.PI / 180f));
		float z = Mathf.Cos(heading * ((float)Math.PI / 180f));
		return new Vector3(x, 0f, z);
	}

	public static float Angle(Quaternion q)
	{
		float num = q.w;
		if (num < -1f)
		{
			num = -1f;
		}
		else if (num > 1f)
		{
			num = 1f;
		}
		return Mathf.Acos(num) * 2f * 57.29578f;
	}

	public static Vector3 forwardAxis(Quaternion q)
	{
		float x = q.x;
		float y = q.y;
		float z = q.z;
		float w = q.w;
		float num = x + x;
		float num2 = y + y;
		float num3 = num * w;
		float num4 = num2 * w;
		float num5 = num * x;
		float num6 = num * z;
		float num7 = num2 * z;
		float num8 = num2 * y;
		return new Vector3(num6 - num4, num7 + num3, 1f - (num5 + num8));
	}

	public static Vector3 TransformVector(Quaternion q, Vector3 v)
	{
		Matrix4x4 matrix4x = default(Matrix4x4);
		matrix4x.SetTRS(Vector3.zero, q, new Vector3(1f, 1f, 1f));
		return matrix4x.MultiplyPoint3x4(v);
	}

	public static float SmoothTo(float current, float dest, float smoothSpeed, float dt)
	{
		return SmoothTo(current, dest, smoothSpeed, dt, 0.002f);
	}

	public static float SmoothTo(float current, float dest, float smoothSpeed, float dt, float extraMovementEpsilon)
	{
		float num = Mathf.Min(smoothSpeed * dt, 1f);
		if (num == 1f)
		{
			return dest;
		}
		float num2 = dest - current;
		float num3 = Mathf.Abs(num2);
		if (num3 <= extraMovementEpsilon * dt)
		{
			return dest;
		}
		float num4 = num2 * num + Mathf.Sign(num2) * extraMovementEpsilon * dt;
		if (Mathf.Abs(num4) >= num3)
		{
			return dest;
		}
		return current + num4;
	}

	public static Vector3 SmoothTo(Vector3 current, Vector3 dest, float smoothSpeed, float dt)
	{
		return SmoothTo(current, dest, smoothSpeed, dt, 0.002f);
	}

	public static Vector3 SmoothTo(Vector3 current, Vector3 dest, float smoothSpeed, float dt, float extraMovementEpsilon)
	{
		float num = Mathf.Min(smoothSpeed * dt, 1f);
		if (num == 1f)
		{
			return dest;
		}
		Vector3 vector = dest - current;
		float magnitude = vector.magnitude;
		if (magnitude <= extraMovementEpsilon * dt)
		{
			return dest;
		}
		Vector3 vector2 = vector * num + vector / magnitude * extraMovementEpsilon * dt;
		if (vector2.sqrMagnitude >= magnitude * magnitude)
		{
			return dest;
		}
		return current + vector2;
	}

	public static float SmoothToDegrees(float current, float dest, float smoothSpeed, float dt)
	{
		return SmoothToDegrees(current, dest, smoothSpeed, dt, 1f);
	}

	public static float SmoothToDegrees(float current, float dest, float smoothSpeed, float dt, float extraMovementEpsilon)
	{
		float num = Mathf.Min(smoothSpeed * dt, 1f);
		if (num == 1f)
		{
			return dest;
		}
		float num2 = RotationDir(current, dest);
		float num3 = Mathf.Abs(num2);
		if (num3 <= extraMovementEpsilon * dt)
		{
			return dest;
		}
		float num4 = num2 * num + Mathf.Sign(num2) * extraMovementEpsilon * dt;
		if (Mathf.Abs(num4) >= num3)
		{
			return dest;
		}
		return current + num4;
	}

	public static float ClampScale(float f, float c1, float c2, float s1, float s2)
	{
		if (c2 == c1)
		{
			return s2;
		}
		if (c2 > c1)
		{
			return (Mathf.Clamp(f, c1, c2) - c1) / (c2 - c1) * (s2 - s1) + s1;
		}
		return (0f - Mathf.Clamp(0f - f, 0f - c1, 0f - c2) - c1) / (c2 - c1) * (s2 - s1) + s1;
	}

	public static int ClampScaleInt(float f, float c1, float c2, int s1, int s2)
	{
		return Mathf.Clamp((int)ClampScale(f, c1, c2, s1, s2 + 1), s1, s2);
	}

	public static float EaseAsType(EInterpType type, float a)
	{
		switch (type)
		{
		case EInterpType.INTERP_TYPE_LINEAR:
			return Mathf.Clamp01(a);
		case EInterpType.INTERP_TYPE_EASE_TO:
			return EaseTo(a);
		case EInterpType.INTERP_TYPE_EASE_FROM:
			return EaseFrom(a);
		default:
			return EaseBetween(a);
		}
	}

	public static float EaseBetween(float a)
	{
		if (a <= 0f)
		{
			return 0f;
		}
		if (a < 0.5f)
		{
			return 2f * a * a;
		}
		if (a < 1f)
		{
			return 1f - 2f * (1f - a) * (1f - a);
		}
		return 1f;
	}

	public static float EaseFrom(float a)
	{
		if (a <= 0f)
		{
			return 0f;
		}
		if (a < 1f)
		{
			return a * a;
		}
		return 1f;
	}

	public static float EaseTo(float a)
	{
		return 1f - EaseFrom(1f - a);
	}

	public static Vector3 computeTrajectoryBySpeed(Vector3 vPosStart, Vector3 vPosEnd, float fStartVelocity, float alternateGravityAccel)
	{
		float num = 0f - alternateGravityAccel;
		Vector3 vector = vPosEnd - vPosStart;
		Vector3 vector2 = vector;
		vector2.y = 0f;
		Vector3 result = default(Vector3);
		float num2;
		if (vector2.sqrMagnitude < 1E-05f)
		{
			result.x = 0f;
			result.z = 0f;
			num2 = fStartVelocity * fStartVelocity + 2f * num * vector.y;
			if (num2 < 0f)
			{
				result.y = Mathf.Sqrt(-2f * num * vector.y);
			}
			else
			{
				result.y = fStartVelocity;
			}
			return result;
		}
		float num3 = Mathf.Max(vector2.magnitude, 0.001f);
		float num4 = vector.x / num3;
		float num5 = vector.z / num3;
		float num6 = 0.5f * num * num3 * num3 / (fStartVelocity * fStartVelocity);
		float num7 = num3;
		float num8 = 0.5f * num * num3 * num3 / (fStartVelocity * fStartVelocity) - vector.y;
		num2 = num7 * num7 - 4f * num6 * num8;
		float y;
		float f;
		if (num2 < 0f)
		{
			float num9 = num3 * Mathf.Sqrt(num / (vector.y - vector.magnitude));
			num6 = 0.5f * num * num3 * num3 / (num9 * num9);
			num7 = num3;
			y = 0.5f * (0f - num7) / num6;
			f = Mathf.Atan2(y, 1f);
			result.x = num9 * Mathf.Cos(f) * num4;
			result.z = num9 * Mathf.Cos(f) * num5;
			result.y = num9 * Mathf.Sin(f);
			return result;
		}
		float num10 = Mathf.Sqrt(num2);
		float num11 = 0.5f * (0f - num7 + num10) / num6;
		float num12 = 0.5f * (0f - num7 - num10) / num6;
		y = ((!(((!(num11 >= 0f)) ? (0f - num11) : num11) < ((!(num12 >= 0f)) ? (0f - num12) : num12))) ? num12 : num11);
		f = Mathf.Atan2(y, 1f);
		result.x = fStartVelocity * Mathf.Cos(f) * num4;
		result.z = fStartVelocity * Mathf.Cos(f) * num5;
		result.y = fStartVelocity * Mathf.Sin(f);
		return result;
	}

	public static GameObject InstantiateAtGob(UnityEngine.Object Prefab, GameObject dest, GameObject childAttachPoint)
	{
		return (GameObject)UnityEngine.Object.Instantiate(Prefab, dest.transform.position, dest.transform.rotation);
	}

	public static bool Overlap(Rect r1, Rect r2)
	{
		return r1.xMax >= r2.x && r1.x < r2.xMax && r1.yMax >= r2.y && r1.y < r2.yMax;
	}

	public static bool IsInScrollView(Rect r, Rect scrollView, Rect scrollPosition, Vector2 scroll)
	{
		Vector2 vector = new Vector2(scrollPosition.x, scrollPosition.y) - scroll - new Vector2(scrollView.x, scrollView.y);
		Rect r2 = new Rect(r.x + vector.x, r.y + vector.y, r.width, r.height);
		return Overlap(r2, scrollPosition);
	}
}
