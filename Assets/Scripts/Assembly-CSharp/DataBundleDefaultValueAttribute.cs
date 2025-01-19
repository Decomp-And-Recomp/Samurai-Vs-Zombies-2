using System;

[AttributeUsage(AttributeTargets.Field)]
public class DataBundleDefaultValueAttribute : Attribute
{
	public object Value { get; set; }

	public DataBundleDefaultValueAttribute(string value)
	{
		Value = value;
	}

	public DataBundleDefaultValueAttribute(int value)
	{
		Value = value;
	}

	public DataBundleDefaultValueAttribute(float value)
	{
		Value = value;
	}

	public DataBundleDefaultValueAttribute(bool value)
	{
		Value = value;
	}
}
