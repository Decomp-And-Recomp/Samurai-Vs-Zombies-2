using System;
using UnityEngine;

public static class AutoPaperdollAdapter
{
	public static void Deserialize(AutoPaperdoll paperdoll, GameObject model, JointSchema[] jointData)
	{
		if (paperdoll == null || model == null || jointData == null)
		{
			return;
		}
		Transform transform = model.transform;
		paperdoll.joints = new AutoPaperdoll.LabeledJoint[jointData.Length];
		for (int i = 0; i < jointData.Length; i++)
		{
			AutoPaperdoll.LabeledJoint labeledJoint = new AutoPaperdoll.LabeledJoint();
			JointSchema joint = jointData[i];
			if (joint == null)
			{
				continue;
			}
			paperdoll.joints[i] = labeledJoint;
			if (!DataBundleRecordKey.IsNullOrEmpty(joint.label))
			{
				labeledJoint.label = joint.label.Key;
			}
			if (!string.IsNullOrEmpty(joint.jointName))
			{
				labeledJoint.joint = ObjectUtils.FindTransformInChildren(transform, (Transform t) => t.name.IndexOf(joint.jointName, StringComparison.Ordinal) != -1);
			}
			labeledJoint.autoAttachPrefab = joint.prefab;
			labeledJoint.transformOffset.position = new Vector3(joint.offsetPositionX, joint.offsetPositionY, joint.offsetPositionZ);
			labeledJoint.transformOffset.rotation = new Vector3(joint.offsetRotationX, joint.offsetRotationY, joint.offsetRotationZ);
			labeledJoint.transformOffset.scale = new Vector3(joint.offsetScaleX, joint.offsetScaleY, joint.offsetScaleZ);
		}
	}
}
