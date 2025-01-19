public interface IDataSource
{
	string[] allIDs { get; }

	bool Contains(string id);

	string GetAttribute(string id, string attributeName);

	string GetAttributeOrNull(string id, string attributeName);
}
