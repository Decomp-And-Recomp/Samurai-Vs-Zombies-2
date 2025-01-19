using System;

[Flags]
public enum DataBundleResourceGroup
{
	None = 0,
	FrontEnd = 1,
	InGame = 2,
	Preview = 4,
	All = 7,
	Default = 2
}
