[DataBundleClass(Category = "Design")]
public class ProceduralWaveListSchema
{
	[DataBundleKey]
	public int key;

	[DataBundleSchemaFilter(typeof(ProceduralWaveSchema), false)]
	public DataBundleRecordKey proceduralWave;
}
