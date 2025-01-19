using System;

[Serializable]
public class GluiPersistentDataValue_Float : GluiPersistentDataValue<float>
{
	public string PersistentEntry_Float;

	public float defaultValue;

	public float GetValue()
	{
		if (string.IsNullOrEmpty(PersistentEntry_Float))
		{
			return defaultValue;
		}
		return GetValue_Generic(PersistentEntry_Float, defaultValue);
	}
}
