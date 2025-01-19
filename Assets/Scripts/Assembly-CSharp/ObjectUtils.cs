using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectUtils
{
	public static void DestroyImmediate(UnityEngine.Object obj)
	{
		DestroyImmediate(obj, false);
	}

	public static void DestroyImmediate(UnityEngine.Object obj, bool allowDestroyingAssets)
	{
		if (!Application.isPlaying)
		{
			UnityEngine.Object.DestroyImmediate(obj);
		}
		else
		{
			UnityEngine.Object.Destroy(obj);
		}
	}

	public static Transform FindTransformInChildren(Transform parent, string name)
	{
		Transform transform = parent.transform.Find(name);
		if (transform == null)
		{
			for (int i = 0; i < parent.transform.childCount; i++)
			{
				transform = FindTransformInChildren(parent.transform.GetChild(i), name);
				if ((bool)transform)
				{
					return transform;
				}
			}
		}
		return transform;
	}

	public static Transform FindTransformInChildren(Transform parent, Predicate<Transform> condition)
	{
		foreach (Transform item in parent)
		{
			if (condition(item))
			{
				return item;
			}
		}
		for (int i = 0; i < parent.transform.childCount; i++)
		{
			Transform transform2 = FindTransformInChildren(parent.transform.GetChild(i), condition);
			if ((bool)transform2)
			{
				return transform2;
			}
		}
		return null;
	}

	public static T FindInChildren<T>(GameObject root, bool includeInactive, Predicate<T> condition) where T : Component
	{
		if (condition != null && root != null)
		{
			T[] componentsInChildren = root.GetComponentsInChildren<T>(includeInactive);
			foreach (T val in componentsInChildren)
			{
				if (condition(val))
				{
					return val;
				}
			}
		}
		return (T)null;
	}

	public static T FindInChildren<T>(GameObject root, bool includeInactive, string name) where T : Component
	{
		return FindInChildren(root, includeInactive, (T component) => name.Equals(component.name));
	}

	public static Component FindComponent<T>(GameObject objectToTest)
	{
		Component[] components = objectToTest.GetComponents<Component>();
		Component[] array = components;
		foreach (Component component in array)
		{
			if (component is T)
			{
				return component;
			}
		}
		return null;
	}

	public static T[] FindComponents<T>(GameObject objectToTest) where T : class
	{
		Component[] components = objectToTest.GetComponents<Component>();
		List<T> list = new List<T>();
		Component[] array = components;
		foreach (Component component in array)
		{
			if (component is T)
			{
				list.Add((T)(object)component);
			}
		}
		return list.ToArray();
	}

	public static List<Component> FindComponentsOfType<T>(GameObject objectToTest)
	{
		Component[] components = objectToTest.GetComponents<Component>();
		List<Component> list = new List<Component>();
		foreach (Component component in components)
		{
			if (component is T)
			{
				list.Add(component);
			}
		}
		return list;
	}

	public static List<GameObject> FindChildrenWithComponent<T>(GameObject objectToTest)
	{
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < objectToTest.transform.childCount; i++)
		{
			Component component = FindComponent<T>(objectToTest.transform.GetChild(i).gameObject);
			if (component != null)
			{
				list.Add(objectToTest.transform.GetChild(i).gameObject);
			}
		}
		return list;
	}

	public static List<GameObject> FindChildrenWithComponentRecursive<T>(GameObject objectToTest)
	{
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < objectToTest.transform.childCount; i++)
		{
			Component component = FindComponent<T>(objectToTest.transform.GetChild(i).gameObject);
			if (component != null)
			{
				list.Add(objectToTest.transform.GetChild(i).gameObject);
			}
			list.AddRange(FindChildrenWithComponentRecursive<T>(objectToTest.transform.GetChild(i).gameObject));
		}
		return list;
	}

	public static GameObject[] GetAllChildren(GameObject parent)
	{
		Transform[] componentsInChildren = parent.GetComponentsInChildren<Transform>();
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].gameObject != parent)
			{
				list.Add(componentsInChildren[i].gameObject);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list.ToArray();
	}

	public static Camera FindFirstCamera(int layer)
	{
		Camera[] allCameras = Camera.allCameras;
		foreach (Camera camera in allCameras)
		{
			if (camera.gameObject.activeInHierarchy)
			{
				uint cullingMask = (uint)camera.cullingMask;
				if ((cullingMask & (1 << layer)) > 0)
				{
					return camera;
				}
			}
		}
		return null;
	}

	public static Vector2 GetObjectScreenPosition(GameObject owner)
	{
		Camera camera = FindFirstCamera(owner.layer);
		if (camera != null)
		{
			return camera.WorldToScreenPoint(owner.transform.position);
		}
		return Vector2.zero;
	}

	public static T ForceComponentExists_AndDestroyOthers<T, U>(GameObject owner) where T : Component where U : Component
	{
		T val = owner.GetComponent<T>();
		if ((UnityEngine.Object)val == (UnityEngine.Object)null)
		{
			Component component = owner.GetComponent(typeof(U));
			if (component != null)
			{
				UnityEngine.Object.DestroyImmediate(component);
			}
			val = owner.AddComponent<T>();
		}
		return val;
	}

	public static void SetLayerRecursively(GameObject go, int layer)
	{
		SetLayerRecursively(go, layer, -1);
	}

	public static void SetLayerRecursively(GameObject go, int layer, int existingLayer)
	{
		if (go == null)
		{
			return;
		}
		if (existingLayer == -1 || go.layer == existingLayer)
		{
			go.layer = layer;
		}
		foreach (Transform item in go.transform)
		{
			SetLayerRecursively(item.gameObject, layer, existingLayer);
		}
	}
}
