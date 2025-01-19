using UnityEngine;

public abstract class GluiDataScanner : SafeEnable_Monobehaviour
{
	public GameObject dataObject;

	public string dataObjectAlternateName;

	public string dataFilterKey;

	public string dataFilterKeySecondary;

	public GluiDataScan_AdditionalParameters additionalParameters;

	private bool hasEnabled;

	private GameObject DataObject
	{
		get
		{
			if (dataObject == null && dataObjectAlternateName != string.Empty)
			{
				return GameObject.Find(dataObjectAlternateName);
			}
			return dataObject;
		}
	}

	private IGluiDataSource DataSource
	{
		get
		{
			GameObject gameObject = DataObject;
			if (gameObject == null)
			{
				return null;
			}
			IGluiDataSource gluiDataSource = (IGluiDataSource)gameObject.GetComponent(typeof(IGluiDataSource));
			if (gluiDataSource == null)
			{
			}
			return gluiDataSource;
		}
	}

	public virtual string DataFilterKey
	{
		get
		{
			return dataFilterKey;
		}
	}

	public virtual string DataFilterKeySecondary
	{
		get
		{
			return dataFilterKeySecondary;
		}
	}

	protected object[] Data
	{
		get
		{
			IGluiDataSource dataSource = DataSource;
			object[] records = null;
			if (dataSource != null)
			{
				dataSource.Get_GluiData(DataFilterKey, DataFilterKeySecondary, additionalParameters, out records);
			}
			return records;
		}
	}

	protected abstract void UpdateFromData();

	public override void OnSafeEnable()
	{
		if (!hasEnabled)
		{
			hasEnabled = true;
			UpdateFromData();
		}
	}
}
