using System.Collections;

public class SmarterStringIterator
{
	private IEnumerator e;

	private bool valid = true;

	public string Current
	{
		get
		{
			if (!valid)
			{
				return null;
			}
			return (string)e.Current;
		}
	}

	public SmarterStringIterator(string[] source)
	{
		e = source.GetEnumerator();
		valid = e.MoveNext();
	}

	public void MoveNext()
	{
		if (valid)
		{
			valid = e.MoveNext();
		}
	}
}
