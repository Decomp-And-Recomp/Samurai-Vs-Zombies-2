using System;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class DataBundleDropdownFilter : Attribute
{
	public static readonly char separator = ';';

	public static readonly string indexLead = "#";

	public static readonly string fieldLead = "@";

	public static readonly string allTables = "*";

	public string CurrentTable { get; set; }

	public string Filter { get; set; }

	public DataBundleDropdownFilter()
	{
	}

	public DataBundleDropdownFilter(string tableName)
	{
		CurrentTable = tableName;
	}
}
