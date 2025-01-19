[DataBundleClass(Localizable = true)]
public class TaggedString
{
	[DataBundleKey(ColumnWidth = 200)]
	public string tag;

	[DataBundleField(ColumnWidth = 1500)]
	public string stringValue;
}
