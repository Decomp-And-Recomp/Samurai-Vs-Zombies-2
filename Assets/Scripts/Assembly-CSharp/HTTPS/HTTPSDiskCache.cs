using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace HTTPS
{
	public class HTTPSDiskCache : MonoBehaviour
	{
		private string cachePath;

		private static HTTPSDiskCache _instance;

		public static HTTPSDiskCache Instance
		{
			get
			{
				if (_instance == null)
				{
					GameObject gameObject = new GameObject("HTTPSDiskCache", typeof(HTTPSDiskCache));
					gameObject.hideFlags = HideFlags.HideAndDontSave;
					_instance = gameObject.GetComponent<HTTPSDiskCache>();
				}
				return _instance;
			}
		}

		private void Awake()
		{
			cachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "uwcache");
			if (!Directory.Exists(cachePath))
			{
				Directory.CreateDirectory(cachePath);
			}
		}

		public HTTPSDiskCacheOperation Fetch(HTTPSRequest request)
		{
			string text = string.Empty;
			byte[] array = MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(request.uri.ToString()));
			foreach (byte b in array)
			{
				text += b.ToString("X2");
			}
			string text2 = Path.Combine(cachePath, text);
			if (File.Exists(text2) && File.Exists(text2 + ".etag"))
			{
				request.SetHeader("If-None-Match", File.ReadAllText(text2 + ".etag"));
			}
			HTTPSDiskCacheOperation hTTPSDiskCacheOperation = new HTTPSDiskCacheOperation();
			hTTPSDiskCacheOperation.request = request;
			StartCoroutine(DownloadAndSave(request, text2, hTTPSDiskCacheOperation));
			return hTTPSDiskCacheOperation;
		}

		private IEnumerator DownloadAndSave(HTTPSRequest request, string filename, HTTPSDiskCacheOperation handle)
		{
			bool useCachedVersion = File.Exists(filename);
			request.Send();
			while (!request.isDone)
			{
				yield return new WaitForEndOfFrame();
			}
			if (request.exception == null && request.response != null && request.response.status == 200)
			{
				string etag = request.response.GetHeader("etag");
				if (etag != string.Empty)
				{
					File.WriteAllBytes(filename, request.response.bytes);
					File.WriteAllText(filename + ".etag", etag);
				}
				useCachedVersion = false;
			}
			if (useCachedVersion)
			{
				if (request.exception != null)
				{
					request.exception = null;
				}
				request.response.status = 304;
				request.response.bytes = File.ReadAllBytes(filename);
				request.isDone = true;
			}
			handle.isDone = true;
		}
	}
}
