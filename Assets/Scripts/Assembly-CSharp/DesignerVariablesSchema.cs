[DataBundleClass]
public class DesignerVariablesSchema
{
	[DataBundleKey(ColumnWidth = 200)]
	public string VariableName;

	[DataBundleField(ColumnWidth = 200)]
	public string description;

	[DataBundleField(ColumnWidth = 200)]
	public DesignerVariables.Operation operation;

	[DataBundleField(ColumnWidth = 200)]
	public string A;

	[DataBundleField(ColumnWidth = 200)]
	public string B;

	[DataBundleField(ColumnWidth = 200)]
	public string C;
}
