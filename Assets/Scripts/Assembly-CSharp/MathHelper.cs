using System;
using UnityEngine;

public static class MathHelper
{
	public static Vector3 ProjectPointOntoPlane(Vector3 pointOnPlane, Vector3 planeNormal, Vector3 point)
	{
		Vector3 lhs = pointOnPlane - point;
		float num = Vector3.Dot(lhs, -planeNormal);
		return point + num * -planeNormal;
	}

	public static Vector3 ProjectPointOntoLine(Vector3 vecLineBegin, Vector3 vecLineEnd, Vector3 point)
	{
		Vector3 lhs = point - vecLineBegin;
		Vector3 rhs = vecLineEnd - vecLineBegin;
		float num = Vector3.Dot(lhs, rhs) / rhs.magnitude;
		if (num < 0f)
		{
			num = 0f;
		}
		else if (num > rhs.magnitude)
		{
			num = rhs.magnitude;
		}
		return vecLineBegin + num * rhs.normalized;
	}

	public static Vector3 ProjectPointOntoSphere(Vector3 vecCenterPos, float fRadius, Vector3 vP)
	{
		Vector3 vecNormal = Vector3.one;
		return ProjectPointOntoSphere(vecCenterPos, fRadius, vP, out vecNormal);
	}

	public static Vector3 ProjectPointOntoSphere(Vector3 vecCenterPos, float fRadius, Vector3 vP, out Vector3 vecNormal)
	{
		vecNormal = (vP - vecCenterPos).normalized;
		return vecCenterPos + vecNormal * fRadius;
	}

	public static Vector3 ProjectPointOntoTriangle(Vector3 vA, Vector3 vB, Vector3 vC, Vector3 vP)
	{
		Vector3 normalOfTriangle = GetNormalOfTriangle(vA, vB, vC);
		Vector3 vector = ProjectPointOntoPlane(vA, normalOfTriangle, vP);
		Vector3 lhs = vB - vA;
		lhs.Normalize();
		Vector3 vector2 = vC - vA;
		vector2.Normalize();
		Vector3 lhs2 = vB - vC;
		lhs2.Normalize();
		Vector3 vector3 = vA - vC;
		vector3.Normalize();
		Vector3 rhs = vector - vA;
		rhs.Normalize();
		Vector3 rhs2 = vector - vC;
		rhs2.Normalize();
		float num = Vector3.Dot(lhs, vector2);
		float num2 = Vector3.Dot(lhs2, vector3);
		float num3 = Vector3.Dot(lhs, rhs);
		float num4 = Vector3.Dot(vector2, rhs);
		float num5 = Vector3.Dot(lhs2, rhs2);
		float num6 = Vector3.Dot(vector3, rhs2);
		if (num3 >= num && num4 >= num && num6 >= num2 && num5 >= num2)
		{
			return vector;
		}
		Vector3 vector4 = ProjectPointOntoLine(vA, vB, vector);
		Vector3 vector5 = ProjectPointOntoLine(vA, vC, vector);
		Vector3 vector6 = ProjectPointOntoLine(vB, vC, vector);
		float sqrMagnitude = (vector - vector4).sqrMagnitude;
		float sqrMagnitude2 = (vector - vector5).sqrMagnitude;
		float sqrMagnitude3 = (vector - vector6).sqrMagnitude;
		if (sqrMagnitude < sqrMagnitude2 && sqrMagnitude < sqrMagnitude3)
		{
			return vector4;
		}
		if (sqrMagnitude2 < sqrMagnitude && sqrMagnitude2 < sqrMagnitude3)
		{
			return vector5;
		}
		return vector6;
	}

	public static bool LinePlaneIntersection(Vector3 pointOnPlane, Vector3 planeNormal, Vector3 linePointA, Vector3 linePointB, out Vector3 vecResult)
	{
		Vector3 vector = Vector3.Normalize(linePointB - linePointA);
		float num = Vector3.Dot(-linePointA, planeNormal);
		float num2 = Vector3.Dot(pointOnPlane, planeNormal);
		float num3 = Vector3.Dot(vector, planeNormal);
		float num4 = (num + num2) / num3;
		vecResult = linePointA + num4 * vector;
		Vector3 vector2 = linePointA - pointOnPlane;
		Vector3 vector3 = linePointB - pointOnPlane;
		Vector3 normalized = vector2.normalized;
		Vector3 normalized2 = vector3.normalized;
		if ((Vector3.Dot(normalized, planeNormal) > 0f && Vector3.Dot(normalized2, planeNormal) < 0f) || (Vector3.Dot(normalized, planeNormal) < 0f && Vector3.Dot(normalized2, planeNormal) > 0f))
		{
			return true;
		}
		return false;
	}

