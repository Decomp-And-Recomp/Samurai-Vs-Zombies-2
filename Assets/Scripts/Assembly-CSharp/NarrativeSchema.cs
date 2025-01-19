using System;
using UnityEngine;

[DataBundleClass(Category = "Design")]
public class NarrativeSchema
{
	[DataBundleKey(ColumnWidth = 200)]
	public string id;

	[DataBundleField(ColumnWidth = 200, StaticResource = true)]
	public GameObject prefab;

	[DataBundleSchemaFilter(typeof(WaveSchema), false)]
	public DataBundleRecordKey showAfterSpecificWave;

	public string showAfterIAP;

	private static NarrativeSchema[] records;

	public static string UdamanTableName
	{
		get
		{
			return "Narratives";
		}
	}

	public static NarrativeSchema NarrativeForWave(DataBundleRecordKey waveKey)
	{
		if (records == null)
		{
			records = DataBundleRuntime.Instance.InitializeRecords<NarrativeSchema>(UdamanTableName);
		}
		return Array.Find(records, (NarrativeSchema r) => r != null && r.showAfterSpecificWave == waveKey);
	}
}
