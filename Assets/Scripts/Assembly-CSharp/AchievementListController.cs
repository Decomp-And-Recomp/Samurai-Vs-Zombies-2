public class AchievementListController : GluiSimpleCollectionController
{
	public override void ReloadData(object arg)
	{
		mData = Singleton<Achievements>.Instance.GetAchievements();
	}
}
