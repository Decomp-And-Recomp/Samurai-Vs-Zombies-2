using System;

[AttributeUsage(AttributeTargets.Field)]
public class DataBundleKeyAttribute : Attribute
{
	public static readonly int defaultWidth = 100;

	public static readonly string fieldLead = DataBundleDropdownFilter.fieldLead;

	public int ColumnWidth { get; set; }

	public Type Schema { get; set; }

	public string Table { get; set; }

	public string DisplayValueOf { get; set; }

	public DataBundleKeyAttribute()
	{
		ColumnWidth = defaultWidth;
	}
}
