public class AgentKey
{
	public string hash;

	public AgentKey(string id)
	{
		hash = id;
	}

	public AgentKey(string id, string dataKey)
	{
		hash = id + '@' + dataKey;
	}
}
