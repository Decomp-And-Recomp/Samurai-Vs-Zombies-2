public class GluiLocale
{
	public readonly string name;

	public GluiStringTable stringTable;

	public GluiLocale(string name)
	{
		this.name = name;
		string path = "Glui/Strings/" + name;
		stringTable = new GluiStringTable(path);
	}

	public void Save()
	{
		string path = "Glui/Strings/" + name + ".txt";
		stringTable.Save(path);
	}

	public string Find(string id)
	{
		return stringTable.Find(id);
	}
}
