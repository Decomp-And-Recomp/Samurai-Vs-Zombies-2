using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool
{
	private static GameObjectPool defaultObjectPool;

	public int maxReleasedCount = -1;

	private Dictionary<string, Stack<TypedWeakReference<GameObject>>> releasedObjects = new Dictionary<string, Stack<TypedWeakReference<GameObject>>>();

	public static GameObjectPool DefaultObjectPool
	{
		get
		{
			if (defaultObjectPool == null)
			{
				defaultObjectPool = new GameObjectPool();
			}
			return defaultObjectPool;
		}
	}

	public GameObject Acquire(string objName)
	{
		return Acquire(objName, null);
	}

	public GameObject Acquire(string objName, Type[] components)
	{
		GameObject gameObject = null;
		if (!string.IsNullOrEmpty(objName))
		{
			if (releasedObjects.ContainsKey(objName))
			{
				Stack<TypedWeakReference<GameObject>> stack = releasedObjects[objName];
				while (gameObject == null && stack.Count > 0)
				{
					gameObject = stack.Pop().ptr;
				}
			}
			if (gameObject == null)
			{
				gameObject = new GameObject(objName);
				if (components != null)
				{
					foreach (Type componentType in components)
					{
						gameObject.AddComponent(componentType);
					}
				}
			}
			else
			{
				gameObject.SetActive(true);
				gameObject.BroadcastMessage("Awake", null, SendMessageOptions.DontRequireReceiver);
				gameObject.BroadcastMessage("Start", null, SendMessageOptions.DontRequireReceiver);
			}
		}
		return gameObject;
	}

	public GameObject Acquire(GameObject objPrefab)
	{
		return Acquire(objPrefab, null, null, null);
	}

	public GameObject Acquire(GameObject objPrefab, Vector3? pos, Quaternion? rot)
	{
		return Acquire(objPrefab, pos, rot, null);
	}

	public GameObject Acquire(GameObject objPrefab, Type[] components)
	{
		return Acquire(objPrefab, null, null, components);
	}

	public GameObject Acquire(GameObject objPrefab, Vector3? pos, Quaternion? rot, Type[] components)
	{
		GameObject gameObject = null;
		if (objPrefab != null)
		{
			string name = objPrefab.name;
			if (releasedObjects.ContainsKey(name))
			{
				Stack<TypedWeakReference<GameObject>> stack = releasedObjects[name];
				while (gameObject == null && stack.Count > 0)
				{
					gameObject = stack.Pop().ptr;
				}
			}
			if (gameObject == null)
			{
				gameObject = UnityEngine.Object.Instantiate(objPrefab) as GameObject;
				gameObject.name = objPrefab.name;
				BundleUtils.ValidateMaterials(gameObject);
				if (components != null)
				{
					foreach (Type componentType in components)
					{
						gameObject.AddComponent(componentType);
					}
				}
				gameObject.SetActive(true);
			}
			else
			{
				gameObject.SetActive(true);
				gameObject.BroadcastMessage("Awake", null, SendMessageOptions.DontRequireReceiver);
				gameObject.BroadcastMessage("Start", null, SendMessageOptions.DontRequireReceiver);
			}
			if (pos.HasValue)
			{
				gameObject.transform.position = pos.Value;
			}
			if (rot.HasValue)
			{
				gameObject.transform.rotation = rot.Value;
			}
		}
		return gameObject;
	}

	public void Release(GameObject obj)
	{
		if (!(obj == null))
		{
			Stack<TypedWeakReference<GameObject>> stack = null;
			string name = obj.name;
			if (!releasedObjects.ContainsKey(name))
			{
				stack = new Stack<TypedWeakReference<GameObject>>();
				releasedObjects.Add(name, stack);
			}
			else
			{
				stack = releasedObjects[name];
			}
			obj.BroadcastMessage("Sleep", null, SendMessageOptions.DontRequireReceiver);
			obj.transform.parent = null;
			if (maxReleasedCount == -1 || stack.Count < maxReleasedCount)
			{
				obj.SetActive(false);
				stack.Push(new TypedWeakReference<GameObject>(obj));
			}
			else
			{
				UnityEngine.Object.Destroy(obj);
			}
		}
	}

	public void Release(GameObject obj, float t)
	{
		SingletonSpawningMonoBehaviour<ApplicationUtilities>.Instance.StartCoroutine(ReleaseDelayed(obj, t));
	}

	private IEnumerator ReleaseDelayed(GameObject obj, float t)
	{
		yield return new WaitForSeconds(t);
		Release(obj);
	}
}
