using System.Collections.Generic;
using UnityEngine;

public abstract class GluiList_Base : GluiDataScanner
{
	public enum ORIENTATION
	{
		HORIZONTAL = 0,
		VERTICAL = 1
	}

	public GameObject listObject;

	private List<GameObject> listObjects = new List<GameObject>();

	private GluiScrollList gluiList;

	protected ORIENTATION Orientation
	{
		get
		{
			return ORIENTATION.HORIZONTAL;
		}
	}

	protected GluiScrollList GluiList
	{
		get
		{
			return gluiList;
		}
	}

	protected Vector2 Size
	{
		get
		{
			return gluiList.Size;
		}
	}

	public void Awake()
	{
		gluiList = (GluiScrollList)GetComponent(typeof(GluiScrollList));
		if (!(gluiList == null))
		{
		}
	}

	public virtual void OnDisable()
	{
		ClearListObjects();
	}

	public void Redraw()
	{
		UpdateFromData();
	}

	protected override void UpdateFromData()
	{
		UpdateFromData(base.Data);
	}

	public void UpdateFromData(object[] records)
	{
		ClearListObjects();
		CreateListObjects(records);
		ArrangeListObjects();
	}

	protected GameObject NewListObject()
	{
		GameObject gameObject = (GameObject)Object.Instantiate(listObject);
		listObjects.Add(gameObject);
		return gameObject;
	}

	protected virtual void CreateListObjects(object[] dataRecords)
	{
		GluiList.Clear();
		if (dataRecords == null)
		{
			return;
		}
		PreCreateListObjects(dataRecords);
		foreach (object data in dataRecords)
		{
			GameObject gameObject = NewListObject();
			UpdateListObject(gameObject, data);
			GluiItemCatalog.Item item = GluiList.AddObject(gameObject);
			if (item != null)
			{
				item.data = data;
				GluiDataScanner[] componentsInChildren = gameObject.GetComponentsInChildren<GluiDataScanner>();
				GluiDataScanner[] array = componentsInChildren;
				foreach (GluiDataScanner gluiDataScanner in array)
				{
					gluiDataScanner.OnSafeEnable();
				}
				continue;
			}
			Object.Destroy(gameObject);
			break;
		}
	}

	protected virtual void PreCreateListObjects(object[] data)
	{
	}

	protected void ClearListObjects()
	{
		listObjects.Clear();
	}

	private bool UpdateListObject(GameObject listObject, object data)
	{
		IGluiElement_DataAdaptor gluiElement_DataAdaptor = (IGluiElement_DataAdaptor)listObject.GetComponent(typeof(IGluiElement_DataAdaptor));
		if (gluiElement_DataAdaptor == null)
		{
			return false;
		}
		gluiElement_DataAdaptor.SetGluiCustomElementData(data);
		return true;
	}

	protected virtual void ArrangeListObjects()
	{
		gluiList.Reframe();
		if (listObjects.Count > 0)
		{
			int num = FindPersistantScrollListItem();
			if (num == -1)
			{
				gluiList.Select(0);
				gluiList.SnapTo(0, GluiScrollList.SelectionSnap.Instant_Center);
			}
			else
			{
				gluiList.Select(num);
				gluiList.SnapTo(num, GluiScrollList.SelectionSnap.Instant_Center);
			}
		}
		else
		{
			gluiList.Select(-1);
		}
	}

	private int FindPersistantScrollListItem()
	{
		int result = -1;
		if (gluiList.persistSelectionAs != string.Empty)
		{
			object data = SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.GetData(gluiList.persistSelectionAs);
			int num = 0;
			foreach (GluiItemCatalog.Item item in gluiList.Items)
			{
				if (item.data == data)
				{
					result = num;
				}
				num++;
			}
		}
		return result;
	}
}
