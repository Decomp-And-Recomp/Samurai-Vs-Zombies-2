using System;

[AttributeUsage(AttributeTargets.Field)]
public class DataBundleFieldAttribute : Attribute
{
	public static readonly int defaultWidth = 100;

	public int Identifier { get; set; }

	public int ColumnWidth { get; set; }

	public bool StaticResource { get; set; }

	public string TooltipInfo { get; set; }

	public bool IgnoreOnBuild { get; set; }

	public DataBundleResourceGroup Group { get; set; }

	public DataBundleFieldAttribute()
	{
		Identifier = -1;
		ColumnWidth = defaultWidth;
		StaticResource = false;
		TooltipInfo = string.Empty;
		IgnoreOnBuild = false;
		Group = DataBundleResourceGroup.InGame;
	}
}
