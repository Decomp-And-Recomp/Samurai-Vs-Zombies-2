using System;

[AttributeUsage(AttributeTargets.Field)]
public class DataBundleSchemaFilterAttribute : Attribute
{
	public Type Schema { get; set; }

	public bool DontFollowRecordLink { get; set; }

	public DataBundleSchemaFilterAttribute(Type schemaFilter, bool dontFollowRecordLink = false)
	{
		Schema = schemaFilter;
		DontFollowRecordLink = dontFollowRecordLink;
	}
}
