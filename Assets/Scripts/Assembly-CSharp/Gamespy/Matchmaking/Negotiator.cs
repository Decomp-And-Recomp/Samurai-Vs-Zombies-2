using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Gamespy.Authentication;

namespace Gamespy.Matchmaking
{
	public class Negotiator
	{
		public struct MappingIP
		{
			public IPAddress privateIPAddress;

			public int privateIp;

			public int privatePort;

			public IPAddress publicIPAddress;

			public int publicIp;

			public int publicPort;
		}

		public class NATProperties
		{
			public bool ipRestricted;

			public bool portRestricted;

			public NATType natType;

			public NATPromiscuity promiscuity;

			public NATMappingScheme mappingScheme;

			public MappingIP[] mappingIP;

			public NATProperties()
			{
				ipRestricted = true;
				portRestricted = true;
				natType = NATType.unknown;
				promiscuity = NATPromiscuity.promiscuity_not_applicable;
				mappingScheme = NATMappingScheme.unrecognized;
				mappingIP = new MappingIP[4];
			}
		}

		private const byte NATNEG_MAGIC_LEN = 6;

		private const byte NN_MAGIC_0 = 253;

		private const byte NN_MAGIC_1 = 252;

		private const byte NN_MAGIC_2 = 30;

		private const byte NN_MAGIC_3 = 102;

		private const byte NN_MAGIC_4 = 106;

		private const byte NN_MAGIC_5 = 178;

		private const byte NAT_VERSION = 4;

		private const byte NN_INIT = 0;

		private const byte NN_INITACK = 1;

		private const byte NN_ERTTEST = 2;

		private const byte NN_ERTACK = 3;

		private const byte NN_STATEUPDATE = 4;

		private const byte NN_CONNECT = 5;

		private const byte NN_CONNECT_ACK = 6;

		private const byte NN_CONNECT_PING = 7;

		private const byte NN_BACKUP_TEST = 8;

		private const byte NN_BACKUP_ACK = 9;

		private const byte NN_ADDRESS_CHECK = 10;

		private const byte NN_ADDRESS_REPLY = 11;

		private const byte NN_NATIFY_REQUEST = 12;

		private const byte NN_REPORT = 13;

		private const byte NN_REPORT_ACK = 14;

		private const byte NN_PREINIT = 15;

		private const byte NN_PREINIT_ACK = 16;

		private const int NAT_DETECT_COOKIE = 777;

		private const int NAT_DETECT_TIMEOUT = 100000000;

		private const byte NN_PT_GP = 0;

		private const byte NN_PT_NN1 = 1;

		private const byte NN_PT_NN2 = 2;

		private const byte NN_PT_NN3 = 3;

		private const int packet_map1a = 0;

		private const int packet_map2 = 1;

		private const int packet_map3 = 2;

		private const int packet_map1b = 3;

		private const int NUM_PACKETS = 4;

		private string _gameName;

		private long startTime;

		private int _negotiatorPort = 27901;

		private int _natAddrCount;

		private IPAddress[] _negotiatorAddr = new IPAddress[3];

		private IPEndPoint[] _negotiatorEndpoint = new IPEndPoint[3];

		private bool gotERT1;

		private bool gotERT2;

		private bool gotERT3;

		private bool gotAddress1a;

		private bool gotAddress1b;

		private bool gotAddress2;

		private bool gotAddress3;

		private UdpClient ert;

		private UdpClient mapping;

		private Socket sockLocalIP;

		private int localIpNetworkByteOrder;

		private NATDetectionState _natDetectionState;

		private NATDetectionResult _result;

		private NATProperties _natInfo = new NATProperties();

		public NATDetectionResult Result
		{
			get
			{
				return _result;
			}
		}

		public NATProperties NATInfo
		{
			get
			{
				return _natInfo;
			}
		}

		public Negotiator(Account.AuthSecurityToken securityToken)
		{
			_gameName = securityToken.GameName;
		}

