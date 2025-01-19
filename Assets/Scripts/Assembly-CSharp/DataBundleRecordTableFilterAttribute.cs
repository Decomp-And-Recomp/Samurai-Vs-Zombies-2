using System;

[AttributeUsage(AttributeTargets.Field)]
public class DataBundleRecordTableFilterAttribute : Attribute
{
	public string Table { get; set; }

	public DataBundleRecordTableFilterAttribute(string tableFilter)
	{
		Table = tableFilter;
	}
}
