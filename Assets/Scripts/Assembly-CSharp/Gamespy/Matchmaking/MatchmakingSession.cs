using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Gamespy.Authentication;
using Gamespy.Common;

namespace Gamespy.Matchmaking
{
	public class MatchmakingSession
	{
		private enum ClientMessageReceiveState
		{
			MessageNotYetReceived = 0,
			MessageReceivedInCallback = 1,
			MessagePassedToHost = 2
		}

		private class GOACryptState
		{
			public short[] cards = new short[256];

			public short rotor;

			public short ratchet;

			public short avalanche;

			public short last_plain;

			public short last_cipher;

			public bool initialized;

			public GOACryptState()
			{
				initialized = false;
			}
		}

		public class StateObject
		{
			public const int BufferSize = 1400;

			public Socket workSocket;

			public byte[] buffer = new byte[1400];
		}

		private const byte MAGIC_BYTE1 = 254;

		private const byte MAGIC_BYTE2 = 253;

		private const byte PACKET_QUERY = 0;

		private const byte PACKET_CHALLENGE = 1;

		private const byte PACKET_ECHO = 2;

		private const byte PACKET_ECHO_RESPONSE = 5;

		private const byte PACKET_HEARTBEAT = 3;

		private const byte PACKET_ADDERROR = 4;

		private const byte PACKET_CLIENT_MESSAGE = 6;

		private const byte PACKET_CLIENT_MESSAGE_ACK = 7;

		private const byte PACKET_KEEPALIVE = 8;

		private const byte PACKET_PREQUERY_IP_VERIFY = 9;

		private const byte PACKET_CLIENT_REGISTERED = 10;

		private const long FIRST_HB_TIME = 100000000L;

		private const long HB_TIME = 600000000L;

		private const long KA_TIME = 200000000L;

		private const long MIN_STATECHANGED_HB_TIME = 50000000L;

		private const int MAX_FIRST_COUNT = 4;

		private const int MAX_DATA_SIZE = 1400;

		private const int INBUF_LEN = 256;

		private const int PUBLIC_ADDR_LEN = 12;

		private const int QR2_OPTION_USE_QUERY_CHALLENGE = 128;

		private const int INSTANCE_KEY_LENGTH = 4;

		private const int MAX_CHALLENGE = 21;

		private const int MAX_CHALLENGE_RESPONSE = 64;

		private const int STATECHANGE_HEARTBEAT = 0;

		private const int STATECHANGE_UPDATE = 1;

		private const int STATECHANGE_SHUTDOWN = 2;

		private const int STATECHANGE_FIRST_HEARTBEAT = 3;

		private const int CLIENT_REQUEST_KEY_LENGTH = 4;

		private const int CLIENT_MESSAGE_HEADER_LENGTH = 11;

		private string _gameName;

		private string _secretKey;

		private int _masterServerHostingPort = 27900;

		private IPAddress _masterServerHostingAddr;

		private IPEndPoint _masterServerHostingEndpoint;

		private UdpClient u;

		private Socket sockLocalIP;

		private string _localIP;

		private byte[] sendBuf = new byte[1400];

		private HostRequestState _hostRequestState;

		private int _natnegotiate;

		private bool _userChangeState;

		private bool _hostUpdateLogged;

		private bool _listedState;

		private long _lastHeartbeat;

		private long _lastKeepAlive;

		private byte[] _instanceKey = new byte[4];

		private List<Key> _lastKeyList;

		private ClientMessageReceiveState _clientMessageReceiveState;

		private MatchmakingRequestResult _result;

		private string _resultMessage;

		private string _host_ClientMessage;

		private bool searchInProgress;

		private bool subscribeInProgress;

		private bool updateReturned;

		private bool hostListUpdated;

		private string[] pushKeyNames;

		private short defaultPort;

		private IAsyncResult pushUpdateSocketResult;

		private int _masterServerSBPort = 28910;

		private IPAddress _masterServerSBAddr;

		private IPEndPoint _masterServerSBEndpoint;

		private Socket s;

		private Socket sm;

		private SearchRequestState _searchRequestState;

		private SendMessageRequestState _sendMessageRequestState;

		private int _sendMessageReceived;

		private List<GameHost> _search_GameHosts = new List<GameHost>();

		private string _randomChallenge = "kukDX7VK";

		private short[] cards_ascending = new short[256]
		{
			0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
			10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
			20, 21, 22, 23, 24, 25, 26, 27, 28, 29,
			30, 31, 32, 33, 34, 35, 36, 37, 38, 39,
			40, 41, 42, 43, 44, 45, 46, 47, 48, 49,
			50, 51, 52, 53, 54, 55, 56, 57, 58, 59,
			60, 61, 62, 63, 64, 65, 66, 67, 68, 69,
			70, 71, 72, 73, 74, 75, 76, 77, 78, 79,
			80, 81, 82, 83, 84, 85, 86, 87, 88, 89,
			90, 91, 92, 93, 94, 95, 96, 97, 98, 99,
			100, 101, 102, 103, 104, 105, 106, 107, 108, 109,
			110, 111, 112, 113, 114, 115, 116, 117, 118, 119,
			120, 121, 122, 123, 124, 125, 126, 127, 128, 129,
			130, 131, 132, 133, 134, 135, 136, 137, 138, 139,
			140, 141, 142, 143, 144, 145, 146, 147, 148, 149,
			150, 151, 152, 153, 154, 155, 156, 157, 158, 159,
			160, 161, 162, 163, 164, 165, 166, 167, 168, 169,
			170, 171, 172, 173, 174, 175, 176, 177, 178, 179,
			180, 181, 182, 183, 184, 185, 186, 187, 188, 189,
			190, 191, 192, 193, 194, 195, 196, 197, 198, 199,
			200, 201, 202, 203, 204, 205, 206, 207, 208, 209,
			210, 211, 212, 213, 214, 215, 216, 217, 218, 219,
			220, 221, 222, 223, 224, 225, 226, 227, 228, 229,
			230, 231, 232, 233, 234, 235, 236, 237, 238, 239,
			240, 241, 242, 243, 244, 245, 246, 247, 248, 249,
			250, 251, 252, 253, 254, 255
		};