		private void ErtReceiveCallback(IAsyncResult res)
		{
			byte[] array = null;
			byte[] array2 = new byte[6];
			for (int i = 0; i < 3; i++)
			{
				array = ert.EndReceive(res, ref _negotiatorEndpoint[i]);
				if (array != null)
				{
					break;
				}
			}
			MemoryStream input = new MemoryStream(array);
			BinaryReader binaryReader = new BinaryReader(input);
			binaryReader.Read(array2, 0, 6);
			if (array2[0] == 253 && array2[1] == 252 && array2[2] == 30 && array2[3] == 102 && array2[4] == 106 && array2[5] == 178)
			{
				binaryReader.ReadByte();
				binaryReader.ReadByte();
				binaryReader.ReadInt32();
				switch (binaryReader.ReadByte())
				{
				case 1:
					gotERT1 = true;
					break;
				case 2:
					_natInfo.ipRestricted = false;
					gotERT2 = true;
					break;
				case 3:
					_natInfo.portRestricted = false;
					gotERT3 = true;
					break;
				}
			}
			ert.BeginReceive(ErtReceiveCallback, null);
		}

		private void MappingReceiveCallback(IAsyncResult res)
		{
			byte[] array = null;
			byte[] array2 = new byte[6];
			for (int i = 0; i < 3; i++)
			{
				array = mapping.EndReceive(res, ref _negotiatorEndpoint[i]);
				if (array != null)
				{
					break;
				}
			}
			MemoryStream input = new MemoryStream(array);
			BinaryReader binaryReader = new BinaryReader(input);
			binaryReader.Read(array2, 0, 6);
			if (array2[0] == 253 && array2[1] == 252 && array2[2] == 30 && array2[3] == 102 && array2[4] == 106 && array2[5] == 178)
			{
				binaryReader.ReadByte();
				binaryReader.ReadByte();
				int num = IPAddress.NetworkToHostOrder(binaryReader.ReadInt32());
				binaryReader.ReadByte();
				binaryReader.ReadByte();
				binaryReader.ReadByte();
				switch (num)
				{
				case 0:
					gotAddress1a = true;
					break;
				case 3:
					gotAddress1b = true;
					break;
				case 1:
					gotAddress2 = true;
					break;
				case 2:
					gotAddress3 = true;
					break;
				}
				int num2 = localIpNetworkByteOrder;
				_natInfo.mappingIP[num].privateIPAddress = new IPAddress(num2);
				_natInfo.mappingIP[num].privateIp = IPAddress.NetworkToHostOrder(num2);
				_natInfo.mappingIP[num].privatePort = ((IPEndPoint)mapping.Client.LocalEndPoint).Port;
				int num3 = binaryReader.ReadInt32();
				_natInfo.mappingIP[num].publicIPAddress = new IPAddress(num3);
				_natInfo.mappingIP[num].publicIp = IPAddress.NetworkToHostOrder(num3);
				int num4 = binaryReader.ReadUInt16();
				int num5 = (num4 << 8) & 0xFF00;
				int num6 = (num4 >> 8) & 0xFF;
				num4 = num5 | num6;
				_natInfo.mappingIP[num].publicPort = num4;
			}
			mapping.BeginReceive(MappingReceiveCallback, null);
		}

		private void InitializeSocket()
		{
			startTime = DateTime.Now.Ticks;
			gotERT1 = false;
			gotERT2 = false;
			gotERT3 = false;
			gotAddress1a = false;
			gotAddress1b = false;
			gotAddress2 = false;
			gotAddress3 = false;
			for (int i = 0; i < 3; i++)
			{
				_negotiatorEndpoint[i] = new IPEndPoint(_negotiatorAddr[i], _negotiatorPort);
			}
			ert = new UdpClient();
			ert.BeginReceive(ErtReceiveCallback, null);
			mapping = new UdpClient();
			mapping.BeginReceive(MappingReceiveCallback, null);
		}

