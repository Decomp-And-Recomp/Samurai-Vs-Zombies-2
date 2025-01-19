using System;
using UnityEngine;

[AddComponentMenu("Glui State/State Asset Bundle Reference")]
public class GluiState_AssetBundle : GluiStateBase
{
	[HideInInspector]
	[DataBundleSchemaFilter(typeof(GluiState_AssetBundleSchema), false)]
	public DataBundleRecordKey prefab;

	private GameObject mLoadedPrefab;

	private DataBundleRecordHandle<GluiState_AssetBundleSchema> mRecordHandle;

	public override void InitState(Action<GameObject> whenDone)
	{
		if (mRecordHandle == null)
		{
			bool loadFromBundleAsync = BundleLoader.LoadFromBundleAsync;
			BundleLoader.LoadFromBundleAsync = false;
			mRecordHandle = new DataBundleRecordHandle<GluiState_AssetBundleSchema>(prefab);
			mRecordHandle.Load(DataBundleResourceGroup.All, false, delegate(GluiState_AssetBundleSchema pls)
			{
				mLoadedPrefab = (GameObject)UnityEngine.Object.Instantiate(pls.prefab);
				ApplyTransform(mLoadedPrefab, base.gameObject);
				processes.AddStateProcesses(mLoadedPrefab);
				whenDone(mLoadedPrefab);
			});
			BundleLoader.LoadFromBundleAsync = loadFromBundleAsync;
		}
	}

	public override void DestroyState()
	{
		if (mRecordHandle == null)
		{
			return;
		}
		if (mLoadedPrefab != null)
		{
			UnityEngine.Object.DestroyImmediate(mLoadedPrefab);
			mLoadedPrefab = null;
			processes.Clear();
			if (!mRecordHandle.Data.skipUnloadUnusedAssets)
			{
				Resources.UnloadUnusedAssets();
			}
		}
		mRecordHandle.Dispose();
		mRecordHandle = null;
	}
}