		private GOACryptState gcs;

		private long pollingStartTime;

		public static MemoryStream outStream;

		public BinaryWriter decryptedFullBuf;

		public MatchmakingRequestResult Result
		{
			get
			{
				return _result;
			}
		}

		public string ResultMessage
		{
			get
			{
				return _resultMessage;
			}
		}

		public string Host_ClientMessage
		{
			get
			{
				return _host_ClientMessage;
			}
		}

		public List<GameHost> Search_GameHosts
		{
			get
			{
				return _search_GameHosts;
			}
		}

		public MatchmakingSession(Account.AuthSecurityToken securityToken)
		{
			if (string.IsNullOrEmpty(securityToken.GameName))
			{
				_result = MatchmakingRequestResult.ConstructorError;
				_resultMessage = "AuthSecurityToken was not populated - ensure Authenticate is successful before constructing a Matchmaking Session";
				_hostRequestState = HostRequestState.CompleteWithError;
				_searchRequestState = SearchRequestState.CompleteWithError;
			}
			else
			{
				_gameName = securityToken.GameName;
				_secretKey = securityToken.SecretKey;
			}
		}

		private void gs_encrypt(byte[] secretKey, int secretKeyLength, byte[] encrypted_value, int length)
		{
			byte[] array = new byte[256];
			for (short num = 0; num < 256; num++)
			{
				array[num] = (byte)num;
			}
			byte b = 0;
			byte b2 = 0;
			for (short num = 0; num < 256; num++)
			{
				b2 = (byte)((secretKey[b] + array[num] + b2) % 256);
				b = (byte)((b + 1) % secretKeyLength);
				byte b3 = array[num];
				array[num] = array[b2];
				array[b2] = b3;
			}
			b = 0;
			b2 = 0;
			for (short num = 0; num < length; num++)
			{
				b = (byte)((b + encrypted_value[num] + 1) % 256);
				b2 = (byte)((array[b] + b2) % 256);
				byte b3 = array[b];
				array[b] = array[b2];
				array[b2] = b3;
				byte b4 = (byte)((array[b] + array[b2]) % 256);
				encrypted_value[num] ^= array[b4];
			}
		}

		private byte encode_ct(byte c)
		{
			if (c < 26)
			{
				return (byte)(65 + c);
			}
			if (c < 52)
			{
				return (byte)(97 + c - 26);
			}
			if (c < 62)
			{
				return (byte)(48 + c - 52);
			}
			switch (c)
			{
			case 62:
				return 43;
			case 63:
				return 47;
			default:
				return 0;
			}
		}

		private int gs_encode(byte[] encrypted_value, int challenge_length, int index)
		{
			byte[] array = new byte[3];
			byte[] array2 = new byte[4];
			int num = 0;
			int num2 = 0;
			while (num < challenge_length)
			{
				int num3 = 0;
				while (num3 <= 2)
				{
					if (num < challenge_length)
					{
						array[num3] = encrypted_value[num2++];
					}
					else
					{
						array[num3] = 0;
					}
					num3++;
					num++;
				}
				array2[0] = (byte)(array[0] >> 2);
				array2[1] = (byte)(((array[0] & 3) << 4) + (array[1] >> 4));
				array2[2] = (byte)(((array[1] & 0xF) << 2) + (array[2] >> 6));
				array2[3] = (byte)(array[2] & 0x3Fu);
				for (num3 = 0; num3 <= 3; num3++)
				{
					sendBuf[index++] = encode_ct(array2[num3]);
				}
			}
			sendBuf[index++] = 0;
			return index;
		}

		private void HostingReceiveCallback(IAsyncResult res)
		{
			byte[] array = u.EndReceive(res, ref _masterServerHostingEndpoint);
			if (array[0] == 254 && array[1] == 253)
			{
				int num = 0;
				byte b = array[2];
				byte[] array2 = new byte[4];
				for (int i = 0; i < 4; i++)
				{
					array2[i] = array[i + 3];
				}
				for (int i = 0; i < 4; i++)
				{
					if (_instanceKey[i] != array2[i])
					{
						u.BeginReceive(HostingReceiveCallback, null);
						return;
					}
				}
				switch (b)
				{
				case 1:
				{
					byte[] array3 = new byte[21];
					byte[] array4 = new byte[65];
					Array.Clear(sendBuf, 0, 1400);
					sendBuf[num++] = 1;
					Array.Copy(array2, 0, sendBuf, num, 4);
					num += 4;
					Array.Copy(array, 7, array3, 0, 21);
					Array.Copy(array3, array4, 21);
					byte[] bytes = Encoding.UTF8.GetBytes(_secretKey);
					gs_encrypt(bytes, _secretKey.Length, array4, 20);
					num = gs_encode(array4, 20, num);
					u.Send(sendBuf, num, _masterServerHostingEndpoint);
					break;
				}
				case 6:
					Array.Clear(sendBuf, 0, 1400);
					sendBuf[num++] = 7;
					Array.Copy(array2, 0, sendBuf, num, 4);
					num += 4;
					Array.Copy(array, 7, sendBuf, num, 4);
					num += 4;
					_host_ClientMessage = Encoding.ASCII.GetString(array, 11, array.Length - 11);
					_hostRequestState = HostRequestState.ClientMessageReceived;
					_clientMessageReceiveState = ClientMessageReceiveState.MessageReceivedInCallback;
					u.Send(sendBuf, num, _masterServerHostingEndpoint);
					break;
				case 10:
					_listedState = true;
					_hostRequestState = HostRequestState.Listed;
					break;
				}
			}
			u.BeginReceive(HostingReceiveCallback, null);
		}

