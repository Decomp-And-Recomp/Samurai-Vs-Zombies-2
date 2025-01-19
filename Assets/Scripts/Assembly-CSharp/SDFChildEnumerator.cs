using System.Collections;
using System.Collections.Generic;

public class SDFChildEnumerator : IEnumerable
{
	private Dictionary<string, SDFTreeNode> mChildsRef;

	public SDFChildEnumerator(Dictionary<string, SDFTreeNode> childs)
	{
		mChildsRef = childs;
	}

	public IEnumerator GetEnumerator()
	{
		return ((IEnumerable)mChildsRef).GetEnumerator();
	}
}
