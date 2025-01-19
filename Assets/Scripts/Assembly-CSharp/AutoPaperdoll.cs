using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Framework/Auto Paperdoll")]
[ExecuteInEditMode]
public class AutoPaperdoll : MonoBehaviour
{
	[Serializable]
	public class Offset
	{
		public Vector3 position;

		public Vector3 rotation;

		public Vector3 scale;
	}

	[Serializable]
	public class LabeledJoint
	{
		public string label = string.Empty;

		public Transform joint;

		public GameObject autoAttachPrefab;

		public Offset transformOffset = new Offset();
	}

	public LabeledJoint[] joints = new LabeledJoint[0];

	private Dictionary<string, LabeledJoint> mJointsDict;

	public void AttachObjectToJoint(GameObject theObject, string theJointLabel)
	{
		AttachObjectToJoint(theObject, theJointLabel, true);
	}

	public void AttachObjectToJoint(GameObject theObject, string theJointLabel, bool ignoreCurrentTransform)
	{
		LabeledJoint labeledJoint = GetJointData(theJointLabel);
		if (labeledJoint == null)
		{
			labeledJoint = new LabeledJoint();
			labeledJoint.joint = base.transform;
		}
		AttachObjectToJoint(theObject, labeledJoint, ignoreCurrentTransform);
	}