		private void InitializeSocket()
		{
			_natnegotiate = 1;
			_userChangeState = false;
			_hostUpdateLogged = false;
			_listedState = false;
			_lastHeartbeat = 0L;
			_lastKeepAlive = 0L;
			Array.Clear(_instanceKey, 0, 4);
			_masterServerHostingEndpoint = new IPEndPoint(_masterServerHostingAddr, _masterServerHostingPort);
			u = new UdpClient(0);
			u.BeginReceive(HostingReceiveCallback, null);
		}

		private void GetHostAddressesForHosting(IAsyncResult result)
		{
			try
			{
				IPAddress[] array = Dns.EndGetHostAddresses(result);
				_masterServerHostingAddr = array[0];
				_hostRequestState = HostRequestState.BeginLocalIPConnect;
			}
			catch (Exception)
			{
				_result = MatchmakingRequestResult.DnsException;
				_hostRequestState = HostRequestState.CompleteWithError;
			}
		}

		private void ResolveMasterServerHostingEndpoint()
		{
			Dns.BeginGetHostAddresses(_gameName + ".master.gamespy.com", GetHostAddressesForHosting, null);
		}

		private void GetLocalHostAddressConnectCallback(IAsyncResult ar)
		{
			try
			{
				Socket socket = (Socket)ar.AsyncState;
				socket.EndConnect(ar);
				_hostRequestState = HostRequestState.BeginLocalIPSend;
			}
			catch (Exception)
			{
				_result = MatchmakingRequestResult.ResolveLocalHostAddressException;
				_hostRequestState = HostRequestState.CompleteWithError;
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
				_hostRequestState = HostRequestState.EndLocalIP;
			}
			catch (Exception)
			{
				_result = MatchmakingRequestResult.ResolveLocalHostAddressException;
				_hostRequestState = HostRequestState.CompleteWithError;
			}
		}

		private void ResolveLocalHostEndpointSend(Socket client)
		{
			byte[] buffer = new byte[1] { 1 };
			client.BeginSend(buffer, 0, 1, SocketFlags.None, GetLocalHostAddressSendCallback, client);
		}

		private void ResolveLocalHostEndpointEnd(Socket client)
		{
			_localIP = ((IPEndPoint)client.LocalEndPoint).Address.ToString();
			client.Close();
		}

		private string stringToString(string s)
		{
			return s ?? "null";
		}

		private int AppendKeyValuePair(string key, string keyValue, int index)
		{
			string value = key;
			if (string.IsNullOrEmpty(value))
			{
				value = index.ToString();
			}
			if (string.IsNullOrEmpty(keyValue))
			{
				value = index.ToString();
			}
			byte[] array = new byte[1];
			byte[] bytes = Encoding.UTF8.GetBytes(key);
			Array.Copy(bytes, 0, sendBuf, index, bytes.Length);
			index += bytes.Length;
			Array.Copy(array, 0, sendBuf, index, array.Length);
			index += array.Length;
			bytes = Encoding.UTF8.GetBytes(keyValue);
			Array.Copy(bytes, 0, sendBuf, index, bytes.Length);
			index += bytes.Length;
			Array.Copy(array, 0, sendBuf, index, array.Length);
			index += array.Length;
			return index;
		}

		private void SendHeartbeat(List<Key> keysToList, int statechanged)
		{
			int index = 0;
			Random random = new Random();
			Array.Clear(sendBuf, 0, 1400);
			sendBuf[index++] = 3;
			if (statechanged == 3)
			{
				for (int i = 0; i < 4; i++)
				{
					byte b = (byte)random.Next();
					_instanceKey[i] = b;
				}
			}
			for (int i = 0; i < 4; i++)
			{
				sendBuf[index++] = _instanceKey[i];
			}
			string key = "localip0";
			string localIP = _localIP;
			index = AppendKeyValuePair(key, localIP, index);
			key = "localport";
			localIP = ((IPEndPoint)u.Client.LocalEndPoint).Port.ToString();
			index = AppendKeyValuePair(key, localIP, index);
			key = "natneg";
			localIP = _natnegotiate.ToString();
			index = AppendKeyValuePair(key, localIP, index);
			if (statechanged != 0)
			{
				key = "statechanged";
				localIP = statechanged.ToString();
				index = AppendKeyValuePair(key, localIP, index);
			}
			key = "gamename";
			localIP = _gameName;
			index = AppendKeyValuePair(key, localIP, index);
			if (statechanged != 2)
			{
				for (int i = 0; i < keysToList.Count; i++)
				{
					key = keysToList[i].KeyName;
					localIP = keysToList[i].KeyValue;
					index = AppendKeyValuePair(key, localIP, index);
				}
			}
			u.Send(sendBuf, index, _masterServerHostingEndpoint);
			_lastKeepAlive = (_lastHeartbeat = DateTime.Now.Ticks);
			if (statechanged != 0)
			{
				_userChangeState = false;
				_hostUpdateLogged = false;
			}
		}

