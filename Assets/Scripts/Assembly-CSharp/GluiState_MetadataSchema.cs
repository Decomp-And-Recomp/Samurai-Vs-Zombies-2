[DataBundleClass(Category = "GLUI")]
public class GluiState_MetadataSchema
{
	public enum InheritTransform
	{
		None = 0,
		Local = 1,
		Global = 2,
		Zero = 3
	}

	[DataBundleKey(ColumnWidth = 300)]
	public string Action;

	[DataBundleField(ColumnWidth = 300)]
	public string DisplayName;

	[DataBundleSchemaFilter(typeof(DynamicEnum), false)]
	[DataBundleRecordTableFilter("GluiStatePriority")]
	[DataBundleField(ColumnWidth = 120)]
	public DataBundleRecordKey priority;

	[DataBundleField(ColumnWidth = 100)]
	public InputLayerType inputLayerType;

	[DataBundleRecordTableFilter("GluiStateExclusiveLayer")]
	[DataBundleField(ColumnWidth = 120)]
	[DataBundleSchemaFilter(typeof(DynamicEnum), false)]
	public DataBundleRecordKey exclusiveLayer;

	[DataBundleField(ColumnWidth = 200, TooltipInfo = "Optionally inherit the transform values from the menu state object we're spawned from.")]
	public InheritTransform inheritStateTransform;
}