	public void AttachObjectToJoint(GameObject theObject, LabeledJoint theJointData, bool ignoreCurrentTransform)
	{
		if (theJointData != null && !(theObject == null))
		{
			theObject.transform.parent = theJointData.joint;
			if (ignoreCurrentTransform)
			{
				theObject.transform.localRotation = Quaternion.Euler(theJointData.transformOffset.rotation);
				theObject.transform.localPosition = theJointData.transformOffset.position;
				theObject.transform.localScale = Vector3.one + theJointData.transformOffset.scale;
			}
			else
			{
				theObject.transform.localRotation *= Quaternion.Euler(theJointData.transformOffset.rotation);
				theObject.transform.localPosition += theJointData.transformOffset.position;
				theObject.transform.localScale += theJointData.transformOffset.scale;
			}
			BroadcastMessage("OnAutoPaperdollAdded", theObject, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void AlignObjectWithJoint(GameObject theObject, string theJointLabel)
	{
		LabeledJoint labeledJoint = GetJointData(theJointLabel);
		if (labeledJoint == null)
		{
			labeledJoint = new LabeledJoint();
			labeledJoint.joint = base.transform;
		}
		AlignObjectWithJoint(theObject, labeledJoint);
	}

	public void AlignObjectWithJoint(GameObject theObject, LabeledJoint theJointData)
	{
		AlignObjectWithJointNoRotation(theObject, theJointData);
		theObject.transform.rotation = theJointData.joint.rotation;
	}

	public void AlignObjectWithJointNoRotation(GameObject theObject, LabeledJoint theJointData)
	{
		theObject.transform.position = theJointData.joint.position;
		Vector3 localScale = new Vector3((!Mathf.Approximately(theJointData.joint.lossyScale.x, 1f)) ? theJointData.joint.lossyScale.x : 1f, (!Mathf.Approximately(theJointData.joint.lossyScale.y, 1f)) ? theJointData.joint.lossyScale.y : 1f, (!Mathf.Approximately(theJointData.joint.lossyScale.z, 1f)) ? theJointData.joint.lossyScale.z : 1f);
		theObject.transform.localScale = localScale;
		ApplyJointOffset(theObject, theJointData.transformOffset);
	}

	public bool HasJoint(string theJointLabel)
	{
		return mJointsDict != null && mJointsDict.ContainsKey(theJointLabel);
	}

	public LabeledJoint GetJointData(string theJointLabel)
	{
		if (mJointsDict == null)
		{
			if (joints == null || joints.Length == 0)
			{
				return null;
			}
			Start();
		}
		LabeledJoint value;
		if (mJointsDict != null && mJointsDict.TryGetValue(theJointLabel, out value))
		{
			return value;
		}
		return null;
	}

	public GameObject InstantiateObjectOnJoint(GameObject thePrefab, string theJointLabel)
	{
		return InstantiateObjectOnJoint(thePrefab, null, theJointLabel, true, true);
	}

	public GameObject InstantiateObjectOnJoint(GameObject thePrefab, Material material, string theJointLabel)
	{
		return InstantiateObjectOnJoint(thePrefab, material, theJointLabel, true, true);
	}

	public GameObject InstantiateObjectOnJoint(GameObject thePrefab, string theJointLabel, bool keepAttachedToJoint)
	{
		return InstantiateObjectOnJoint(thePrefab, null, theJointLabel, keepAttachedToJoint, keepAttachedToJoint);
	}

	public GameObject InstantiateObjectOnJoint(GameObject thePrefab, string theJointLabel, bool keepAttachedToJoint, bool applyJointScale)
	{
		return InstantiateObjectOnJoint(thePrefab, null, theJointLabel, keepAttachedToJoint, applyJointScale);
	}

	public GameObject InstantiateObjectOnJoint(GameObject thePrefab, Material material, string theJointLabel, bool keepAttachedToJoint, bool applyJointScale)
	{
		if (thePrefab == null)
		{
			return null;
		}
		GameObject gameObject = GameObjectPool.DefaultObjectPool.Acquire(thePrefab);
		ApplyObjectToJoint(gameObject, material, theJointLabel, keepAttachedToJoint, applyJointScale, true);
		return gameObject;
	}

	public void ApplyObjectToJoint(GameObject obj, Material material, string theJointLabel, bool keepAttachedToJoint, bool applyJointScale, bool applyJointRotation)
	{
		if (obj == null)
		{
			return;
		}
		LabeledJoint labeledJoint = GetJointData(theJointLabel);
		if (labeledJoint == null)
		{
			labeledJoint = new LabeledJoint();
			labeledJoint.joint = base.transform;
		}
		if (material != null)
		{
			Renderer[] componentsInChildren = obj.GetComponentsInChildren<Renderer>(true);
			foreach (Renderer renderer in componentsInChildren)
			{
				renderer.sharedMaterial = material;
			}
		}
		Vector3 localScale = obj.transform.localScale;
		if (keepAttachedToJoint)
		{
			AttachObjectToJoint(obj, labeledJoint, true);
		}
		else if (applyJointRotation)
		{
			AlignObjectWithJoint(obj, labeledJoint);
		}
		else
		{
			AlignObjectWithJointNoRotation(obj, labeledJoint);
		}
		if (!applyJointScale)
		{
			obj.transform.localScale = localScale;
		}
	}

	public Vector3 GetJointPosition(string theJointLabel)
	{
		return GetRelativeJointPosition(theJointLabel) + base.transform.position;
	}

	public Vector3 GetJointPosition(string theJointLabel, Vector3 defaultOffsetIfJointNotFound)
	{
		Vector3 vector = GetRelativeJointPosition(theJointLabel);
		if (vector == Vector3.zero)
		{
			vector = defaultOffsetIfJointNotFound;
		}
		return vector + base.transform.position;
	}

	public Vector3 GetJointPosition(LabeledJoint theJointData)
	{
		return GetRelativeJointPosition(theJointData) + base.transform.position;
	}

	public Vector3 GetRelativeJointPosition(string theJointLabel)
	{
		LabeledJoint jointData = GetJointData(theJointLabel);
		if (jointData == null)
		{
			return Vector3.zero;
		}
		return GetRelativeJointPosition(jointData);
	}

	public Vector3 GetRelativeJointPosition(LabeledJoint theJointData)
	{
		Vector3 position = theJointData.transformOffset.position;
		if (theJointData.transformOffset.scale.x != 0f && theJointData.transformOffset.scale.y != 0f && theJointData.transformOffset.scale.z != 0f)
		{
			position.Scale(theJointData.transformOffset.scale);
		}
		position = theJointData.joint.rotation * position;
		return theJointData.joint.position + position - base.transform.position;
	}

	public Quaternion GetJointRotation(string theJointLabel)
	{
		LabeledJoint jointData = GetJointData(theJointLabel);
		if (jointData == null)
		{
			return base.transform.rotation;
		}
		return GetJointRotation(jointData);
	}

	public Quaternion GetJointRotation(LabeledJoint theJointData)
	{
		Vector3 eulerAngles = theJointData.joint.eulerAngles;
		eulerAngles += theJointData.transformOffset.rotation;
		return Quaternion.Euler(eulerAngles);
	}

	public Quaternion GetJointLocalRotation(string theJointLabel)
	{
		LabeledJoint jointData = GetJointData(theJointLabel);
		if (jointData == null)
		{
			return Quaternion.identity;
		}
		return GetJointLocalRotation(jointData);
	}

	public Quaternion GetJointLocalRotation(LabeledJoint theJointData)
	{
		Vector3 localEulerAngles = theJointData.joint.localEulerAngles;
		localEulerAngles += theJointData.transformOffset.rotation;
		return Quaternion.Euler(localEulerAngles);
	}

	public Vector3 GetJointScale(string theJointLabel)
	{
		LabeledJoint jointData = GetJointData(theJointLabel);
		if (jointData == null)
		{
			return base.transform.lossyScale;
		}
		return GetJointScale(jointData);
	}

	public Vector3 GetJointScale(LabeledJoint theJointData)
	{
		Vector3 lossyScale = theJointData.joint.lossyScale;
		Vector3 localScale = theJointData.joint.localScale;
		Vector3 vector = localScale + theJointData.transformOffset.scale;
		lossyScale.x = lossyScale.x / localScale.x * vector.x;
		lossyScale.y = lossyScale.x / localScale.y * vector.y;
		lossyScale.z = lossyScale.x / localScale.z * vector.z;
		return lossyScale;
	}

	public Vector3 GetJointLocalScale(string theJointLabel)
	{
		LabeledJoint jointData = GetJointData(theJointLabel);
		if (jointData == null)
		{
			return new Vector3(1f, 1f, 1f);
		}
		return GetJointLocalScale(jointData);
	}

	public Vector3 GetJointLocalScale(LabeledJoint theJointData)
	{
		return theJointData.joint.localScale + theJointData.transformOffset.scale;
	}

	private void Start()
	{
		if (joints == null || joints.Length == 0 || mJointsDict != null)
		{
			return;
		}
		mJointsDict = new Dictionary<string, LabeledJoint>();
		LabeledJoint[] array = joints;
		foreach (LabeledJoint labeledJoint in array)
		{
			if (labeledJoint == null)
			{
				continue;
			}
			if (labeledJoint.joint == null)
			{
				labeledJoint.joint = base.transform;
			}
			if (!string.IsNullOrEmpty(labeledJoint.label))
			{
				mJointsDict.Add(labeledJoint.label, labeledJoint);
			}
			if (!(labeledJoint.autoAttachPrefab != null))
			{
				continue;
			}
			GameObject autoAttachPrefab = labeledJoint.autoAttachPrefab;
			if (!(autoAttachPrefab == null))
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(autoAttachPrefab) as GameObject;
				if ((bool)gameObject)
				{
					AttachObjectToJoint(gameObject, labeledJoint, true);
					labeledJoint.autoAttachPrefab = gameObject;
					ObjectUtils.SetLayerRecursively(gameObject, base.gameObject.layer);
				}
			}
		}
	}

	private void ApplyJointOffset(GameObject theObject, Offset theOffset)
	{
		Transform transform = theObject.transform;
		transform.localScale += theOffset.scale;
		transform.localEulerAngles += theOffset.rotation;
		Vector3 position = theOffset.position;
		position.Scale(transform.lossyScale);
		position = transform.rotation * position;
		transform.localPosition += position;
	}
}
