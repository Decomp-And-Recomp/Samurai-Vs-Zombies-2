using System;

namespace UnityEngine
{
	public static class GameObject_Extensions
	{
		public static GameObject FindChild(this GameObject parent, string id)
		{
			Transform transform = ObjectUtils.FindTransformInChildren(parent.transform, id);
			if (transform != null)
			{
				return transform.gameObject;
			}
			return null;
		}

		public static GameObject FindChild(this GameObject parent, Predicate<Transform> condition)
		{
			Transform transform = ObjectUtils.FindTransformInChildren(parent.transform, condition);
			if (transform != null)
			{
				return transform.gameObject;
			}
			return null;
		}

		public static T FindChildComponent<T>(this GameObject parent, string id) where T : Component
		{
			GameObject gameObject = parent.FindChild(id);
			if (gameObject != null)
			{
				return gameObject.GetComponent<T>();
			}
			return (T)null;
		}

		public static T FindChildComponent<T>(this GameObject parent, Predicate<Transform> condition) where T : Component
		{
			GameObject gameObject = parent.FindChild(condition);
			if (gameObject != null)
			{
				return gameObject.GetComponent<T>();
			}
			return (T)null;
		}
	}
}
