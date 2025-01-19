using System.Collections;
using UnityEngine;

namespace HTTPS
{
	public class SimpleHTTPS : MonoBehaviour
	{
		public delegate void HTTPSResponseDelegate(HTTPSResponse response);

		public void Send(HTTPSRequest request, HTTPSResponseDelegate responseDelegate)
		{
			StartCoroutine(_Send(request, responseDelegate));
		}

		private IEnumerator _Send(HTTPSRequest request, HTTPSResponseDelegate responseDelegate)
		{
			request.Send();
			while (!request.isDone)
			{
				yield return new WaitForEndOfFrame();
			}
			if (request.exception == null)
			{
				responseDelegate(request.response);
			}
		}
	}
}
