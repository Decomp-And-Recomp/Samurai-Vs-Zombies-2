using System.ComponentModel;
using System.Net;

namespace DaveyM69.Components.SNTP
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class RemoteSNTPServer
	{
		public const int DefaultPort = 123;

		private string _HostNameOrAddress;

		private int _Port;

		public static readonly RemoteSNTPServer[] TimeServerList = new RemoteSNTPServer[62]
		{
			new RemoteSNTPServer("time.windows.com"),
			new RemoteSNTPServer("africa.pool.ntp.org"),
			new RemoteSNTPServer("time1.euro.apple.com"),
			new RemoteSNTPServer("time.euro.apple.com"),
			new RemoteSNTPServer("asia.pool.ntp.org"),
			new RemoteSNTPServer("0.asia.pool.ntp.org"),
			new RemoteSNTPServer("1.asia.pool.ntp.org"),
			new RemoteSNTPServer("2.asia.pool.ntp.org"),
			new RemoteSNTPServer("3.asia.pool.ntp.org"),
			new RemoteSNTPServer("au.pool.ntp.org"),
			new RemoteSNTPServer("0.au.pool.ntp.org"),
			new RemoteSNTPServer("1.au.pool.ntp.org"),
			new RemoteSNTPServer("2.au.pool.ntp.org"),
			new RemoteSNTPServer("3.au.pool.ntp.org"),
			new RemoteSNTPServer("ntp.blueyonder.co.uk"),
			new RemoteSNTPServer("ca.pool.ntp.org"),
			new RemoteSNTPServer("0.ca.pool.ntp.org"),
			new RemoteSNTPServer("1.ca.pool.ntp.org"),
			new RemoteSNTPServer("2.ca.pool.ntp.org"),
			new RemoteSNTPServer("3.ca.pool.ntp.org"),
			new RemoteSNTPServer("europe.pool.ntp.org"),
			new RemoteSNTPServer("0.europe.pool.ntp.org"),
			new RemoteSNTPServer("1.europe.pool.ntp.org"),
			new RemoteSNTPServer("2.europe.pool.ntp.org"),
			new RemoteSNTPServer("3.europe.pool.ntp.org"),
			new RemoteSNTPServer("time-nw.nist.gov"),
			new RemoteSNTPServer("north-america.pool.ntp.org"),
			new RemoteSNTPServer("0.north-america.pool.ntp.org"),
			new RemoteSNTPServer("1.north-america.pool.ntp.org"),
			new RemoteSNTPServer("2.north-america.pool.ntp.org"),
			new RemoteSNTPServer("3.north-america.pool.ntp.org"),
			new RemoteSNTPServer("time.cableol.net"),
			new RemoteSNTPServer("oceania.pool.ntp.org"),
			new RemoteSNTPServer("0.oceania.pool.ntp.org"),
			new RemoteSNTPServer("1.oceania.pool.ntp.org"),
			new RemoteSNTPServer("2.oceania.pool.ntp.org"),
			new RemoteSNTPServer("3.oceania.pool.ntp.org"),
			new RemoteSNTPServer("pool.ntp.org"),
			new RemoteSNTPServer("0.pool.ntp.org"),
			new RemoteSNTPServer("1.pool.ntp.org"),
			new RemoteSNTPServer("2.pool.ntp.org"),
			new RemoteSNTPServer("south-america.pool.ntp.org"),
			new RemoteSNTPServer("0.south-america.pool.ntp.org"),
			new RemoteSNTPServer("1.south-america.pool.ntp.org"),
			new RemoteSNTPServer("2.south-america.pool.ntp.org"),
			new RemoteSNTPServer("3.south-america.pool.ntp.org"),
			new RemoteSNTPServer("ntp1.ja.net"),
			new RemoteSNTPServer("uk.pool.ntp.org"),
			new RemoteSNTPServer("0.uk.pool.ntp.org"),
			new RemoteSNTPServer("1.uk.pool.ntp.org"),
			new RemoteSNTPServer("2.uk.pool.ntp.org"),
			new RemoteSNTPServer("3.uk.pool.ntp.org"),
			new RemoteSNTPServer("us.pool.ntp.org"),
			new RemoteSNTPServer("0.us.pool.ntp.org"),
			new RemoteSNTPServer("1.us.pool.ntp.org"),
			new RemoteSNTPServer("2.us.pool.ntp.org"),
			new RemoteSNTPServer("3.us.pool.ntp.org"),
			new RemoteSNTPServer("tock.usno.navy.mil"),
			new RemoteSNTPServer("tick.usno.navy.mil"),
			new RemoteSNTPServer("ntp1.usno.navy.mil"),
			new RemoteSNTPServer("clock.xmission.com"),
			new RemoteSNTPServer("time.nist.gov")
		};

		public static readonly RemoteSNTPServer Default = new RemoteSNTPServer();

		[NotifyParentProperty(true)]
		[Description("The host name or address of the server.")]
		public string HostNameOrAddress
		{
			get
			{
				return _HostNameOrAddress;
			}
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					value = TimeServerList[0].HostNameOrAddress;
				}
				value = value.Trim();
				_HostNameOrAddress = value;
			}
		}

		[NotifyParentProperty(true)]
		[Description("The port number that this server uses.")]
		[DefaultValue(123)]
		public int Port
		{
			get
			{
				return _Port;
			}
			set
			{
				if (value >= 0 && value <= 65535)
				{
					_Port = value;
				}
				else
				{
					_Port = 123;
				}
			}
		}

		public RemoteSNTPServer(string hostNameOrAddress, int port)
		{
			HostNameOrAddress = hostNameOrAddress;
			Port = port;
		}

		public RemoteSNTPServer(string hostNameOrAddress)
			: this(hostNameOrAddress, 123)
		{
		}

		public RemoteSNTPServer()
			: this(TimeServerList[0].HostNameOrAddress, 123)
		{
		}

		public IPEndPoint GetIPEndPoint()
		{
			return new IPEndPoint(Dns.GetHostAddresses(HostNameOrAddress)[0], Port);
		}

		public override string ToString()
		{
			return string.Format("{0}:{1}", HostNameOrAddress, Port);
		}
	}
}