		private void GetHostAddressesForHosting(IAsyncResult result)
		{
			try
			{
				IPAddress[] array = Dns.EndGetHostAddresses(result);
				_negotiatorAddr[_natAddrCount] = array[0];
				_natAddrCount++;
				if (_natAddrCount == 3)
				{
					_natAddrCount = 0;
					_natDetectionState = NATDetectionState.BeginLocalIPConnect;
				}
			}
			catch (Exception)
			{
				_result = NATDetectionResult.DnsException;
				_natDetectionState = NATDetectionState.CompleteWithError;
			}
		}

		private void ResolveNATDetectionEndpoints()
		{
			Dns.BeginGetHostAddresses(_gameName + ".natneg1.gamespy.com", GetHostAddressesForHosting, null);
			Dns.BeginGetHostAddresses(_gameName + ".natneg2.gamespy.com", GetHostAddressesForHosting, null);
			Dns.BeginGetHostAddresses(_gameName + ".natneg3.gamespy.com", GetHostAddressesForHosting, null);
		}

		private void GetLocalHostAddressConnectCallback(IAsyncResult ar)
		{
			try
			{
				Socket socket = (Socket)ar.AsyncState;
				socket.EndConnect(ar);
				_natDetectionState = NATDetectionState.BeginLocalIPSend;
			}
			catch (Exception)
			{
				_result = NATDetectionResult.ResolveLocalHostAddressException;
				_natDetectionState = NATDetectionState.CompleteWithError;
			}
		}

		private void ResolveLocalHostEndpointConnect(Socket client)
		{
			client.BeginConnect("www.google.com", 80, GetLocalHostAddressConnectCallback, client);
		}

		private void GetLocalHostAddressSendCallback(IAsyncResult ar)
		{
			try
			{
				Socket socket = (Socket)ar.AsyncState;
				socket.EndSend(ar);
				_natDetectionState = NATDetectionState.EndLocalIP;
			}
			catch (Exception)
			{
				_result = NATDetectionResult.ResolveLocalHostAddressException;
				_natDetectionState = NATDetectionState.CompleteWithError;
			}
		}

		private void ResolveLocalHostEndpointSend(Socket client)
		{
			byte[] buffer = new byte[1] { 1 };
			client.BeginSend(buffer, 0, 1, SocketFlags.None, GetLocalHostAddressSendCallback, client);
		}

		private void ResolveLocalHostEndpointEnd(Socket client)
		{
			byte[] addressBytes = IPAddress.Parse(((IPEndPoint)client.LocalEndPoint).Address.ToString()).GetAddressBytes();
			localIpNetworkByteOrder = 0;
			localIpNetworkByteOrder = addressBytes[3] << 24;
			localIpNetworkByteOrder |= addressBytes[2] << 16;
			localIpNetworkByteOrder |= addressBytes[1] << 8;
			localIpNetworkByteOrder |= addressBytes[0];
			client.Close();
		}

		private void SendReachabilityPacket(byte natPortType)
		{
			MemoryStream output = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(output);
			binaryWriter.Write((byte)253);
			binaryWriter.Write((byte)252);
			binaryWriter.Write((byte)30);
			binaryWriter.Write((byte)102);
			binaryWriter.Write((byte)106);
			binaryWriter.Write((byte)178);
			binaryWriter.Write((byte)4);
			binaryWriter.Write((byte)12);
			binaryWriter.Write(IPAddress.HostToNetworkOrder(777));
			binaryWriter.Write(natPortType);
			binaryWriter.Seek(0, SeekOrigin.Begin);
			byte[] array = new byte[(int)binaryWriter.BaseStream.Length + 60];
			binaryWriter.BaseStream.Read(array, 0, (int)binaryWriter.BaseStream.Length);
			if (natPortType == 1 || natPortType == 2)
			{
				ert.Send(array, (int)binaryWriter.BaseStream.Length + 60, _negotiatorEndpoint[0]);
			}
			else
			{
				ert.Send(array, (int)binaryWriter.BaseStream.Length + 60, _negotiatorEndpoint[1]);
			}
		}

