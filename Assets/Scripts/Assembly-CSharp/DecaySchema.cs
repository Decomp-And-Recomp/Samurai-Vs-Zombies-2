[DataBundleClass]
public class DecaySchema
{
	[DataBundleKey(ColumnWidth = 200)]
	public string name;

	public float minutesPerTick;

	public bool allowFastForward;

	public string actionOnTick;

	[DataBundleField(ColumnWidth = 200, TooltipInfo = "Text for game implementation")]
	public string displayTitle;

	[DataBundleField(ColumnWidth = 200, TooltipInfo = "Text for game implementation")]
	public string displayMessage;
}
