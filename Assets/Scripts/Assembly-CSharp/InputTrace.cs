using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Input/Input Trace")]
public class InputTrace : MonoBehaviour
{
	public class HitInfo
	{
		public GameObject target;

		public Camera camera;

		public RaycastHit raycastHit;

		public override string ToString()
		{
			return string.Format("camera={0}, raycastHit={1}, target={2}", (!(camera != null)) ? "null" : camera.ToString(), raycastHit.ToString(), (!(target != null)) ? null : target.name);
		}
	}

	private static int CompareCameraDepth(Camera firstCamera, Camera secondCamera)
	{
		return Convert.ToInt32(firstCamera.depth - secondCamera.depth);
	}

	private static int RaycastHitDistanceCompare(RaycastHit firstHit, RaycastHit secondHit)
	{
		return Convert.ToInt32(firstHit.distance - secondHit.distance);
	}

	private List<Camera> GetCamerasToTrace()
	{
		List<Camera> list = new List<Camera>();
		list.AddRange(Camera.allCameras);
		list.Sort(CompareCameraDepth);
		return list;
	}

	public List<Camera> FilterCamerasToTrace(List<Camera> cameras, Vector2 inputPosition)
	{
		List<Camera> list = new List<Camera>();
		foreach (Camera camera in cameras)
		{
			if (camera.pixelRect.Contains(inputPosition))
			{
				if (camera.clearFlags == CameraClearFlags.Skybox || camera.clearFlags == CameraClearFlags.Color)
				{
					list.Clear();
				}
				if (!(camera.farClipPlane <= 0f))
				{
					list.Add(camera);
				}
			}
		}
		return list;
	}

	private List<HitInfo> Trace(Vector2 inputPosition, List<Camera> cameras)
	{
		List<HitInfo> list = new List<HitInfo>();
		foreach (Camera camera in cameras)
		{
			RaycastHit[] array = Physics.RaycastAll(camera.ScreenPointToRay(inputPosition), camera.farClipPlane, camera.cullingMask);
			Array.Sort(array, RaycastHitDistanceCompare);
			RaycastHit[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				RaycastHit raycastHit = array2[i];
				HitInfo hitInfo = new HitInfo();
				if ((bool)raycastHit.rigidbody)
				{
					hitInfo.raycastHit = raycastHit;
					hitInfo.target = raycastHit.rigidbody.gameObject;
					hitInfo.camera = camera;
				}
				else
				{
					hitInfo.raycastHit = raycastHit;
					hitInfo.target = raycastHit.collider.gameObject;
					hitInfo.camera = camera;
				}
				list.Add(hitInfo);
			}
		}
		return list;
	}

	public List<HitInfo> Trace(Vector2 inputPosition)
	{
		List<Camera> camerasToTrace = GetCamerasToTrace();
		camerasToTrace = FilterCamerasToTrace(camerasToTrace, inputPosition);
		camerasToTrace.Reverse();
		return Trace(inputPosition, camerasToTrace);
	}
}
