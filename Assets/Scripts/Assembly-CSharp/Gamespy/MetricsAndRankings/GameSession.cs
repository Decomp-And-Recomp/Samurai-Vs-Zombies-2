using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Gamespy.Authentication;
using Gamespy.Common;
using HTTP;

namespace Gamespy.MetricsAndRankings
{
	public class GameSession
	{
		public enum MatchResult
		{
			Win = 0,
			Loss = 1,
			Draw = 2,
			Disconnect = 3,
			Desync = 4,
			None = 5
		}

		public enum DataType
		{
			Int32 = 0,
			Int16 = 1,
			Byte = 2,
			String = 3,
			Float = 4,
			Int64 = 5
		}

		public class Key
		{
			public int keyId;

			public DataType type;

			public string value;

			public Key(int keyId, DataType type, string value)
			{
				this.keyId = keyId;
				this.type = type;
				this.value = value;
			}
		}

		public class PlayerData
		{
			public List<Key> keys;

			public MatchResult result;

			public string connectionId;

			public PlayerData(List<Key> keys, MatchResult result, string connectionId)
			{
				this.keys = keys;
				this.result = result;
				this.connectionId = connectionId;
			}
		}

		private const string _createSoapAction = "\"http://gamespy.net/atlas/services/submissionservice/CreateSession\"";

		private const string _joinSoapAction = "\"http://gamespy.net/atlas/services/submissionservice/SetReportIntention\"";

		private const string _submitSoapAction = "\"http://gamespy.net/atlas/services/submissionservice/SubmitReportData\"";

		private Account.AuthSecurityToken _securityToken;

		private string _atlasReportingUri = ".comp.pubsvs.gamespy.com/ATLAS/SubmissionService/2.0/SubmissionService.asmx";

		private Gamespy.Common.RequestState _requestState;

		private int _rulesetVersion;

		private string _hostConnectionId = string.Empty;

		private AtlasRequestResult _result;

		private string _resultMessage = string.Empty;

		private Request _createRequest;

		private string _create_SessionId = string.Empty;

		private Request _joinRequest;

		private string _join_ConnectionId = string.Empty;

		private Request _submitRequest;

		public AtlasRequestResult Result
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

		public string Create_SessionId
		{
			get
			{
				return _create_SessionId;
			}
		}

		public string Join_ConnectionId
		{
			get
			{
				return _join_ConnectionId;
			}
		}

		public GameSession(Account.AuthSecurityToken securityToken, int rulesetVersion)
		{
			if (string.IsNullOrEmpty(securityToken.GameName))
			{
				_result = AtlasRequestResult.ConstructorError;
				_resultMessage = "AuthSecurityToken was not populated - ensure Authenticate is successful before constructing a GameSession";
				_requestState = Gamespy.Common.RequestState.Complete;
				return;
			}
			_securityToken = securityToken;
			_rulesetVersion = rulesetVersion;
			if (Type.GetType("Mono.Runtime") != null && Debug.isDebugBuild)
			{
				_atlasReportingUri = "http://" + _securityToken.GameName + _atlasReportingUri;
			}
			else
			{
				_atlasReportingUri = "https://" + _securityToken.GameName + _atlasReportingUri;
			}
		}

		private void BuildCreateRequest(Request createRequest, Account.AuthSecurityToken securityToken)
		{
			int gameId = securityToken.GameId;
			string sessionToken = securityToken.SessionToken;
			int profileId = securityToken.ProfileId;
			LoginCertificate loginCert = securityToken.LoginCert;
			string proof = securityToken.Proof;
			createRequest.AddHeader("Content-Type", "text/xml");
			createRequest.AddHeader("SOAPAction", "\"http://gamespy.net/atlas/services/submissionservice/CreateSession\"");
			createRequest.AddHeader("GameID", gameId.ToString());
			createRequest.AddHeader("SessionToken", sessionToken);
			createRequest.AddHeader("ProfileID", profileId.ToString());
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:SOAP-ENC=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:gsc=\"http://gamespy.net/atlas/services/submissionservice/\"><SOAP-ENV:Body><gsc:CreateSession><gsc:certificate><gsc:length>" + loginCert.length + "</gsc:length><gsc:version>" + loginCert.version + "</gsc:version><gsc:partnercode>" + loginCert.partnercode + "</gsc:partnercode><gsc:namespaceid>" + loginCert.namespaceid + "</gsc:namespaceid><gsc:userid>" + loginCert.userid + "</gsc:userid><gsc:profileid>" + loginCert.profileid + "</gsc:profileid><gsc:expiretime>" + loginCert.expiretime + "</gsc:expiretime><gsc:profilenick>" + loginCert.profilenick + "</gsc:profilenick><gsc:uniquenick>" + loginCert.uniquenick + "</gsc:uniquenick><gsc:cdkeyhash></gsc:cdkeyhash><gsc:peerkeymodulus>" + loginCert.peerkeymodulus + "</gsc:peerkeymodulus><gsc:peerkeyexponent>" + loginCert.peerkeyexponent + "</gsc:peerkeyexponent><gsc:serverdata>" + loginCert.serverdata + "</gsc:serverdata><gsc:signature>" + loginCert.signature + "</gsc:signature><gsc:timestamp>" + loginCert.timestamp + "</gsc:timestamp></gsc:certificate><gsc:proof>" + proof + "</gsc:proof><gsc:gameid>" + gameId + "</gsc:gameid><gsc:platformid>0</gsc:platformid></gsc:CreateSession></SOAP-ENV:Body></SOAP-ENV:Envelope>");
			createRequest.bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
		}