		private void SendKeepAlive()
		{
			int bytes = 0;
			Array.Clear(sendBuf, 0, 1400);
			sendBuf[bytes++] = 8;
			for (int i = 0; i < 4; i++)
			{
				sendBuf[bytes++] = _instanceKey[i];
			}
			u.Send(sendBuf, bytes, _masterServerHostingEndpoint);
			_lastKeepAlive = DateTime.Now.Ticks;
		}

		private bool DidKeyListChange(List<Key> keyList)
		{
			for (int i = 0; i < keyList.Count; i++)
			{
				if (_lastKeyList[i].KeyValue != keyList[i].KeyValue)
				{
					return true;
				}
			}
			return false;
		}

		private void CheckSendHeartbeat(List<Key> keysToList)
		{
			if (DidKeyListChange(keysToList))
			{
				_userChangeState = true;
			}
			long ticks = DateTime.Now.Ticks;
			if (!_listedState)
			{
				if (ticks - _lastHeartbeat > 100000000)
				{
					SendHeartbeat(keysToList, 3);
				}
			}
			else if (_userChangeState)
			{
				if (ticks - _lastHeartbeat > 50000000)
				{
					_lastKeyList = CopyKeysList(keysToList);
					SendHeartbeat(keysToList, 1);
				}
			}
			else if (ticks - _lastHeartbeat > 600000000)
			{
				SendHeartbeat(keysToList, 0);
			}
			if (ticks - _lastKeepAlive > 200000000)
			{
				SendKeepAlive();
			}
		}

		private List<Key> CopyKeysList(List<Key> keyList)
		{
			List<Key> list = new List<Key>();
			for (int i = 0; i < keyList.Count; i++)
			{
				Key key = new Key();
				key.KeyName = keyList[i].KeyName;
				key.KeyValue = keyList[i].KeyValue;
				list.Add(key);
			}
			return list;
		}

		private GOACryptState GOACryptStateInit(byte[] key, byte keysize)
		{
			Array.Copy(cards_ascending, gcs.cards, 256);
			short num = 0;
			int num2 = 0;
			short num3 = 0;
			int num4 = 255;
			for (int num5 = 255; num5 > 0; num5--)
			{
				int num6 = num5;
				if (num6 == 0)
				{
					return null;
				}
				int num7 = 0;
				int num9;
				do
				{
					short num8 = 0;
					num8 |= (short)(key[num2] & 0xFF);
					try
					{
						num3 = (short)((gcs.cards[num3] + num8) & 0xFF);
					}
					catch (Exception)
					{
						return null;
					}
					num2++;
					if (num2 >= keysize)
					{
						num2 = 0;
						num3 = (short)((num3 + keysize) & 0xFF);
					}
					num9 = num4 & num3;
					num7++;
					if (num7 > 11)
					{
						num9 %= num6;
					}
				}
				while (num9 > num6);
				num = (short)num9;
				short num10 = gcs.cards[num5];
				gcs.cards[num5] = gcs.cards[num];
				gcs.cards[num] = num10;
				if ((num5 & (num5 - 1)) == 0)
				{
					num4 >>= 1;
				}
			}
			gcs.rotor = gcs.cards[1];
			gcs.ratchet = gcs.cards[3];
			gcs.avalanche = gcs.cards[5];
			gcs.last_plain = gcs.cards[7];
			gcs.last_cipher = gcs.cards[num3];
			gcs.initialized = true;
			return gcs;
		}

		private GOACryptState InitCryptKey(byte[] sbResponse, byte keyOffset, byte keyLength)
		{
			try
			{
				string secretKey = _secretKey;
				string randomChallenge = _randomChallenge;
				int length = secretKey.Length;
				byte[] bytes = Encoding.ASCII.GetBytes(secretKey);
				byte b = (byte)randomChallenge.Length;
				byte[] bytes2 = Encoding.ASCII.GetBytes(randomChallenge);
				for (int i = 0; i < keyLength; i++)
				{
					bytes2[i * bytes[i % length] % b] ^= (byte)(bytes2[i % b] ^ sbResponse[i + keyOffset]);
				}
				return GOACryptStateInit(bytes2, b);
			}
			catch (Exception)
			{
				return null;
			}
		}

		private byte[] GOADecrypt(GOACryptState state, byte[] encryptedBuffer, int startIndex, int len)
		{
			short num = state.rotor;
			short num2 = state.ratchet;
			short num3 = state.avalanche;
			short num4 = state.last_plain;
			short num5 = state.last_cipher;
			byte[] array = new byte[len];
			for (int i = 0; i < len; i++)
			{
				array[i] = encryptedBuffer[i + startIndex];
			}
			for (int i = 0; i < len; i++)
			{
				try
				{
					num2 = (short)((num2 + state.cards[num]) & 0xFF);
					num = (short)((num != 255) ? ((short)(num + 1)) : 0);
					short num6 = state.cards[num5];
					state.cards[num5] = state.cards[num2];
					state.cards[num2] = state.cards[num4];
					state.cards[num4] = state.cards[num];
					state.cards[num] = num6;
					num3 = (short)((num3 + state.cards[num6]) & 0xFF);
					num4 = (short)((short)(array[i] & 0xFF) ^ state.cards[(state.cards[num3] + state.cards[num]) & 0xFF] ^ state.cards[state.cards[(state.cards[num4] + state.cards[num5] + state.cards[num2]) & 0xFF]]);
					num5 = (short)(array[i] & 0xFF);
					array[i] = (byte)((uint)num4 & 0xFFu);
				}
				catch (Exception)
				{
					return null;
				}
			}
			state.rotor = num;
			state.ratchet = num2;
			state.avalanche = num3;
			state.last_plain = num4;
			state.last_cipher = num5;
			return array;
		}

