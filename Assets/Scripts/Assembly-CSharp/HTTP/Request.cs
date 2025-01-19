using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace HTTP
{
	public class Request
	{
		public string method = "GET";

		public string protocol = "HTTP/1.1";

		public byte[] bytes;

		public Uri uri;

		public static byte[] EOL = new byte[2] { 13, 10 };

		public Response response;

		public bool isDone;

		public int maximumRetryCount = 8;

		public bool acceptGzip;

		public bool useCache;

		public Exception exception;

		public RequestState state;

		public Dictionary<string, List<string>> headers = new Dictionary<string, List<string>>();

		private static Dictionary<string, string> etags = new Dictionary<string, string>();

		public string Text
		{
			get
			{
				if (bytes == null)
				{
					return string.Empty;
				}
				return Encoding.UTF8.GetString(bytes);
			}
			set
			{
				bytes = Encoding.UTF8.GetBytes(value);
			}
		}

		public Request(string method, string uri)
		{
			this.method = method;
			this.uri = new Uri(uri);
		}

		public Request(string method, string uri, bool useCache)
		{
			this.method = method;
			this.uri = new Uri(uri);
			this.useCache = useCache;
		}

		public Request(string method, string uri, byte[] bytes)
		{
			this.method = method;
			this.uri = new Uri(uri);
			this.bytes = bytes;
		}

		public void AddHeader(string name, string value)
		{
			GetHeaders(name).Add(value);
		}

		public string GetHeader(string name)
		{
			List<string> list = GetHeaders(name);
			if (list.Count == 0)
			{
				return string.Empty;
			}
			return list[0];
		}

		public List<string> GetHeaders(string name)
		{
			foreach (string key in headers.Keys)
			{
				if (string.Compare(name, key, true) == 0)
				{
					return headers[key];
				}
			}
			List<string> list = new List<string>();
			headers.Add(name, list);
			return list;
		}

		public void SetHeader(string name, string value)
		{
			List<string> list = GetHeaders(name);
			list.Clear();
			list.Add(value);
		}

		public void PopHeader(string name)
		{
			if (headers.ContainsKey(name))
			{
				headers.Remove(name);
			}
		}

		public void Send()
		{
			isDone = false;
			state = RequestState.Waiting;
			if (acceptGzip)
			{
				SetHeader("Accept-Encoding", "gzip");
			}
			WaitCallback waitCallback = delegate
			{
				try
				{
					int num = 0;
					while (++num < maximumRetryCount)
					{
						if (useCache)
						{
							try
							{
								string value = string.Empty;
								if (etags.TryGetValue(uri.AbsoluteUri, out value))
								{
									lock (headers)
									{
										SetHeader("If-None-Match", value);
									}
								}
							}
							catch (Exception)
							{
							}
						}
						try
						{
							SetHeader("Host", uri.Host);
						}
						catch (Exception)
						{
						}
						TcpClient tcpClient = new TcpClient();
						tcpClient.Connect(uri.Host, uri.Port);
						using (NetworkStream networkStream = tcpClient.GetStream())
						{
							Stream stream = networkStream;
							if (uri.Scheme.ToLower() == "https")
							{
								stream = new SslStream(networkStream, false, ValidateServerCertificate);
								try
								{
									SslStream sslStream = stream as SslStream;
									sslStream.AuthenticateAsClient(uri.Host);
								}
								catch (Exception ex3)
								{
									exception = ex3;
									state = RequestState.Done;
									isDone = true;
									return;
								}
							}
							WriteToStream(stream);
							try
							{
								response = new Response();
								state = RequestState.Reading;
								response.ReadFromStream(stream);
							}
							catch (Exception)
							{
							}
						}
						tcpClient.Close();
						int status = response.status;
						if (status == 301 || status == 302 || status == 307)
						{
							uri = new Uri(response.GetHeader("Location"));
						}
						else
						{
							num = maximumRetryCount;
						}
					}
					if (useCache)
					{
						try
						{
							string header = response.GetHeader("etag");
							if (header.Length > 0)
							{
								etags[uri.AbsoluteUri] = header;
							}
						}
						catch (Exception)
						{
						}
					}
				}
				catch (Exception value2)
				{
					Console.WriteLine("Caught Exception, aborting request.");
					Console.WriteLine(value2);
					exception = value2;
					response = null;
				}
				state = RequestState.Done;
				isDone = true;
			};
			if (!ApplicationUtilities.HasShutdown)
			{
				ThreadPool.QueueUserWorkItem(waitCallback);
			}
			else
			{
				waitCallback(null);
			}
		}

		public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}

		private void WriteToStream(Stream outputStream)
		{
			BinaryWriter binaryWriter = new BinaryWriter(outputStream);
			bool flag = false;
			binaryWriter.Write(Encoding.ASCII.GetBytes(method.ToUpper() + " " + uri.PathAndQuery + " " + protocol));
			binaryWriter.Write(EOL);
			try
			{
				lock (headers)
				{
					if (bytes != null && bytes.Length > 0)
					{
						SetHeader("Content-Length", bytes.Length.ToString());
						flag = true;
					}
					else
					{
						PopHeader("Content-Length");
					}
				}
			}
			catch (Exception)
			{
			}
			try
			{
				lock (headers)
				{
					foreach (string key in headers.Keys)
					{
						foreach (string item in headers[key])
						{
							binaryWriter.Write(Encoding.ASCII.GetBytes(key + ": " + item));
							binaryWriter.Write(EOL);
						}
					}
				}
			}
			catch (Exception)
			{
			}
			binaryWriter.Write(EOL);
			if (flag)
			{
				binaryWriter.Write(bytes);
			}
		}
	}
}
