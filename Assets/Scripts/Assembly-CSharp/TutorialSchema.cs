[DataBundleClass(Category = "Design")]
public class TutorialSchema
{
	[DataBundleField(ColumnWidth = 200)]
	[DataBundleKey]
	public string name;

	[DataBundleField(ColumnWidth = 200)]
	public string actionToTrigger;

	[DataBundleSchemaFilter(typeof(TutorialSchema), false)]
	[DataBundleField(ColumnWidth = 200)]
	public DataBundleRecordKey tutorialToPlayNext;

	[DataBundleField(ColumnWidth = 200)]
	public string actionSendOnStart;

	[DataBundleField(ColumnWidth = 200)]
	public string actionSendOnDone;
}
