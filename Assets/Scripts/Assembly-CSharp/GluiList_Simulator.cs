using UnityEngine;

[AddComponentMenu("Glui List/List Simulator")]
public class GluiList_Simulator : GluiList_Base
{
	public enum SeparatorToUse
	{
		None = 0,
		OneAtStart = 1
	}

	public int separatorsAtStart;

	public int separatorsAtEnd;

	public int numberOfItemsToSimulate;

	public override void OnSafeEnable()
	{
		UpdateFromData();
	}

	public override void OnDisable()
	{
		ClearListObjects();
	}

	protected override void UpdateFromData()
	{
		UpdateFromData(null);
	}

	protected override void CreateListObjects(object[] records)
	{
		base.GluiList.Clear();
		if (!(listObject == null))
		{
			for (int i = 0; i < separatorsAtStart; i++)
			{
				base.GluiList.AddObject(new GameObject("Separator"), GluiItemCatalog.Item.Type.Separator, null);
			}
			for (int j = 0; j < numberOfItemsToSimulate; j++)
			{
				GameObject obj = NewListObject();
				base.GluiList.AddObject(obj);
			}
			for (int k = 0; k < separatorsAtEnd; k++)
			{
				base.GluiList.AddObject(new GameObject("Separator"), GluiItemCatalog.Item.Type.Separator, null);
			}
		}
	}
}
