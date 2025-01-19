[DataBundleClass(Category = "Design")]
public class HeroStarsSchema
{
	[DataBundleKey]
	public string id;

	public int speed;

	public int ranged;

	public int melee;

	public int health;

	public int allySlots;

	public int ability;
}