		private void BuildJoinRequest(Request joinRequest, Account.AuthSecurityToken securityToken, string sessionId)
		{
			int gameId = securityToken.GameId;
			string sessionToken = securityToken.SessionToken;
			int profileId = securityToken.ProfileId;
			LoginCertificate loginCert = securityToken.LoginCert;
			string proof = securityToken.Proof;
			joinRequest.AddHeader("Content-Type", "text/xml");
			joinRequest.AddHeader("SOAPAction", "\"http://gamespy.net/atlas/services/submissionservice/SetReportIntention\"");
			joinRequest.AddHeader("GameID", gameId.ToString());
			joinRequest.AddHeader("SessionToken", sessionToken);
			joinRequest.AddHeader("ProfileID", profileId.ToString());
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:SOAP-ENC=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:gsc=\"http://gamespy.net/atlas/services/submissionservice/\"><SOAP-ENV:Body><gsc:SetReportIntention><gsc:certificate><gsc:length>" + loginCert.length + "</gsc:length><gsc:version>" + loginCert.version + "</gsc:version><gsc:partnercode>" + loginCert.partnercode + "</gsc:partnercode><gsc:namespaceid>" + loginCert.namespaceid + "</gsc:namespaceid><gsc:userid>" + loginCert.userid + "</gsc:userid><gsc:profileid>" + loginCert.profileid + "</gsc:profileid><gsc:expiretime>" + loginCert.expiretime + "</gsc:expiretime><gsc:profilenick>" + loginCert.profilenick + "</gsc:profilenick><gsc:uniquenick>" + loginCert.uniquenick + "</gsc:uniquenick><gsc:cdkeyhash></gsc:cdkeyhash><gsc:peerkeymodulus>" + loginCert.peerkeymodulus + "</gsc:peerkeymodulus><gsc:peerkeyexponent>" + loginCert.peerkeyexponent + "</gsc:peerkeyexponent><gsc:serverdata>" + loginCert.serverdata + "</gsc:serverdata><gsc:signature>" + loginCert.signature + "</gsc:signature><gsc:timestamp>" + loginCert.timestamp + "</gsc:timestamp></gsc:certificate><gsc:proof>" + proof + "</gsc:proof><gsc:gameid>" + gameId + "</gsc:gameid><gsc:csid>" + sessionId + "</gsc:csid>");
			if (_hostConnectionId.Length > 0)
			{
				stringBuilder.Append("<gsc:ccid>" + _hostConnectionId + "</gsc:ccid>");
			}
			else
			{
				stringBuilder.Append("<gsc:ccid></gsc:ccid>");
			}
			stringBuilder.Append("<gsc:authoritative>1</gsc:authoritative></gsc:SetReportIntention></SOAP-ENV:Body></SOAP-ENV:Envelope>");
			joinRequest.bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
		}

		private void BuildSubmitRequest(Request submitRequest, Account.AuthSecurityToken securityToken, string sessionId, string connectionId, string snapshot)
		{
			int gameId = securityToken.GameId;
			string sessionToken = securityToken.SessionToken;
			int profileId = securityToken.ProfileId;
			LoginCertificate loginCert = securityToken.LoginCert;
			string proof = securityToken.Proof;
			submitRequest.AddHeader("Content-Type", "text/xml");
			submitRequest.AddHeader("SOAPAction", "\"http://gamespy.net/atlas/services/submissionservice/SubmitReportData\"");
			submitRequest.AddHeader("GameID", gameId.ToString());
			submitRequest.AddHeader("SessionToken", sessionToken);
			submitRequest.AddHeader("ProfileID", profileId.ToString());
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:SOAP-ENC=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:gsc=\"http://gamespy.net/atlas/services/submissionservice/\"><SOAP-ENV:Body><gsc:SubmitReportData><gsc:certificate><gsc:length>" + loginCert.length + "</gsc:length><gsc:version>" + loginCert.version + "</gsc:version><gsc:partnercode>" + loginCert.partnercode + "</gsc:partnercode><gsc:namespaceid>" + loginCert.namespaceid + "</gsc:namespaceid><gsc:userid>" + loginCert.userid + "</gsc:userid><gsc:profileid>" + loginCert.profileid + "</gsc:profileid><gsc:expiretime>" + loginCert.expiretime + "</gsc:expiretime><gsc:profilenick>" + loginCert.profilenick + "</gsc:profilenick><gsc:uniquenick>" + loginCert.uniquenick + "</gsc:uniquenick><gsc:cdkeyhash></gsc:cdkeyhash><gsc:peerkeymodulus>" + loginCert.peerkeymodulus + "</gsc:peerkeymodulus><gsc:peerkeyexponent>" + loginCert.peerkeyexponent + "</gsc:peerkeyexponent><gsc:serverdata>" + loginCert.serverdata + "</gsc:serverdata><gsc:signature>" + loginCert.signature + "</gsc:signature><gsc:timestamp>" + loginCert.timestamp + "</gsc:timestamp></gsc:certificate><gsc:proof>" + proof + "</gsc:proof><gsc:gameid>" + gameId + "</gsc:gameid><gsc:csid>" + sessionId + "</gsc:csid><gsc:ccid>" + _join_ConnectionId + "</gsc:ccid><gsc:authoritative>1</gsc:authoritative><gsc:snapshotData>" + snapshot + "</gsc:snapshotData></gsc:SubmitReportData></SOAP-ENV:Body></SOAP-ENV:Envelope>");
			submitRequest.bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
		}

		public static int Get7BitEncodedLengthByteSize(int length)
		{
			if (length >= 128)
			{
				if (length >= 1024)
				{
					if (length >= 2097152)
					{
						if (length >= 268435456)
						{
							return 5;
						}
						return 4;
					}
					return 3;
				}
				return 2;
			}
			return 1;
		}

		private static int GetWriteSize(List<Key> keys)
		{
			int num = 0;
			for (int i = 0; i < keys.Count; i++)
			{
				num += 4;
				switch (keys[i].type)
				{
				case DataType.Byte:
					num++;
					break;
				case DataType.Int16:
					num += 2;
					break;
				case DataType.Int32:
				case DataType.Float:
					num += 4;
					break;
				case DataType.Int64:
					num += 8;
					break;
				case DataType.String:
					num += keys[i].value.ToString().Length;
					num += Get7BitEncodedLengthByteSize(keys[i].value.ToString().Length);
					break;
				}
			}
			return num;
		}

		private static void WriteKey(BinaryWriter writer, Key key)
		{
			writer.Write(IPAddress.HostToNetworkOrder((short)key.keyId));
			switch (key.type)
			{
			case DataType.Byte:
				writer.Write(IPAddress.HostToNetworkOrder((short)2));
				writer.Write(Convert.ToByte(key.value));
				break;
			case DataType.Float:
				writer.Write(IPAddress.HostToNetworkOrder((short)4));
				writer.Write(Convert.ToSingle(key.value));
				break;
			case DataType.Int16:
				writer.Write(IPAddress.HostToNetworkOrder((short)1));
				writer.Write(IPAddress.HostToNetworkOrder(Convert.ToInt16(key.value)));
				break;
			case DataType.Int32:
				writer.Write(IPAddress.HostToNetworkOrder((short)0));
				writer.Write(IPAddress.HostToNetworkOrder(Convert.ToInt32(key.value)));
				break;
			case DataType.Int64:
				writer.Write(IPAddress.HostToNetworkOrder((short)5));
				writer.Write(IPAddress.HostToNetworkOrder(Convert.ToInt64(key.value)));
				break;
			case DataType.String:
				writer.Write(IPAddress.HostToNetworkOrder((short)3));
				writer.Write(key.value);
				break;
			}
		}

		private string CreateSnapshot(int rulesetVer, List<Key> gameKeys, List<PlayerData> matchPlayers)
		{
			int host = 2;
			byte[] buffer = new byte[16];
			int host2 = 0;
			int host3 = 1;
			short num = (short)matchPlayers.Count;
			short num2 = 0;
			short num3 = (short)gameKeys.Count;
			short num4 = 0;
			for (int i = 0; i < num; i++)
			{
				num4 += (short)matchPlayers[i].keys.Count;
			}
			short host4 = 0;
			int host5 = 20 * num;
			int host6 = 0;
			int host7 = 4 * (num + num2);
			int writeSize = GetWriteSize(gameKeys);
			int num5 = 0;
			for (int j = 0; j < num; j++)
			{
				num5 += GetWriteSize(matchPlayers[j].keys);
				num5 += 2;
			}
			int host8 = 0;
			MemoryStream output = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(output);
			binaryWriter.Write(IPAddress.HostToNetworkOrder(host));
			binaryWriter.Write(IPAddress.HostToNetworkOrder(rulesetVer));
			binaryWriter.Write(buffer);
			binaryWriter.Write(IPAddress.HostToNetworkOrder(host2));
			binaryWriter.Write(IPAddress.HostToNetworkOrder(host3));
			binaryWriter.Write(IPAddress.HostToNetworkOrder(num));
			binaryWriter.Write(IPAddress.HostToNetworkOrder(num2));
			binaryWriter.Write(IPAddress.HostToNetworkOrder(num3));
			binaryWriter.Write(IPAddress.HostToNetworkOrder(num4));
			binaryWriter.Write(IPAddress.HostToNetworkOrder(host4));
			binaryWriter.Write(IPAddress.HostToNetworkOrder((short)0));
			binaryWriter.Write(IPAddress.HostToNetworkOrder(host5));
			binaryWriter.Write(IPAddress.HostToNetworkOrder(host6));
			binaryWriter.Write(IPAddress.HostToNetworkOrder(host7));
			binaryWriter.Write(IPAddress.HostToNetworkOrder(writeSize));
			binaryWriter.Write(IPAddress.HostToNetworkOrder(num5));
			binaryWriter.Write(IPAddress.HostToNetworkOrder(host8));
			for (int k = 0; k < num; k++)
			{
				byte[] array = new Guid(matchPlayers[k].connectionId).ToByteArray();
				binaryWriter.Write(new byte[16]
				{
					array[3],
					array[2],
					array[1],
					array[0],
					array[5],
					array[4],
					array[7],
					array[6],
					array[8],
					array[9],
					array[10],
					array[11],
					array[12],
					array[13],
					array[14],
					array[15]
				});
				binaryWriter.Write(IPAddress.HostToNetworkOrder(0));
			}
			for (int l = 0; l < num; l++)
			{
				binaryWriter.Write(IPAddress.HostToNetworkOrder((int)matchPlayers[l].result));
			}
			for (int m = 0; m < num3; m++)
			{
				WriteKey(binaryWriter, gameKeys[m]);
			}
			for (int n = 0; n < num; n++)
			{
				binaryWriter.Write(IPAddress.HostToNetworkOrder((short)matchPlayers[n].keys.Count));
				for (int num6 = 0; num6 < matchPlayers[n].keys.Count; num6++)
				{
					WriteKey(binaryWriter, matchPlayers[n].keys[num6]);
				}
			}
			MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
			byte[] array2 = new byte[binaryWriter.BaseStream.Length];
			array2 = ((MemoryStream)binaryWriter.BaseStream).ToArray();
			buffer = mD5CryptoServiceProvider.ComputeHash(array2);
			binaryWriter.Seek(8, SeekOrigin.Begin);
			binaryWriter.Write(buffer);
			binaryWriter.Seek(0, SeekOrigin.Begin);
			byte[] array3 = new byte[(int)binaryWriter.BaseStream.Length];
			binaryWriter.BaseStream.Read(array3, 0, (int)binaryWriter.BaseStream.Length);
			return TypeConverters.ByteArrayToHexString(array3);
		}

		public Gamespy.Common.RequestState Create()
		{
			if (_requestState == Gamespy.Common.RequestState.Complete && _result != AtlasRequestResult.ConstructorError)
			{
				_requestState = Gamespy.Common.RequestState.Beginning;
				_result = AtlasRequestResult.Success;
				_resultMessage = string.Empty;
			}
			switch (_requestState)
			{
			case Gamespy.Common.RequestState.Beginning:
				try
				{
					_createRequest = new Request("POST", _atlasReportingUri);
					BuildCreateRequest(_createRequest, _securityToken);
				}
				catch (Exception ex2)
				{
					_result = AtlasRequestResult.ErrorCreatingRequest;
					_resultMessage = ex2.GetType().ToString() + ": " + ex2.Message;
					_requestState = Gamespy.Common.RequestState.Complete;
					break;
				}
				_createRequest.Send();
				_requestState = Gamespy.Common.RequestState.Pending;
				break;
			case Gamespy.Common.RequestState.Pending:
				if (_createRequest.state != HTTP.RequestState.Done)
				{
					break;
				}
				if (_createRequest.exception != null)
				{
					_result = AtlasRequestResult.ErrorSendingRequest;
				}
				else if (_createRequest.response.status != 200 && _createRequest.response.status != 500)
				{
					_result = AtlasRequestResult.HttpError;
				}
				else
				{
					string header = _createRequest.response.GetHeader("Error");
					if (header != string.Empty)
					{
						_result = AtlasRequestResult.Error;
						_resultMessage = header;
						_requestState = Gamespy.Common.RequestState.Complete;
						break;
					}
					bool flag = false;
					string text = _createRequest.response.Text;
					try
					{
						XmlReader xmlReader = XmlReader.Create(new StringReader(text));
						xmlReader.Read();
						while (!xmlReader.EOF)
						{
							if (xmlReader.NodeType == XmlNodeType.Element)
							{
								switch (xmlReader.Name)
								{
								case "result":
									if (xmlReader.ReadElementContentAsInt() == 0)
									{
										_result = AtlasRequestResult.Success;
									}
									else
									{
										_result = AtlasRequestResult.Error;
									}
									flag = true;
									break;
								case "message":
									_resultMessage = xmlReader.ReadElementContentAsString();
									break;
								case "csid":
									_create_SessionId = xmlReader.ReadElementContentAsString();
									break;
								case "ccid":
									_hostConnectionId = xmlReader.ReadElementContentAsString();
									break;
								case "faultstring":
									_resultMessage = xmlReader.ReadElementContentAsString();
									flag = true;
									break;
								default:
									xmlReader.Read();
									break;
								}
							}
							else
							{
								xmlReader.Read();
							}
						}
					}
					catch (Exception ex)
					{
						_resultMessage = ex.GetType().ToString() + ": " + ex.Message;
					}
					if (_createRequest.response.status == 500)
					{
						_result = AtlasRequestResult.HttpError;
					}
					else if (!flag)
					{
						_result = AtlasRequestResult.ResponseParseError;
					}
				}
				_requestState = Gamespy.Common.RequestState.Complete;
				break;
			}
			return _requestState;
		}

		public Gamespy.Common.RequestState Join(string sessionId)
		{
			if (_requestState == Gamespy.Common.RequestState.Complete && _result != AtlasRequestResult.ConstructorError)
			{
				_requestState = Gamespy.Common.RequestState.Beginning;
				_result = AtlasRequestResult.Success;
				_resultMessage = string.Empty;
			}
			switch (_requestState)
			{
			case Gamespy.Common.RequestState.Beginning:
				_create_SessionId = sessionId;
				try
				{
					_joinRequest = new Request("POST", _atlasReportingUri);
					BuildJoinRequest(_joinRequest, _securityToken, sessionId);
				}
				catch (Exception ex2)
				{
					_result = AtlasRequestResult.ErrorCreatingRequest;
					_resultMessage = ex2.GetType().ToString() + ": " + ex2.Message;
					_requestState = Gamespy.Common.RequestState.Complete;
					break;
				}
				_joinRequest.Send();
				_requestState = Gamespy.Common.RequestState.Pending;
				break;
			case Gamespy.Common.RequestState.Pending:
				if (_joinRequest.state != HTTP.RequestState.Done)
				{
					break;
				}
				if (_joinRequest.exception != null)
				{
					_result = AtlasRequestResult.ErrorSendingRequest;
				}
				else if (_joinRequest.response.status != 200 && _joinRequest.response.status != 500)
				{
					_result = AtlasRequestResult.HttpError;
				}
				else
				{
					string header = _joinRequest.response.GetHeader("Error");
					if (header != string.Empty)
					{
						_result = AtlasRequestResult.Error;
						_resultMessage = header;
						_requestState = Gamespy.Common.RequestState.Complete;
						break;
					}
					bool flag = false;
					string text = _joinRequest.response.Text;
					try
					{
						XmlReader xmlReader = XmlReader.Create(new StringReader(text));
						xmlReader.Read();
						while (!xmlReader.EOF)
						{
							if (xmlReader.NodeType == XmlNodeType.Element)
							{
								switch (xmlReader.Name)
								{
								case "result":
									if (xmlReader.ReadElementContentAsInt() == 0)
									{
										_result = AtlasRequestResult.Success;
									}
									else
									{
										_result = AtlasRequestResult.Error;
									}
									flag = true;
									break;
								case "message":
									_resultMessage = xmlReader.ReadElementContentAsString();
									break;
								case "ccid":
									_join_ConnectionId = xmlReader.ReadElementContentAsString();
									break;
								case "faultstring":
									_resultMessage = xmlReader.ReadElementContentAsString();
									flag = true;
									break;
								default:
									xmlReader.Read();
									break;
								}
							}
							else
							{
								xmlReader.Read();
							}
						}
					}
					catch (Exception ex)
					{
						_resultMessage = ex.GetType().ToString() + ": " + ex.Message;
					}
					if (_joinRequest.response.status == 500)
					{
						_result = AtlasRequestResult.HttpError;
					}
					else if (!flag)
					{
						_result = AtlasRequestResult.ResponseParseError;
					}
				}
				_requestState = Gamespy.Common.RequestState.Complete;
				break;
			}
			return _requestState;
		}

		public Gamespy.Common.RequestState Submit(List<Key> gameKeys, List<PlayerData> sessionPlayers)
		{
			if (_requestState == Gamespy.Common.RequestState.Complete)
			{
				_requestState = Gamespy.Common.RequestState.Beginning;
				_resultMessage = string.Empty;
			}
			switch (_requestState)
			{
			case Gamespy.Common.RequestState.Beginning:
				try
				{
					string snapshot = CreateSnapshot(_rulesetVersion, gameKeys, sessionPlayers);
					_submitRequest = new Request("POST", _atlasReportingUri);
					BuildSubmitRequest(_submitRequest, _securityToken, _create_SessionId, _join_ConnectionId, snapshot);
				}
				catch (Exception ex2)
				{
					_result = AtlasRequestResult.ErrorCreatingRequest;
					_resultMessage = ex2.GetType().ToString() + ": " + ex2.Message;
					_requestState = Gamespy.Common.RequestState.Complete;
					break;
				}
				_submitRequest.Send();
				_requestState = Gamespy.Common.RequestState.Pending;
				break;
			case Gamespy.Common.RequestState.Pending:
				if (_submitRequest.state != HTTP.RequestState.Done)
				{
					break;
				}
				if (_submitRequest.exception != null)
				{
					_result = AtlasRequestResult.ErrorSendingRequest;
				}
				else if (_submitRequest.response.status != 200 && _submitRequest.response.status != 500)
				{
					_result = AtlasRequestResult.HttpError;
				}
				else
				{
					string header = _submitRequest.response.GetHeader("Error");
					if (header != string.Empty)
					{
						_result = AtlasRequestResult.Error;
						_resultMessage = header;
						_requestState = Gamespy.Common.RequestState.Complete;
						break;
					}
					bool flag = false;
					string text = _submitRequest.response.Text;
					try
					{
						XmlReader xmlReader = XmlReader.Create(new StringReader(text));
						xmlReader.Read();
						while (!xmlReader.EOF)
						{
							if (xmlReader.NodeType == XmlNodeType.Element)
							{
								switch (xmlReader.Name)
								{
								case "result":
									if (xmlReader.ReadElementContentAsInt() == 0)
									{
										_result = AtlasRequestResult.Success;
									}
									else
									{
										_result = AtlasRequestResult.Error;
									}
									flag = true;
									break;
								case "message":
									_resultMessage = xmlReader.ReadElementContentAsString();
									break;
								case "faultstring":
									_resultMessage = xmlReader.ReadElementContentAsString();
									flag = true;
									break;
								default:
									xmlReader.Read();
									break;
								}
							}
							else
							{
								xmlReader.Read();
							}
						}
					}
					catch (Exception ex)
					{
						_resultMessage = ex.GetType().ToString() + ": " + ex.Message;
					}
					if (_submitRequest.response.status == 500)
					{
						_result = AtlasRequestResult.HttpError;
					}
					else if (!flag)
					{
						_result = AtlasRequestResult.ResponseParseError;
					}
				}
				_requestState = Gamespy.Common.RequestState.Complete;
				break;
			}
			return _requestState;
		}
	}
}