		private byte[] DecryptSBResponse(byte[] encryptedResponse)
		{
			if (!gcs.initialized)
			{
				byte b = (byte)((encryptedResponse[0] ^ 0xEC) + 2);
				byte b2 = (byte)(encryptedResponse[b - 1] ^ 0xEAu);
				gcs = InitCryptKey(encryptedResponse, b, b2);
				int num = b + b2;
				return GOADecrypt(gcs, encryptedResponse, num, encryptedResponse.Length - num);
			}
			return GOADecrypt(gcs, encryptedResponse, 0, encryptedResponse.Length);
		}

		private bool CheckForHappyEnding(byte[] decryptedResp)
		{
			int num = -1;
			byte[] bytes = BitConverter.GetBytes(uint.MaxValue);
			for (int i = 0; i <= decryptedResp.Length - bytes.Length; i++)
			{
				num = i;
				for (int j = 0; j < bytes.Length; j++)
				{
					if (decryptedResp[i + j] != bytes[j])
					{
						num = -1;
						break;
					}
				}
				if (num != -1)
				{
					break;
				}
			}
			if (num >= 0)
			{
				return true;
			}
			return false;
		}

		private bool PopulateServerList(BinaryReader binReader)
		{
			bool result = false;
			int network = binReader.ReadInt32();
			network = IPAddress.NetworkToHostOrder(network);
			defaultPort = binReader.ReadInt16();
			defaultPort = IPAddress.NetworkToHostOrder(defaultPort);
			byte b = binReader.ReadByte();
			string[] array = new string[b];
			for (int i = 0; i < b; i++)
			{
				binReader.ReadByte();
				while (binReader.BaseStream.Length - binReader.BaseStream.Position > 0)
				{
					char c = binReader.ReadChar();
					if (c == '\0')
					{
						break;
					}
					array[i] += c;
				}
			}
			binReader.ReadByte();
			bool flag = false;
			bool flag2 = false;
			int num = 0;
			while (!flag && !flag2)
			{
				byte serverFlags = binReader.ReadByte();
				GameHost item = new GameHost(serverFlags);
				uint num2 = (uint)binReader.ReadInt32();
				if (num2 == uint.MaxValue)
				{
					result = true;
					flag = true;
				}
				else
				{
					_search_GameHosts.Add(item);
					if (_search_GameHosts[num].serverSize > binReader.BaseStream.Length - binReader.BaseStream.Position)
					{
						flag2 = true;
						continue;
					}
					try
					{
						_search_GameHosts[num].ip = TypeConverters.LongToIP(num2);
						if (_search_GameHosts[num].port != -1)
						{
							short network2 = binReader.ReadInt16();
							_search_GameHosts[num].port = (ushort)IPAddress.NetworkToHostOrder(network2);
						}
						else
						{
							_search_GameHosts[num].port = defaultPort;
						}
						if (!_search_GameHosts[num].privateIp.Equals("-1"))
						{
							uint num3 = (uint)binReader.ReadInt32();
							_search_GameHosts[num].privateIp = TypeConverters.LongToIP(num3);
						}
						if (_search_GameHosts[num].privatePort != -1)
						{
							short network3 = binReader.ReadInt16();
							_search_GameHosts[num].privatePort = (ushort)IPAddress.NetworkToHostOrder(network3);
						}
						else
						{
							_search_GameHosts[num].privatePort = defaultPort;
						}
						if (!_search_GameHosts[num].icmpIp.Equals("-1"))
						{
							uint num4 = (uint)binReader.ReadInt32();
							_search_GameHosts[num].icmpIp = TypeConverters.LongToIP(num4);
						}
					}
					catch (Exception)
					{
						_result = MatchmakingRequestResult.DecryptSearchResponseException;
						_searchRequestState = SearchRequestState.CompleteWithError;
						return true;
					}
					for (int j = 0; j < array.Length; j++)
					{
						string text = string.Empty;
						if (binReader.BaseStream.Length - binReader.BaseStream.Position == 0L)
						{
							flag2 = true;
							break;
						}
						binReader.ReadByte();
						bool flag3 = false;
						while (binReader.BaseStream.Length - binReader.BaseStream.Position > 0)
						{
							char c2 = binReader.ReadChar();
							if (c2 == '\0')
							{
								flag3 = true;
								break;
							}
							text += c2;
						}
						_search_GameHosts[num].keys.Add(new Key(array[j], text));
						if (binReader.BaseStream.Length - binReader.BaseStream.Position == 0L && !flag3)
						{
							flag2 = true;
							break;
						}
					}
				}
				if (!flag && !flag2)
				{
					num++;
				}
			}
			return result;
		}

		private void SearchConnectCallback(IAsyncResult ar)
		{
			try
			{
				Socket socket = (Socket)ar.AsyncState;
				socket.EndConnect(ar);
				_searchRequestState = SearchRequestState.StartSendSearchRequest;
			}
			catch (Exception)
			{
				_result = MatchmakingRequestResult.SearchSocketConnectException;
				_searchRequestState = SearchRequestState.CompleteWithError;
			}
		}

		private void SendMessageConnectCallback(IAsyncResult ar)
		{
			try
			{
				Socket socket = (Socket)ar.AsyncState;
				socket.EndConnect(ar);
				_sendMessageRequestState = SendMessageRequestState.StartSendMessageRequest;
				_sendMessageReceived = 0;
			}
			catch (Exception)
			{
				_result = MatchmakingRequestResult.SendMessageSocketConnectException;
				_searchRequestState = SearchRequestState.CompleteWithError;
			}
		}

