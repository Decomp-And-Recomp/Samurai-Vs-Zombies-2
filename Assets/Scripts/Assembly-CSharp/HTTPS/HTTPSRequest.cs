using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace HTTPS
{
	public class HTTPSRequest
	{
		public string method = "GET";

		public string protocol = "HTTP/1.1";

		public byte[] bytes;

		public Uri uri;

		public static byte[] EOL = new byte[2] { 13, 10 };

		public HTTPSResponse response;

		public bool isDone;

		public int maximumRetryCount = 8;

		public bool acceptGzip;

		public bool useCache;

		public Exception exception;

		public HTTPSRequestState state;

		public ManualResetEvent resetEvent;

		public bool useCertificates;

		public X509CertificateCollection certificateCollection;

		public SslProtocols sslProtocols;

		public static List<ManualResetEvent> activeResetEvents = new List<ManualResetEvent>();

		private Dictionary<string, List<string>> headers = new Dictionary<string, List<string>>();

		private static Dictionary<string, string> etags = new Dictionary<string, string>();

		public string Text
		{
			set
			{
				bytes = Encoding.UTF8.GetBytes(value);
			}
		}

		public HTTPSRequest(string method, string uri)
		{
			this.method = method;
			this.uri = new Uri(uri);
		}

		public HTTPSRequest(string method, string uri, bool useCache)
		{
			this.method = method;
			this.uri = new Uri(uri);
			this.useCache = useCache;
		}

		public HTTPSRequest(string method, string uri, byte[] bytes)
		{
			this.method = method;
			this.uri = new Uri(uri);
			this.bytes = bytes;
		}

		public void SetCertificateAndProtocols(X509CertificateCollection xcc, SslProtocols sslp)
		{
			useCertificates = true;
			certificateCollection = xcc;
			sslProtocols = sslp;
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
			resetEvent = new ManualResetEvent(false);
			activeResetEvents.Add(resetEvent);
			state = HTTPSRequestState.Waiting;
			if (acceptGzip)
			{
				SetHeader("Accept-Encoding", "gzip");
			}
			ThreadPool.QueueUserWorkItem(delegate
			{
				try
				{
					int num = 0;
					while (++num < maximumRetryCount)
					{
						if (useCache)
						{
							string value = string.Empty;
							if (etags.TryGetValue(uri.AbsoluteUri, out value))
							{
								SetHeader("If-None-Match", value);
							}
						}
						SetHeader("Host", uri.Host);
						TcpClient tcpClient = new TcpClient();
						tcpClient.Connect(uri.Host, uri.Port);
						using (NetworkStream networkStream = tcpClient.GetStream())
						{
							Stream stream = networkStream;
							if (uri.Scheme.ToLower() == "https")
							{
								if (!useCertificates)
								{
									stream = new SslStream(networkStream, false, ValidateServerCertificate);
									SslStream sslStream = stream as SslStream;
									sslStream.AuthenticateAsClient(uri.Host);
								}
								else
								{
									stream = new SslStream(networkStream, false, ValidateServerCertificate, SelectLocalCertificate);
									SslStream sslStream2 = stream as SslStream;
									sslStream2.AuthenticateAsClient(uri.Host, certificateCollection, sslProtocols, false);
								}
							}
							WriteToStream(stream);
							response = new HTTPSResponse();
							state = HTTPSRequestState.Reading;
							response.ReadFromStream(stream);
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
						string header = response.GetHeader("etag");
						if (header.Length > 0)
						{
							etags[uri.AbsoluteUri] = header;
						}
					}
				}
				catch (Exception value2)
				{
					Console.WriteLine("Unhandled Exception, aborting request.");
					Console.WriteLine(value2);
					exception = value2;
					response = null;
				}
				finally
				{
					resetEvent.Set();
					activeResetEvents.Remove(resetEvent);
				}
				state = HTTPSRequestState.Done;
				isDone = true;
			});
		}

		public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}

		public static void WaitForActiveRequestsToComplete()
		{
			WaitHandle.WaitAll(activeResetEvents.ToArray());
		}

		public static X509Certificate SelectLocalCertificate(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
		{
			if (acceptableIssuers != null && acceptableIssuers.Length > 0 && localCertificates != null && localCertificates.Count > 0)
			{
				foreach (X509Certificate localCertificate in localCertificates)
				{
					string issuer = localCertificate.Issuer;
					if (Array.IndexOf(acceptableIssuers, issuer) != -1)
					{
						return localCertificate;
					}
				}
			}
			if (localCertificates != null && localCertificates.Count > 0)
			{
				return localCertificates[0];
			}
			return null;
		}

		private void WriteToStream(Stream outputStream)
		{
			BinaryWriter binaryWriter = new BinaryWriter(outputStream);
			bool flag = false;
			binaryWriter.Write(Encoding.ASCII.GetBytes(method.ToUpper() + " " + uri.PathAndQuery + " " + protocol));
			binaryWriter.Write(EOL);
			if (bytes != null && bytes.Length > 0)
			{
				SetHeader("Content-Length", bytes.Length.ToString());
				flag = true;
			}
			else
			{
				PopHeader("Content-Length");
			}
			foreach (string key in headers.Keys)
			{
				foreach (string item in headers[key])
				{
					binaryWriter.Write(Encoding.ASCII.GetBytes(key + ": " + item));
					binaryWriter.Write(EOL);
				}
			}
			binaryWriter.Write(EOL);
			if (flag)
			{
				binaryWriter.Write(bytes);
			}
		}
	}
}