	public static Vector3 GetPointOnCurve(Vector3 p1, Vector3 k1, Vector3 p2, float t)
	{
		float num = 1f - t;
		Vector3 vector = num * p1 + t * k1;
		Vector3 vector2 = num * k1 + t * p2;
		return num * vector + t * vector2;
	}

	public static Vector3 GetPointOnCurve(Vector3 p1, Vector3 k1, Vector3 k2, Vector3 p2, float t)
	{
		float num = 1f - t;
		Vector3 vector = num * p1 + t * k1;
		Vector3 vector2 = num * k1 + t * k2;
		Vector3 vector3 = num * k2 + t * p2;
		Vector3 vector4 = num * vector + t * vector2;
		Vector3 vector5 = num * vector2 + t * vector3;
		return num * vector4 + t * vector5;
	}

	public static Vector3 GetPointOnCurveWithoutInfluencePoint(Vector3 p1, Vector3 p2, Vector3 p3, float t)
	{
		float num = 0.5f;
		float num2 = num * num;
		Vector3 vector = p1 * num2;
		Vector3 vector2 = p3 * num2;
		Vector3 k = (p1 - 2f * vector - vector + vector2 - p2) / (-2f * num + 2f * num2);
		return GetPointOnCurve(p1, k, p3, t);
	}

	public static float GetCurveLength(Vector3 p1, Vector3 p2, Vector3 p3, int iNumOfPartitions = 40)
	{
		float num = 0f;
		float num2 = 0f;
		Vector3 vector = Vector3.zero;
		for (int i = 0; i < iNumOfPartitions; i++)
		{
			if (i == 0)
			{
				num = 0f;
				vector = GetPointOnCurveWithoutInfluencePoint(p1, p2, p3, num);
				continue;
			}
			num = (float)i / (float)iNumOfPartitions;
			Vector3 pointOnCurveWithoutInfluencePoint = GetPointOnCurveWithoutInfluencePoint(p1, p2, p3, num);
			Vector3 vector2 = pointOnCurveWithoutInfluencePoint - vector;
			vector = pointOnCurveWithoutInfluencePoint;
			num2 += Mathf.Abs(vector2.magnitude);
		}
		return num2;
	}

	public static Vector3 FromPointToPointOnCurve(Vector3 point, Vector3 p1, Vector3 p2, Vector3 p3, float t)
	{
		Vector3 pointOnCurveWithoutInfluencePoint = GetPointOnCurveWithoutInfluencePoint(p1, p2, p3, t);
		return pointOnCurveWithoutInfluencePoint - point;
	}

	public static Vector3 GetClosestPointOnCurve(Vector3 point, Vector3 p1, Vector3 p2, Vector3 p3, out float fT, int iNumOfPartitions = 40)
	{
		Vector3 result = p1;
		float num = float.MaxValue;
		float num2 = 0f;
		float num3 = 0f;
		for (int i = 0; i < iNumOfPartitions; i++)
		{
			num3 = (float)i / (float)iNumOfPartitions;
			Vector3 vector = FromPointToPointOnCurve(point, p1, p2, p3, num3);
			if (vector.sqrMagnitude < num)
			{
				num = vector.sqrMagnitude;
				result = vector;
				num2 = num3;
			}
		}
		fT = num2;
		return result;
	}

	public static float GetTOnLine(Vector3 a, Vector3 b, Vector3 p)
	{
		float sqrMagnitude = (b - a).sqrMagnitude;
		float sqrMagnitude2 = (p - a).sqrMagnitude;
		return sqrMagnitude2 / sqrMagnitude;
	}

	public static Vector3 GetNormalOfTriangle(Vector3 a, Vector3 b, Vector3 c)
	{
		Vector3 lhs = b - a;
		Vector3 rhs = c - a;
		lhs.Normalize();
		rhs.Normalize();
		Vector3 result = Vector3.Cross(lhs, rhs);
		result.Normalize();
		return result;
	}

