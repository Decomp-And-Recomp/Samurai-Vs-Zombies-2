using System.Collections;
using System.Collections.Generic;

public class SDFAttributesEnumerator : IEnumerable
{
	private Dictionary<string, string> mAttributesRef;

	public SDFAttributesEnumerator(Dictionary<string, string> attributes)
	{
		mAttributesRef = attributes;
	}

	public IEnumerator GetEnumerator()
	{
		return ((IEnumerable)mAttributesRef).GetEnumerator();
	}
}