		private void SendMappingPacket(byte natPortType, int id)
		{
			MemoryStream output = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(output);
			binaryWriter.Write((byte)253);
			binaryWriter.Write((byte)252);
			binaryWriter.Write((byte)30);
			binaryWriter.Write((byte)102);
			binaryWriter.Write((byte)106);
			binaryWriter.Write((byte)178);
			binaryWriter.Write((byte)4);
			binaryWriter.Write((byte)10);
			binaryWriter.Write(IPAddress.HostToNetworkOrder(id));
			binaryWriter.Write(natPortType);
			binaryWriter.Seek(0, SeekOrigin.Begin);
			byte[] array = new byte[(int)binaryWriter.BaseStream.Length + 60];
			binaryWriter.BaseStream.Read(array, 0, (int)binaryWriter.BaseStream.Length);
			switch (id)
			{
			case 0:
			case 3:
				mapping.Send(array, (int)binaryWriter.BaseStream.Length + 60, _negotiatorEndpoint[0]);
				break;
			case 1:
				mapping.Send(array, (int)binaryWriter.BaseStream.Length + 60, _negotiatorEndpoint[1]);
				break;
			default:
				mapping.Send(array, (int)binaryWriter.BaseStream.Length + 60, _negotiatorEndpoint[2]);
				break;
			}
		}

		private void SendDetectionPackets()
		{
			SendReachabilityPacket(1);
			SendReachabilityPacket(2);
			SendReachabilityPacket(3);
			SendMappingPacket(1, 0);
			SendMappingPacket(1, 3);
			SendMappingPacket(2, 1);
			SendMappingPacket(3, 2);
		}

		private void CheckDetectionStatus()
		{
			long ticks = DateTime.Now.Ticks;
			if (ticks - startTime > 100000000)
			{
				_natDetectionState = NATDetectionState.DetermineNATConfiguration;
			}
			if (gotERT1 && gotERT2 && gotERT3 && gotAddress1a && gotAddress1b && gotAddress2 && gotAddress3)
			{
				_natDetectionState = NATDetectionState.DetermineNATConfiguration;
			}
		}