	public static float GetAreaOfTriangle(Vector3 A, Vector3 B, Vector3 C)
	{
		Vector3 lhs = B - A;
		Vector3 rhs = C - B;
		Vector3 normalOfTriangle = GetNormalOfTriangle(A, B, C);
		Vector3 lhs2 = Vector3.Cross(lhs, rhs);
		return Vector3.Dot(lhs2, normalOfTriangle) / 2f;
	}

	public static Vector3 GetTriangleBarycentricCoordinate(Vector3 A, Vector3 B, Vector3 C, Vector3 P)
	{
		float areaOfTriangle = GetAreaOfTriangle(A, B, C);
		float x = GetAreaOfTriangle(P, B, C) / areaOfTriangle;
		float y = GetAreaOfTriangle(A, P, C) / areaOfTriangle;
		float z = GetAreaOfTriangle(P, A, B) / areaOfTriangle;
		return new Vector3(x, y, z);
	}

	public static bool IsPointOnTriangle(Vector3 A, Vector3 B, Vector3 C, Vector3 P)
	{
		Vector3 triangleBarycentricCoordinate = GetTriangleBarycentricCoordinate(A, B, C, P);
		return IsPointOnTriangle(triangleBarycentricCoordinate);
	}

	public static bool IsPointOnTriangle(Vector3 vecBary)
	{
		float num = vecBary.x + vecBary.y + vecBary.z;
		if (num <= 1f && vecBary.x >= 0f && vecBary.y >= 0f && vecBary.z >= 0f)
		{
			return true;
		}
		return false;
	}

	public static Vector3 GetPointOnTriangleFromBarycentricCoordinate(Vector3 A, Vector3 B, Vector3 C, Vector3 bary)
	{
		return bary.x * A + bary.y * B + bary.z * C;
	}

	public static float HFov2VFov(float HFovInDeg, float ScreenWidth, float ScreenHeight)
	{
		float num = ScreenWidth / ScreenHeight;
		float num2 = HFovInDeg * ((float)Math.PI / 180f);
		float num3 = 2f * Mathf.Atan(Mathf.Tan(num2 / 2f) / num);
		return 57.29578f * num3;
	}

	public static float VFov2HFov(float VFovInDeg, float ScreenWidth, float ScreenHeight)
	{
		float num = ScreenWidth / ScreenHeight;
		float num2 = VFovInDeg * ((float)Math.PI / 180f);
		float num3 = 2f * Mathf.Atan(Mathf.Tan(num2 / 2f) * num);
		return 57.29578f * num3;
	}

	public static Quaternion GetQuaternionConjugate(Quaternion quat)
	{
		Quaternion result = quat;
		result.x *= -1f;
		result.y *= -1f;
		result.z *= -1f;
		return result;
	}

	public static float GetQuaternionMagnitude(Quaternion quat)
	{
		float num = quat.x * quat.x + quat.y * quat.y + quat.z * quat.z;
		float num2 = quat.w * quat.w;
		return Mathf.Sqrt(num + num2);
	}

	public static Quaternion GetQuaternionInverse(Quaternion quat)
	{
		Quaternion quaternionConjugate = GetQuaternionConjugate(quat);
		float quaternionMagnitude = GetQuaternionMagnitude(quat);
		quaternionConjugate.x /= quaternionMagnitude;
		quaternionConjugate.y /= quaternionMagnitude;
		quaternionConjugate.z /= quaternionMagnitude;
		quaternionConjugate.w /= quaternionMagnitude;
		return quaternionConjugate;
	}

	public static float GetAngleFromQuaternion(Quaternion quat)
	{
		return Mathf.Acos(quat.w) * 2f;
	}

	public static Vector3 GetRotationVectorFromQuaternion(Quaternion quat)
	{
		float angleFromQuaternion = GetAngleFromQuaternion(quat);
		float f = angleFromQuaternion / 2f;
		float num = Mathf.Sin(f);
		float x = quat.x / num;
		float y = quat.y / num;
		float z = quat.z / num;
		Vector3 result = new Vector3(x, y, z);
		result.Normalize();
		return result;
	}

	public static Vector3 GetLookVectorFromQuaternion(Quaternion quat)
	{
		Vector3 result = quat * Vector3.forward;
		result.Normalize();
		return result;
	}

	public static Vector3 GetRightVectorFromQuaternion(Quaternion quat)
	{
		Vector3 result = quat * Vector3.right;
		result.Normalize();
		return result;
	}

	public static Vector3 GetUpVectorFromQuaternion(Quaternion quat)
	{
		Vector3 result = quat * Vector3.up;
		result.Normalize();
		return result;
	}
}
