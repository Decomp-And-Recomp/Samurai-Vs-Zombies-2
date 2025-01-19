using System;

[AttributeUsage(AttributeTargets.Method)]
public class DataBundleCustomContextCallbackAttribute : Attribute
{
	public Type Type { get; set; }

	public string Label { get; set; }

	public DataBundleCustomContextCallbackAttribute(Type type, string label)
	{
		Type = type;
		Label = label;
	}
}