		private void SearchSendCallback(IAsyncResult ar)
		{
			try
			{
				Socket socket = (Socket)ar.AsyncState;
				socket.EndSend(ar);
				_searchRequestState = SearchRequestState.StartReceiveSearchResponse;
			}
			catch (Exception)
			{
				_result = MatchmakingRequestResult.SearchSendRequestException;
				_searchRequestState = SearchRequestState.CompleteWithError;
			}
		}

		private void SendMessageSendCallback(IAsyncResult ar)
		{
			try
			{
				Socket socket = (Socket)ar.AsyncState;
				socket.EndSend(ar);
				_sendMessageReceived++;
				if (_sendMessageReceived == 2)
				{
					_sendMessageRequestState = SendMessageRequestState.ShuttingDown;
				}
			}
			catch (Exception)
			{
				_result = MatchmakingRequestResult.SendMessageSendRequestException;
				_searchRequestState = SearchRequestState.CompleteWithError;
			}
		}

		private void ServerBrowsingReceiveCallback(IAsyncResult ar)
		{
			try
			{
				StateObject stateObject = (StateObject)ar.AsyncState;
				Socket workSocket = stateObject.workSocket;
				int num = workSocket.EndReceive(ar);
				byte[] array = new byte[num];
				Array.Copy(stateObject.buffer, 0, array, 0, num);
				byte[] array2 = DecryptSBResponse(array);
				decryptedFullBuf.Write(array2);
				if (!CheckForHappyEnding(array2))
				{
					workSocket.BeginReceive(stateObject.buffer, 0, 1400, SocketFlags.None, ServerBrowsingReceiveCallback, stateObject);
					return;
				}
				decryptedFullBuf.Seek(0, SeekOrigin.Begin);
				byte[] buffer = new byte[(int)decryptedFullBuf.BaseStream.Length];
				decryptedFullBuf.BaseStream.Read(buffer, 0, (int)decryptedFullBuf.BaseStream.Length);
				Stream input = new MemoryStream(buffer);
				BinaryReader binReader = new BinaryReader(input);
				if (PopulateServerList(binReader))
				{
					if (_searchRequestState != SearchRequestState.CompleteWithError)
					{
						if (searchInProgress)
						{
							_searchRequestState = SearchRequestState.ShuttingDown;
						}
						else if (subscribeInProgress)
						{
							hostListUpdated = true;
							_searchRequestState = SearchRequestState.UpdateReceived;
						}
					}
				}
				else
				{
					_search_GameHosts = new List<GameHost>();
					workSocket.BeginReceive(stateObject.buffer, 0, 1400, SocketFlags.None, ServerBrowsingReceiveCallback, stateObject);
				}
			}
			catch (Exception)
			{
				_result = MatchmakingRequestResult.SearchResponseException;
				_searchRequestState = SearchRequestState.CompleteWithError;
			}
		}

		private bool CompareHostKeys(GameHost hostA, GameHost hostB)
		{
			if (hostA.keys.Count != hostB.keys.Count)
			{
				return false;
			}
			for (int i = 0; i < hostA.keys.Count; i++)
			{
				if (hostA.keys[i].KeyValue != hostB.keys[i].KeyValue)
				{
					return false;
				}
			}
			return true;
		}

		private void SearchSendRequest(Socket client, List<string> serverKeyNames, int count, string filter)
		{
			short num = 0;
			byte value = 0;
			byte value2 = 1;
			byte value3 = 3;
			int value4 = 0;
			string gameName = _gameName;
			string gameName2 = _gameName;
			string randomChallenge = _randomChallenge;
			string text = string.Empty;
			for (int i = 0; i < serverKeyNames.Count; i++)
			{
				text = text + "\\" + serverKeyNames[i];
			}
			int num2 = 1;
			num2 += 128;
			num = (short)(25 + gameName.Length + 1 + gameName2.Length + 1 + filter.Length + 1 + text.Length + 1);
			MemoryStream output = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(output);
			binaryWriter.Write(IPAddress.HostToNetworkOrder(num));
			binaryWriter.Write(value);
			binaryWriter.Write(value2);
			binaryWriter.Write(value3);
			binaryWriter.Write(value4);
			byte[] bytes = Encoding.ASCII.GetBytes(gameName + '\0');
			byte[] bytes2 = Encoding.ASCII.GetBytes(gameName2 + '\0');
			byte[] bytes3 = Encoding.ASCII.GetBytes(randomChallenge);
			byte[] bytes4 = Encoding.ASCII.GetBytes(filter + '\0');
			byte[] bytes5 = Encoding.ASCII.GetBytes(text + '\0');
			binaryWriter.Write(bytes);
			binaryWriter.Write(bytes2);
			binaryWriter.Write(bytes3);
			binaryWriter.Write(bytes4);
			binaryWriter.Write(bytes5);
			binaryWriter.Write(IPAddress.HostToNetworkOrder(num2));
			binaryWriter.Write(count);
			binaryWriter.Seek(0, SeekOrigin.Begin);
			byte[] array = new byte[(int)binaryWriter.BaseStream.Length];
			binaryWriter.BaseStream.Read(array, 0, (int)binaryWriter.BaseStream.Length);
			client.BeginSend(array, 0, array.Length, SocketFlags.None, SearchSendCallback, client);
		}

