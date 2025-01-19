using System;
using System.Collections;
using HTTPS;
using UnityEngine;

public class RetrieveProxy : MonoBehaviour
{
	public string url;

	public int gameId;

	private SimpleHTTPS sdub;

	private bool _proxyRetrieved;

	private string _ip;

	private int _port;

	private string _password;

	private string _encryptedPassword;

	private bool _error;

	private string _errorString;

	public bool proxyRetrieved
	{
		get
		{
			return _proxyRetrieved;
		}
	}

	public string ip
	{
		get
		{
			return _ip;
		}
	}

	public int port
	{
		get
		{
			return _port;
		}
	}

	public string password
	{
		get
		{
			return _password;
		}
	}

	public string encryptedPassword
	{
		get
		{
			return _encryptedPassword;
		}
	}

	public bool error
	{
		get
		{
			return _error;
		}
	}

	public string errorString
	{
		get
		{
			return _errorString;
		}
	}

	private void Start()
	{
		sdub = base.gameObject.GetComponent<SimpleHTTPS>();
	}

	public void GetServer()
	{
		DoRequest(url + "/?gameId=" + gameId);
	}

	public void RetryGetServer(string oldIp, int oldPort)
	{
		DoRequest(url + "/?gameId=" + gameId + "&fail=" + oldIp + ":" + oldPort);
	}

	private void DoRequest(string url)
	{
		_proxyRetrieved = false;
		_ip = null;
		_port = 0;
		_error = false;
		_errorString = null;
		if (sdub == null)
		{
			sdub = base.gameObject.AddComponent<SimpleHTTPS>();
		}
		HTTPSRequest hTTPSRequest = new HTTPSRequest("GET", url);
		hTTPSRequest.SetHeader("Authorization", BuildAuthToken());
		sdub.Send(hTTPSRequest, Result);
	}

	private static string BuildAuthToken()
	{
		string[] array = new string[8];
		array[3] = "rkY";
		array[5] = "hhjioewhtJVIDG";
		array[6] = "6FJ2eTM";
		array[2] = "YLjfeigJFDKW";
		array[0] = "4940jdFJEGIO";
		array[4] = "lsplRc";
		array[1] = "KH18X4";
		array[5] = "iQDSv";
		array[7] = "ibANOMDnZiSdYsd";
		array[0] = "P7GQYDn0lW";
		array[2] = "YL7s0ZKBbh9W";
		return string.Join(string.Empty, array);
	}

	private void Result(HTTPSResponse resp)
	{
		if (resp.Text == null)
		{
			_error = true;
			_errorString = resp.message;
			return;
		}
		Hashtable hashtable = (Hashtable)JSON.Decode(resp.Text);
		if (hashtable == null)
		{
			_error = true;
			_errorString = resp.message;
			return;
		}
		if (hashtable.ContainsKey("error"))
		{
			_error = true;
			_errorString = hashtable["error"].ToString();
			return;
		}
		_ip = hashtable["ip"].ToString();
		_port = int.Parse(hashtable["port"].ToString());
		if (hashtable.ContainsKey("password"))
		{
			_encryptedPassword = hashtable["password"].ToString();
			try
			{
				_password = DecryptPassword(_encryptedPassword);
			}
			catch (Exception)
			{
				_password = string.Empty;
				_encryptedPassword = string.Empty;
			}
		}
		else
		{
			_encryptedPassword = string.Empty;
			_password = string.Empty;
		}
		_proxyRetrieved = true;
	}

	public static string DecryptPassword(string password)
	{
		char[] array = password.ToCharArray();
		string text = string.Empty;
		int num = 0;
		int num2 = 0;
		int num3 = 65;
		int num4 = 90;
		int num5 = 97;
		int num6 = 122;
		int num7 = 48;
		int num8 = 57;
		while (num2 < array.Length)
		{
			int num9 = array[num2];
			if (num9 >= num3 && num9 <= num4)
			{
				int i;
				for (i = num9 - (num + num3); i < 0; i += 26)
				{
				}
				text += (char)(i + num7);
			}
			if (num9 >= num5 && num9 <= num6)
			{
				text += (char)(num6 - num9 + num3);
			}
			if (num9 >= num7 && num9 <= num8)
			{
				int num10 = 0;
				int num11 = 1;
				while (num9 >= num7 && num9 <= num8)
				{
					num10 += (num9 - num7) * num11;
					num11 *= 10;
					num2++;
					if (num2 >= array.Length)
					{
						throw new Exception("bad password");
					}
					num9 = array[num2];
				}
				text += (char)(num6 - num10 + 5);
			}
			num2 += 2;
			num++;
		}
		return text;
	}
}
