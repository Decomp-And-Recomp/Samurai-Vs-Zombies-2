[DataBundleClass(Category = "Audio")]
public class USoundBusSchema
{
	[DataBundleField(ColumnWidth = 300)]
	[DataBundleKey]
	public string busName;

	[DataBundleField(ColumnWidth = 300, TooltipInfo = "Should sound events on this bus pause when gameplay is paused?")]
	public bool pauseWithGameplay;

	public static implicit operator bool(USoundBusSchema obj)
	{
		return obj != null;
	}
}
