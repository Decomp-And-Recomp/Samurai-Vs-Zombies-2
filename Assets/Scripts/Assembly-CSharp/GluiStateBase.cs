using System;
using UnityEngine;

public abstract class GluiStateBase : MonoBehaviour
{
	public string actionToHandle;

	public string actionToHandleReverse;

	public GluiReaction onStart;

	public GluiReaction onReady;

	public GluiReaction onExit;

	public GluiState_MetadataSchema.InheritTransform defaultTransformInherit;

	[DataBundleSchemaFilter(typeof(GluiState_MetadataSchema), false)]
	[HideInInspector]
	public DataBundleRecordKey optionalMetadata;

	public GluiStateProcesses processes = new GluiStateProcesses();

	public abstract void InitState(Action<GameObject> whenDone);

	public abstract void DestroyState();

	public void GetMetadata(string metadataRecordKey, out GluiState_MetadataSchema metadata)
	{
		if (metadataRecordKey != string.Empty && DataBundleRuntime.Instance != null && DataBundleRuntime.Instance.Initialized)
		{
			metadata = DataBundleRuntime.Instance.InitializeRecord<GluiState_MetadataSchema>(metadataRecordKey);
		}
		else
		{
			metadata = null;
		}
	}

	public virtual void ApplyTransform(GameObject newObject, GameObject parent)
	{
		GluiState_MetadataSchema.InheritTransform inheritStateTransform = defaultTransformInherit;
		GluiState_MetadataSchema metadata;
		GetMetadata(optionalMetadata, out metadata);
		if (metadata != null && metadata.inheritStateTransform != 0)
		{
			inheritStateTransform = metadata.inheritStateTransform;
		}
		switch (inheritStateTransform)
		{
		case GluiState_MetadataSchema.InheritTransform.None:
			newObject.transform.parent = parent.transform;
			break;
		case GluiState_MetadataSchema.InheritTransform.Global:
		{
			Vector3 localPosition2 = newObject.transform.localPosition;
			newObject.transform.parent = parent.transform;
			newObject.transform.localPosition = localPosition2;
			break;
		}
		case GluiState_MetadataSchema.InheritTransform.Local:
		{
			Vector3 localPosition = newObject.transform.localPosition;
			newObject.transform.parent = parent.transform;
			newObject.transform.localPosition = localPosition + parent.transform.localPosition;
			break;
		}
		case GluiState_MetadataSchema.InheritTransform.Zero:
			newObject.transform.parent = parent.transform;
			newObject.transform.localPosition = Vector3.zero;
			break;
		}
	}

	public int GetPriority(string defaultMetadataRecordKey)
	{
		int result = 0;
		GluiState_MetadataSchema metadata;
		GetMetadata(optionalMetadata, out metadata);
		if (metadata == null)
		{
			GetMetadata(defaultMetadataRecordKey, out metadata);
		}
		if (metadata != null)
		{
			result = DynamicEnum.ToIndex(metadata.priority);
		}
		return result;
	}

	public void GetInputExclusivity(string defaultMetadataRecordKey, out int exclusiveLayer, out InputLayerType inputLayerType)
	{
		exclusiveLayer = 0;
		inputLayerType = InputLayerType.Normal;
		GluiState_MetadataSchema metadata;
		GetMetadata(optionalMetadata, out metadata);
		if (metadata == null)
		{
			GetMetadata(defaultMetadataRecordKey, out metadata);
		}
		if (metadata != null)
		{
			exclusiveLayer = DynamicEnum.ToIndex(metadata.exclusiveLayer);
			inputLayerType = metadata.inputLayerType;
		}
	}

	public bool HandlesAction(string action)
	{
		return string.Equals(action, actionToHandle) || string.Equals(action, actionToHandleReverse);
	}
}
