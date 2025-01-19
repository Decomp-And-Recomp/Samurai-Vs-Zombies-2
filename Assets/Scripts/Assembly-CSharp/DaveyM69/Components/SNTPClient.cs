using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using DaveyM69.Components.SNTP;

namespace DaveyM69.Components
{
	[DefaultProperty("RemoteSNTPServer")]
	[DefaultEvent("QueryServerCompleted")]
	public class SNTPClient : Component
	{
		private delegate void WorkerThreadStartDelegate();

		public const int DefaultTimeout = 5000;

		public const VersionNumber DefaultVersionNumber = VersionNumber.Version3;

		private int _Timeout;

		private AsyncOperation asyncOperation;

		public static readonly RemoteSNTPServer DefaultServer = RemoteSNTPServer.Default;

		private readonly SendOrPostCallback operationCompleted;

		private readonly WorkerThreadStartDelegate threadStart;

		[Browsable(false)]
		public bool IsBusy { get; private set; }

		public static DateTime Now
		{
			get
			{
				return GetNow();
			}
		}

		[Category("Connection")]
		[Description("The server to use.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public RemoteSNTPServer RemoteSNTPServer { get; set; }

		[DefaultValue(0)]
		public int CurrentServerIndex { get; set; }

		[Description("The timeout in milliseconds used for sending and receiving.")]
		[Category("Connection")]
		[DefaultValue(5000)]
		public int Timeout
		{
			get
			{
				return _Timeout;
			}
			set
			{
				if (value < -1)
				{
					value = 5000;
				}
				_Timeout = value;
			}
		}

		[DefaultValue(VersionNumber.Version3)]
		[Description("The NTP/SNTP version to use.")]
		[Category("Connection")]
		public VersionNumber VersionNumber { get; set; }

		[Description("Raised when a query to the server completes successfully.")]
		[Category("Success")]
		[method: MethodImpl(32)]
		public event EventHandler<QueryServerCompletedEventArgs> QueryServerCompleted;

		public SNTPClient()
		{
			Initialize();
			threadStart = WorkerThreadStart;
			operationCompleted = AsyncOperationCompleted;
			Timeout = 5000;
			VersionNumber = VersionNumber.Version3;
		}

		public static TimeSpan GetCurrentLocalTimeZoneOffset()
		{
			return TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
		}

		public static DateTime GetNow()
		{
			return GetNow(RemoteSNTPServer.Default, 500);
		}

		public static DateTime GetNow(RemoteSNTPServer remoteSNTPServer)
		{
			return GetNow(remoteSNTPServer, 500);
		}

		public static DateTime GetNow(int timeout)
		{
			return GetNow(RemoteSNTPServer.Default, timeout);
		}

		public static DateTime GetNow(RemoteSNTPServer remoteSNTPServer, int timeout)
		{
			SNTPClient sNTPClient = new SNTPClient();
			sNTPClient.RemoteSNTPServer = remoteSNTPServer;
			sNTPClient.Timeout = timeout;
			QueryServerCompletedEventArgs queryServerCompletedEventArgs = sNTPClient.QueryServer();
			if (queryServerCompletedEventArgs.Succeeded)
			{
				return DateTime.Now.AddSeconds(queryServerCompletedEventArgs.Data.LocalClockOffset);
			}
			return DateTime.MinValue;
		}

		public bool QueryServerAsync()
		{
			bool result = false;
			if (!IsBusy)
			{
				IsBusy = true;
				asyncOperation = AsyncOperationManager.CreateOperation(null);
				threadStart.BeginInvoke(null, null);
				result = true;
			}
			return result;
		}

		protected virtual void OnQueryServerCompleted(QueryServerCompletedEventArgs e)
		{
			EventHandler<QueryServerCompletedEventArgs> queryServerCompleted = this.QueryServerCompleted;
			if (queryServerCompleted != null)
			{
				queryServerCompleted(this, e);
			}
		}

		private void AsyncOperationCompleted(object arg)
		{
			IsBusy = false;
			OnQueryServerCompleted((QueryServerCompletedEventArgs)arg);
		}

		private void Initialize()
		{
			if (RemoteSNTPServer == null)
			{
				RemoteSNTPServer = DefaultServer;
			}
		}

		private QueryServerCompletedEventArgs QueryServer()
		{
			QueryServerCompletedEventArgs queryServerCompletedEventArgs = new QueryServerCompletedEventArgs();
			Initialize();
			UdpClient udpClient = null;
			bool flag = false;
			try
			{
				udpClient = new UdpClient();
				IPEndPoint remoteEP = RemoteSNTPServer.GetIPEndPoint();
				udpClient.Client.SendTimeout = Timeout;
				udpClient.Client.ReceiveTimeout = Timeout;
				udpClient.Connect(remoteEP);
				SNTPData clientRequestPacket = SNTPData.GetClientRequestPacket(VersionNumber);
				udpClient.Send(clientRequestPacket, clientRequestPacket.Length);
				queryServerCompletedEventArgs.Data = udpClient.Receive(ref remoteEP);
				queryServerCompletedEventArgs.Data.DestinationDateTime = DateTime.Now.ToUniversalTime();
				if (queryServerCompletedEventArgs.Data.Mode == Mode.Server)
				{
					queryServerCompletedEventArgs.Succeeded = true;
				}
				else
				{
					flag = true;
				}
				return queryServerCompletedEventArgs;
			}
			catch (Exception exception)
			{
				queryServerCompletedEventArgs.ErrorData = new ErrorData(exception);
				flag = true;
			}
			finally
			{
				if (flag)
				{
					CurrentServerIndex++;
					if (CurrentServerIndex == RemoteSNTPServer.TimeServerList.Length)
					{
						queryServerCompletedEventArgs.ErrorData = new ErrorData("The response from the server was invalid.");
					}
					else
					{
						RemoteSNTPServer = RemoteSNTPServer.TimeServerList[CurrentServerIndex];
						queryServerCompletedEventArgs = QueryServer();
					}
				}
				if (udpClient != null)
				{
					udpClient.Close();
				}
			}
			return queryServerCompletedEventArgs;
		}

		private void WorkerThreadStart()
		{
			lock (this)
			{
				QueryServerCompletedEventArgs queryServerCompletedEventArgs = null;
				try
				{
					queryServerCompletedEventArgs = QueryServer();
				}
				catch (Exception ex)
				{
					throw ex;
				}
				asyncOperation.PostOperationCompleted(operationCompleted, queryServerCompletedEventArgs);
			}
		}
	}
}