		private void DetermineNATConfiguration()
		{
			if (_natInfo.mappingIP[0].publicIp == 0 || _natInfo.mappingIP[1].publicIp == 0 || _natInfo.mappingIP[2].publicIp == 0)
			{
				_natDetectionState = NATDetectionState.CompleteWithError;
				_result = NATDetectionResult.DetectionFailed;
				return;
			}
			if (!_natInfo.portRestricted && !_natInfo.ipRestricted && _natInfo.mappingIP[0].privateIPAddress.ToString() == "127.0.0.1")
			{
				_natInfo.natType = NATType.no_nat;
			}
			else if (_natInfo.mappingIP[0].privateIPAddress.ToString() == "127.0.0.1")
			{
				_natInfo.natType = NATType.firewall_only;
			}
			else if (!_natInfo.ipRestricted && !_natInfo.portRestricted && Math.Abs(_natInfo.mappingIP[2].publicPort - _natInfo.mappingIP[1].publicPort) >= 1)
			{
				_natInfo.natType = NATType.symmetric;
				_natInfo.promiscuity = NATPromiscuity.promiscuous;
			}
			else if (_natInfo.ipRestricted && !_natInfo.portRestricted && Math.Abs(_natInfo.mappingIP[2].publicPort - _natInfo.mappingIP[1].publicPort) >= 1)
			{
				_natInfo.natType = NATType.symmetric;
				_natInfo.promiscuity = NATPromiscuity.port_promiscuous;
			}
			else if (!_natInfo.ipRestricted && _natInfo.portRestricted && Math.Abs(_natInfo.mappingIP[2].publicPort - _natInfo.mappingIP[1].publicPort) >= 1)
			{
				_natInfo.natType = NATType.symmetric;
				_natInfo.promiscuity = NATPromiscuity.ip_promiscuous;
			}
			else if (_natInfo.ipRestricted && _natInfo.portRestricted && Math.Abs(_natInfo.mappingIP[2].publicPort - _natInfo.mappingIP[1].publicPort) >= 1)
			{
				_natInfo.natType = NATType.symmetric;
				_natInfo.promiscuity = NATPromiscuity.not_promiscuous;
			}
			else if (_natInfo.portRestricted)
			{
				_natInfo.natType = NATType.port_restricted_cone;
			}
			else if (_natInfo.ipRestricted && !_natInfo.portRestricted)
			{
				_natInfo.natType = NATType.restricted_cone;
			}
			else if (!_natInfo.ipRestricted && !_natInfo.portRestricted)
			{
				_natInfo.natType = NATType.full_cone;
			}
			else
			{
				_natInfo.natType = NATType.unknown;
			}
			if (_natInfo.mappingIP[0].publicPort == _natInfo.mappingIP[0].privatePort && _natInfo.mappingIP[1].publicPort == _natInfo.mappingIP[1].privatePort && _natInfo.mappingIP[2].publicPort == _natInfo.mappingIP[2].privatePort)
			{
				_natInfo.mappingScheme = NATMappingScheme.private_as_public;
			}
			else if (_natInfo.mappingIP[0].publicPort == _natInfo.mappingIP[1].publicPort && _natInfo.mappingIP[1].publicPort == _natInfo.mappingIP[2].publicPort)
			{
				_natInfo.mappingScheme = NATMappingScheme.consistent_port;
			}
			else if (_natInfo.mappingIP[0].publicPort == _natInfo.mappingIP[0].privatePort && _natInfo.mappingIP[2].publicPort - _natInfo.mappingIP[1].publicPort == 1)
			{
				_natInfo.mappingScheme = NATMappingScheme.mixed;
			}
			else if (_natInfo.mappingIP[2].publicPort - _natInfo.mappingIP[1].publicPort == 1)
			{
				_natInfo.mappingScheme = NATMappingScheme.incremental;
			}
			else
			{
				_natInfo.mappingScheme = NATMappingScheme.unrecognized;
			}
			_natDetectionState = NATDetectionState.Complete;
		}

		public NATDetectionState DetectMyNAT()
		{
			switch (_natDetectionState)
			{
			case NATDetectionState.Begin:
				_result = NATDetectionResult.Success;
				_natDetectionState = NATDetectionState.DnsPending;
				ResolveNATDetectionEndpoints();
				break;
			case NATDetectionState.BeginLocalIPConnect:
				_natDetectionState = NATDetectionState.LocalIPPending;
				sockLocalIP = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				ResolveLocalHostEndpointConnect(sockLocalIP);
				break;
			case NATDetectionState.BeginLocalIPSend:
				_natDetectionState = NATDetectionState.LocalIPPending;
				ResolveLocalHostEndpointSend(sockLocalIP);
				break;
			case NATDetectionState.EndLocalIP:
				_natDetectionState = NATDetectionState.InitializeSocket;
				ResolveLocalHostEndpointEnd(sockLocalIP);
				break;
			case NATDetectionState.InitializeSocket:
				_natDetectionState = NATDetectionState.StartSendingDetectionPackets;
				InitializeSocket();
				break;
			case NATDetectionState.StartSendingDetectionPackets:
				_natDetectionState = NATDetectionState.DetectionResponsePending;
				SendDetectionPackets();
				break;
			case NATDetectionState.DetectionResponsePending:
				CheckDetectionStatus();
				break;
			case NATDetectionState.DetermineNATConfiguration:
				DetermineNATConfiguration();
				break;
			case NATDetectionState.CompleteWithError:
				_natDetectionState = NATDetectionState.Complete;
				break;
			case NATDetectionState.Complete:
				_natDetectionState = NATDetectionState.InitializeSocket;
				_result = NATDetectionResult.Success;
				break;
			}
			return _natDetectionState;
		}
	}
}