		private void SendMessageSendRequest(Socket client, string address, int port, string message)
		{
			if (message.Length > 128)
			{
				_sendMessageRequestState = SendMessageRequestState.ShuttingDown;
				_result = MatchmakingRequestResult.SendMessageRequestTooLong;
				return;
			}
			short value = IPAddress.HostToNetworkOrder((short)(9 + message.Length));
			byte value2 = 2;
			uint value3 = BitConverter.ToUInt32(IPAddress.Parse(address).GetAddressBytes(), 0);
			int num = IPAddress.HostToNetworkOrder(port);
			num >>= 16;
			ushort value4 = (ushort)num;
			byte[] bytes = Encoding.ASCII.GetBytes(message + '\0');
			MemoryStream output = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(output);
			MemoryStream output2 = new MemoryStream();
			BinaryWriter binaryWriter2 = new BinaryWriter(output2);
			binaryWriter.Write(value);
			binaryWriter.Write(value2);
			binaryWriter.Write(value3);
			binaryWriter.Write(value4);
			binaryWriter2.Write(bytes);
			binaryWriter.Seek(0, SeekOrigin.Begin);
			byte[] array = new byte[(int)binaryWriter.BaseStream.Length];
			binaryWriter.BaseStream.Read(array, 0, (int)binaryWriter.BaseStream.Length);
			binaryWriter2.Seek(0, SeekOrigin.Begin);
			byte[] array2 = new byte[(int)binaryWriter2.BaseStream.Length];
			binaryWriter2.BaseStream.Read(array2, 0, (int)binaryWriter2.BaseStream.Length);
			client.BeginSend(array, 0, array.Length, SocketFlags.None, SendMessageSendCallback, client);
			client.BeginSend(array2, 0, array2.Length, SocketFlags.None, SendMessageSendCallback, client);
		}

		private void Receive(Socket client)
		{
			StateObject stateObject = new StateObject();
			stateObject.workSocket = client;
			client.BeginReceive(stateObject.buffer, 0, 1400, SocketFlags.None, ServerBrowsingReceiveCallback, stateObject);
		}

		private void GetHostAddressesForBrowsing(IAsyncResult result)
		{
			try
			{
				IPAddress[] array = Dns.EndGetHostAddresses(result);
				_masterServerSBAddr = array[0];
				_searchRequestState = SearchRequestState.InitializeSocket;
				_sendMessageRequestState = SendMessageRequestState.InitializeSocket;
			}
			catch (Exception)
			{
				_result = MatchmakingRequestResult.DnsException;
				_searchRequestState = SearchRequestState.CompleteWithError;
			}
		}

		private void ResolveMasterServerSBEndpoint()
		{
			Dns.BeginGetHostAddresses(_gameName + ".ms0.gamespy.com", GetHostAddressesForBrowsing, null);
		}

		private void InitializeSearch()
		{
			_masterServerSBEndpoint = new IPEndPoint(_masterServerSBAddr, _masterServerSBPort);
			s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			gcs = new GOACryptState();
			outStream = new MemoryStream();
			decryptedFullBuf = new BinaryWriter(outStream);
			s.BeginConnect(_masterServerSBEndpoint, SearchConnectCallback, s);
		}

		private void InitializeSendMessage()
		{
			_masterServerSBEndpoint = new IPEndPoint(_masterServerSBAddr, _masterServerSBPort);
			sm = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			sm.BeginConnect(_masterServerSBEndpoint, SendMessageConnectCallback, sm);
		}

		public HostRequestState Host(List<Key> keysToList)
		{
			if (_clientMessageReceiveState == ClientMessageReceiveState.MessagePassedToHost)
			{
				_hostRequestState = HostRequestState.Listed;
				_clientMessageReceiveState = ClientMessageReceiveState.MessageNotYetReceived;
			}
			switch (_hostRequestState)
			{
			case HostRequestState.Begin:
				_result = MatchmakingRequestResult.Success;
				_hostRequestState = HostRequestState.DnsPending;
				_clientMessageReceiveState = ClientMessageReceiveState.MessageNotYetReceived;
				ResolveMasterServerHostingEndpoint();
				break;
			case HostRequestState.BeginLocalIPConnect:
				_hostRequestState = HostRequestState.LocalIPPending;
				sockLocalIP = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				ResolveLocalHostEndpointConnect(sockLocalIP);
				break;
			case HostRequestState.BeginLocalIPSend:
				_hostRequestState = HostRequestState.LocalIPPending;
				ResolveLocalHostEndpointSend(sockLocalIP);
				break;
			case HostRequestState.EndLocalIP:
				_hostRequestState = HostRequestState.InitializeSocket;
				ResolveLocalHostEndpointEnd(sockLocalIP);
				break;
			case HostRequestState.InitializeSocket:
				_hostRequestState = HostRequestState.ListedResponsePending;
				_lastKeyList = CopyKeysList(keysToList);
				InitializeSocket();
				break;
			case HostRequestState.ListedResponsePending:
			case HostRequestState.Listed:
				CheckSendHeartbeat(keysToList);
				break;
			case HostRequestState.ClientMessageReceived:
				_clientMessageReceiveState = ClientMessageReceiveState.MessagePassedToHost;
				break;
			case HostRequestState.CompleteWithError:
				_hostRequestState = HostRequestState.Complete;
				break;
			case HostRequestState.Complete:
				_hostRequestState = HostRequestState.InitializeSocket;
				_result = MatchmakingRequestResult.Success;
				break;
			}
			return _hostRequestState;
		}

		public HostRequestState StopHosting()
		{
			if (_hostRequestState != HostRequestState.Complete)
			{
				SendHeartbeat(_lastKeyList, 2);
				u.Close();
			}
			_hostRequestState = HostRequestState.Complete;
			return _hostRequestState;
		}

