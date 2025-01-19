[DataBundleClass(Category = "Design", Comment = "Conditions and effects for allies that buff other allies")]
public class BuffSchema
{
	[DataBundleKey]
	public string id;

	public float damageBuffPercent;

	public int leadershipCostModifier;

	public bool affectMount;

	public bool affectMounted;

	public bool CanBuff(CharacterData cd)
	{
		return (affectMount && cd.isMount) || (affectMounted && cd.isMounted);
	}

	public bool CanBuff(Character c)
	{
		return (affectMount && c.isMount) || (affectMounted && c.isMounted);
	}

	public bool CanBuff(HelperSchema h)
	{
		return (affectMount && h.isMount) || (affectMounted && h.isMounted);
	}
}
