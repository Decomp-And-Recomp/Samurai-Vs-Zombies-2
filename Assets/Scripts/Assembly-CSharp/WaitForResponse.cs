using System.Collections;

public class WaitForResponse<T> : IEnumerator
{
	private bool responseRecieved;

	private T response;

	public object Current
	{
		get
		{
			return null;
		}
	}

	public T Response
	{
		get
		{
			return response;
		}
		set
		{
			response = value;
			responseRecieved = true;
		}
	}

	public bool MoveNext()
	{
		return !responseRecieved;
	}

	public void Reset()
	{
		responseRecieved = false;
	}
}
