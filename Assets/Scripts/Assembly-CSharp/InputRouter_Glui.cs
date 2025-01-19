using UnityEngine;

[AddComponentMenu("Input/Input Router Glui")]
public class InputRouter_Glui : InputRouter
{
	public InputRouter_Glui()
		: base(InputRouter_GluiSearchMethod)
	{
	}

	private static Component[] InputRouter_GluiSearchMethod(GameObject o)
	{
		if (o == null)
		{
			return null;
		}
		return o.GetComponents<GluiWidget>();
	}
}