		public SearchRequestState Search(List<string> keyNames, int count, string filter)
		{
			if (count > 100)
			{
				_searchRequestState = SearchRequestState.CompleteWithError;
				_result = MatchmakingRequestResult.SearchCountTooHigh;
			}
			switch (_searchRequestState)
			{
			case SearchRequestState.Begin:
				searchInProgress = true;
				_result = MatchmakingRequestResult.Success;
				_searchRequestState = SearchRequestState.DnsPending;
				ResolveMasterServerSBEndpoint();
				break;
			case SearchRequestState.InitializeSocket:
				_searchRequestState = SearchRequestState.SocketConnectPending;
				InitializeSearch();
				break;
			case SearchRequestState.StartSendSearchRequest:
				_searchRequestState = SearchRequestState.SearchRequestPending;
				SearchSendRequest(s, keyNames, count, filter);
				break;
			case SearchRequestState.StartReceiveSearchResponse:
				_searchRequestState = SearchRequestState.ReceiveSearchResponsePending;
				Receive(s);
				break;
			case SearchRequestState.ShuttingDown:
				searchInProgress = false;
				_searchRequestState = SearchRequestState.Complete;
				s.Shutdown(SocketShutdown.Both);
				s.Close();
				break;
			case SearchRequestState.CompleteWithError:
				_searchRequestState = SearchRequestState.Complete;
				break;
			case SearchRequestState.Complete:
				searchInProgress = true;
				_searchRequestState = SearchRequestState.InitializeSocket;
				_result = MatchmakingRequestResult.Success;
				_search_GameHosts = new List<GameHost>();
				break;
			}
			return _searchRequestState;
		}

		public SearchRequestState Subscribe(List<string> keyNames, string filter)
		{
			if (!(_gameName == "MobShootios") && !(_gameName == "MobShootand") && !(_gameName == "GluTestiph") && !(_gameName == "gmtest"))
			{
				_result = MatchmakingRequestResult.MethodNotAvailable;
				_resultMessage = "Subscribe is no longer a supported feature";
				return SearchRequestState.Complete;
			}
			switch (_searchRequestState)
			{
			case SearchRequestState.Begin:
				subscribeInProgress = true;
				_result = MatchmakingRequestResult.Success;
				_searchRequestState = SearchRequestState.DnsPending;
				ResolveMasterServerSBEndpoint();
				break;
			case SearchRequestState.InitializeSocket:
				_searchRequestState = SearchRequestState.SocketConnectPending;
				InitializeSearch();
				break;
			case SearchRequestState.StartSendSearchRequest:
				_searchRequestState = SearchRequestState.SearchRequestPending;
				pollingStartTime = DateTime.Now.Ticks;
				SearchSendRequest(s, keyNames, 50, filter);
				break;
			case SearchRequestState.StartReceiveSearchResponse:
				_searchRequestState = SearchRequestState.ReceiveSearchResponsePending;
				Receive(s);
				break;
			case SearchRequestState.UpdateReceived:
				if (!updateReturned && hostListUpdated)
				{
					updateReturned = true;
					break;
				}
				updateReturned = false;
				hostListUpdated = false;
				_searchRequestState = SearchRequestState.ShuttingDown;
				break;
			case SearchRequestState.ShuttingDown:
				_searchRequestState = SearchRequestState.PollingInterval;
				s.Shutdown(SocketShutdown.Both);
				s.Close();
				break;
			case SearchRequestState.PollingInterval:
			{
				long ticks = DateTime.Now.Ticks;
				if (ticks - pollingStartTime > 60000000)
				{
					_searchRequestState = SearchRequestState.InitializeSocket;
					_result = MatchmakingRequestResult.Success;
					_search_GameHosts = new List<GameHost>();
				}
				break;
			}
			case SearchRequestState.CompleteWithError:
				subscribeInProgress = false;
				if (s.Connected)
				{
					s.Shutdown(SocketShutdown.Both);
					s.Close();
				}
				_searchRequestState = SearchRequestState.Complete;
				break;
			case SearchRequestState.Complete:
				subscribeInProgress = true;
				_searchRequestState = SearchRequestState.InitializeSocket;
				_result = MatchmakingRequestResult.Success;
				_search_GameHosts = new List<GameHost>();
				break;
			}
			return _searchRequestState;
		}

		public SearchRequestState StopSubscribing()
		{
			if (_searchRequestState != SearchRequestState.Complete)
			{
				subscribeInProgress = false;
				_searchRequestState = SearchRequestState.Complete;
				if (s.Connected)
				{
					s.Shutdown(SocketShutdown.Both);
					s.Close();
				}
			}
			return _searchRequestState;
		}

		public SendMessageRequestState SendMessageToHost(string ip, int port, string message)
		{
			switch (_sendMessageRequestState)
			{
			case SendMessageRequestState.Begin:
				_result = MatchmakingRequestResult.Success;
				_sendMessageRequestState = SendMessageRequestState.DnsPending;
				ResolveMasterServerSBEndpoint();
				break;
			case SendMessageRequestState.InitializeSocket:
				_sendMessageRequestState = SendMessageRequestState.SocketConnectPending;
				InitializeSendMessage();
				break;
			case SendMessageRequestState.StartSendMessageRequest:
				_sendMessageRequestState = SendMessageRequestState.SendMessageRequestPending;
				SendMessageSendRequest(sm, ip, port, message);
				break;
			case SendMessageRequestState.ShuttingDown:
				_sendMessageRequestState = SendMessageRequestState.Complete;
				sm.Shutdown(SocketShutdown.Both);
				sm.Close();
				break;
			case SendMessageRequestState.CompleteWithError:
				_sendMessageRequestState = SendMessageRequestState.Complete;
				break;
			case SendMessageRequestState.Complete:
				_sendMessageRequestState = SendMessageRequestState.InitializeSocket;
				_result = MatchmakingRequestResult.Success;
				break;
			}
			return _sendMessageRequestState;
		}
	}
}
