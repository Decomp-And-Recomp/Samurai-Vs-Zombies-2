[DataBundleClass(Category = "Design", Comment = "Daily Rewards")]
public class DailyRewardSchema
{
	public enum Type
	{
		Coins = 0,
		Gems = 1,
		Revives = 2
	}

	[DataBundleKey]
	public string id;

	public Type type;

	public int num;

	public void Initialize(string tableName)
	{
	}
}
