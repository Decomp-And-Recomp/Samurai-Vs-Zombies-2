public class GluiPersistentDataValue<T>
{
	protected T GetValue_Generic(string PersistentEntryForValue, T defaultValue)
	{
		if (PersistentEntryForValue != string.Empty)
		{
			T val = (T)SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.GetData(PersistentEntryForValue);
			if (val != null)
			{
				return val;
			}
		}
		return defaultValue;
	}
}
