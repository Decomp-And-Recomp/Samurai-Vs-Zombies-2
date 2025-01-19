using System;

[AttributeUsage(AttributeTargets.Class)]
public class DataBundleClassAttribute : Attribute
{
	public bool TransposeView { get; set; }

	public string DisplayName { get; set; }

	public bool Localizable { get; set; }

	public string Category { get; set; }

	public string Comment { get; set; }

	public DataBundleClassAttribute()
	{
		TransposeView = false;
		DisplayName = null;
		Localizable = false;
		Category = string.Empty;
		Comment = string.Empty;
	}
}
