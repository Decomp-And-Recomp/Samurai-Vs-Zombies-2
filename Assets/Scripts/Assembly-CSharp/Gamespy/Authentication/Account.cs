using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Gamespy.Common;
using HTTP;

namespace Gamespy.Authentication
{
	public class Account
	{
		public class AuthSecurityToken : ICloneable
		{
			public string GameName;

			public string SecretKey;

			public int GameId;

			public string SessionToken;

			public int ProfileId;

			public LoginCertificate LoginCert;

			public string Proof;

			public float timeOfAuthentication;

			public object Clone()
			{
				return MemberwiseClone();
			}
		}

		public enum CreateResult
		{
			Success = 0,
			ServerInitFailed = 1,
			InvalidPassword = 2,
			NicknameInvalid = 3,
			NicknameAlreadyExistsForUser = 4,
			UniqueNicknameAlreadyInUse = 5,
			UserFoundForEmailButNotSamePassword = 6,
			DBError = 7,
			ServerError = 8,
			ErrorCreatingRequest = 9,
			HttpError = 10,
			ResponseParseError = 11,
			ErrorSendingRequest = 12,
			InvalidGameCredentials = 13
		}

		public enum AuthenticateResult
		{
			Success = 0,
			ServerInitFailed = 1,
			UserNotFound = 2,
			InvalidPassword = 3,
			InvalidProfile = 4,
			UniqueNickExpired = 5,
			DBError = 6,
			ServerError = 7,
			ErrorCreatingRequest = 8,
			HttpError = 9,
			ResponseParseError = 10,
			ErrorSendingRequest = 11,
			InvalidGameCredentials = 12,
			AlreadyLoggedIn = 13
		}

		private const string _createSoapAction = "\"http://gamespy.net/AuthService/CreateUserAccount\"";

		private const string _authenticateSoapAction = "\"http://gamespy.net/AuthService/LoginUniqueNickWithGameId\"";

		private string _accessKey = string.Empty;

		private Gamespy.Common.RequestState _requestState;

		private string _authUri = ".auth.pubsvs.gamespy.com/AuthService/AuthService.asmx";

		private Request _createRequest;

		private Request _authenticateRequest;

		private bool authenticated;

		private AuthSecurityToken _securityToken = new AuthSecurityToken();

		private AuthenticateResult _authenticate_Result;

		private int _authenticate_ProfileId = -1;

		private CreateResult _create_Result;

		private string _resultMessage;

		public AuthSecurityToken SecurityToken
		{
			get
			{
				return _securityToken;
			}
		}

		public AuthenticateResult Authenticate_Result
		{
			get
			{
				return _authenticate_Result;
			}
		}

		public int Authenticate_ProfileId
		{
			get
			{
				return _authenticate_ProfileId;
			}
		}

		public CreateResult Create_Result
		{
			get
			{
				return _create_Result;
			}
		}

		public string ResultMessage
		{
			get
			{
				return _resultMessage;
			}
		}

		private string EncryptPassword(string plainPassword)
		{
			string text = "BF05D63E93751AD4A59A4A7389CF0BE8A22CCDEEA1E7F12C062D6E194472EFDA5184CCECEB4FBADF5EB1D7ABFE91181453972AA971F624AF9BA8F0F82E2869FB7D44BDE8D56EE50977898F3FEE75869622C4981F07506248BD3D092E8EA05C12B2FA37881176084C8F8B8756C4722CDC57D2AD28ACD3AD85934FB48D6B2D2027";
			string text2 = "010001";
			byte[] modulus = TypeConverters.HexStringToByteArray(text);
			byte[] exponent = TypeConverters.HexStringToByteArray(text2);
			byte[] bytes = Encoding.UTF8.GetBytes(plainPassword);
			RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider();
			RSAParameters parameters = default(RSAParameters);
			parameters.Modulus = modulus;
			parameters.Exponent = exponent;
			rSACryptoServiceProvider.ImportParameters(parameters);
			byte[] bytes2 = rSACryptoServiceProvider.Encrypt(bytes, false);
			return TypeConverters.ByteArrayToHexString(bytes2);
		}

		private void BuildCreateRequest(Request createRequest, int partnerCode, int namespaceId, string email, string uniqueNick, string password, int gameId)
		{
			string text = EncryptPassword(password);
			string text2 = uniqueNick;
			if (text2.Length > 30)
			{
				text2 = text2.Substring(0, 20);
			}
			createRequest.AddHeader("Content-Type", "text/xml");
			createRequest.AddHeader("SOAPAction", "\"http://gamespy.net/AuthService/CreateUserAccount\"");
			createRequest.AddHeader("GameID", gameId.ToString());
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:SOAP-ENC=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:ns1=\"http://gamespy.net/AuthService/\"><SOAP-ENV:Body><ns1:CreateUserAccount><ns1:version>1</ns1:version><ns1:partnercode>" + partnerCode + "</ns1:partnercode><ns1:namespaceid>" + namespaceId + "</ns1:namespaceid><ns1:email>" + email + "</ns1:email><ns1:profilenick>" + text2 + "</ns1:profilenick><ns1:uniquenick>" + uniqueNick + "</ns1:uniquenick><ns1:password><ns1:Value>" + text + "</ns1:Value></ns1:password></ns1:CreateUserAccount></SOAP-ENV:Body></SOAP-ENV:Envelope>");
			createRequest.bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
		}

		private void BuildAuthenticateRequest(Request authenticateRequest, int partnerCode, int namespaceId, string uniqueNick, string password, int gameId, string accessKey)
		{
			string text = EncryptPassword(password);
			authenticateRequest.AddHeader("Content-Type", "text/xml");
			authenticateRequest.AddHeader("SOAPAction", "\"http://gamespy.net/AuthService/LoginUniqueNickWithGameId\"");
			authenticateRequest.AddHeader("GameID", gameId.ToString());
			authenticateRequest.AddHeader("AccessKey", accessKey.ToString());
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:SOAP-ENC=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:ns1=\"http://gamespy.net/AuthService/\"><SOAP-ENV:Body><ns1:LoginUniqueNickWithGameId><ns1:version>1</ns1:version><ns1:partnercode>" + partnerCode + "</ns1:partnercode><ns1:namespaceid>" + namespaceId + "</ns1:namespaceid><ns1:uniquenick>" + uniqueNick + "</ns1:uniquenick><ns1:password><ns1:Value>" + text + "</ns1:Value></ns1:password><ns1:gameid>" + gameId.ToString() + "</ns1:gameid></ns1:LoginUniqueNickWithGameId></SOAP-ENV:Body></SOAP-ENV:Envelope>");
			authenticateRequest.bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
		}

		private string PrivateDataToProof(string privateData)
		{
			MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
			byte[] bytes = mD5CryptoServiceProvider.ComputeHash(TypeConverters.HexStringToByteArray(privateData));
			return TypeConverters.ByteArrayToHexString(bytes);
		}

		public void SetGameCredentials(string gameName, int gameId, string secretKey, string accessKey)
		{
			_securityToken.GameName = gameName;
			_securityToken.GameId = gameId;
			_securityToken.SecretKey = secretKey;
			_accessKey = accessKey;
			if (Type.GetType("Mono.Runtime") != null && Debug.isDebugBuild)
			{
				_authUri = "http://" + gameName + _authUri;
			}
			else
			{
				_authUri = "https://" + gameName + _authUri;
			}
		}

		public Gamespy.Common.RequestState Create(int partnerCode, int namespaceId, string email, string uniqueNick, string password)
		{
			if (_requestState == Gamespy.Common.RequestState.Complete)
			{
				_requestState = Gamespy.Common.RequestState.Beginning;
				_create_Result = CreateResult.Success;
				_resultMessage = string.Empty;
			}
			switch (_requestState)
			{
			case Gamespy.Common.RequestState.Beginning:
				if (_securityToken.GameName != string.Empty && _securityToken.GameId >= 0 && _securityToken.SecretKey != string.Empty && _accessKey != string.Empty)
				{
					try
					{
						_createRequest = new Request("POST", _authUri);
						BuildCreateRequest(_createRequest, partnerCode, namespaceId, email, uniqueNick, password, _securityToken.GameId);
					}
					catch (Exception ex2)
					{
						_create_Result = CreateResult.ErrorCreatingRequest;
						_resultMessage = ex2.GetType().ToString() + ": " + ex2.Message;
						_requestState = Gamespy.Common.RequestState.Complete;
						break;
					}
					_createRequest.Send();
					_requestState = Gamespy.Common.RequestState.Pending;
				}
				else
				{
					_create_Result = CreateResult.InvalidGameCredentials;
					_requestState = Gamespy.Common.RequestState.Complete;
				}
				break;
			case Gamespy.Common.RequestState.Pending:
				if (_createRequest.state != HTTP.RequestState.Done)
				{
					break;
				}
				if (_createRequest.exception != null)
				{
					_create_Result = CreateResult.ErrorSendingRequest;
				}
				else if (_createRequest.response.status != 200)
				{
					_create_Result = CreateResult.HttpError;
				}
				else
				{
					string header = _createRequest.response.GetHeader("Error");
					if (header != string.Empty)
					{
						_create_Result = CreateResult.InvalidGameCredentials;
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
								case "responseCode":
								{
									int create_Result = xmlReader.ReadElementContentAsInt();
									_create_Result = (CreateResult)create_Result;
									flag = true;
									break;
								}
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
					if (!flag)
					{
						_create_Result = CreateResult.ResponseParseError;
					}
				}
				_requestState = Gamespy.Common.RequestState.Complete;
				break;
			}
			return _requestState;
		}

		public Gamespy.Common.RequestState Authenticate(int partnerCode, int namespaceId, string uniqueNick, string password)
		{
			if (_requestState == Gamespy.Common.RequestState.Complete)
			{
				_requestState = Gamespy.Common.RequestState.Beginning;
				_authenticate_Result = AuthenticateResult.Success;
				_resultMessage = string.Empty;
			}
			switch (_requestState)
			{
			case Gamespy.Common.RequestState.Beginning:
				if (authenticated)
				{
					_authenticate_Result = AuthenticateResult.AlreadyLoggedIn;
					_requestState = Gamespy.Common.RequestState.Complete;
				}
				if (_securityToken.GameName != string.Empty && _securityToken.GameId >= 0 && _securityToken.SecretKey != string.Empty && _accessKey != string.Empty)
				{
					try
					{
						_authenticateRequest = new Request("POST", _authUri);
						BuildAuthenticateRequest(_authenticateRequest, partnerCode, namespaceId, uniqueNick, password, _securityToken.GameId, _accessKey);
					}
					catch (Exception ex2)
					{
						_authenticate_Result = AuthenticateResult.ErrorCreatingRequest;
						_resultMessage = ex2.GetType().ToString() + ": " + ex2.Message;
						_requestState = Gamespy.Common.RequestState.Complete;
						break;
					}
					_authenticateRequest.Send();
					_requestState = Gamespy.Common.RequestState.Pending;
				}
				else
				{
					_authenticate_Result = AuthenticateResult.InvalidGameCredentials;
					_requestState = Gamespy.Common.RequestState.Complete;
				}
				break;
			case Gamespy.Common.RequestState.Pending:
				if (_authenticateRequest.state != HTTP.RequestState.Done)
				{
					break;
				}
				if (_authenticateRequest.exception != null)
				{
					_authenticate_Result = AuthenticateResult.ErrorSendingRequest;
				}
				else if (_authenticateRequest.response.status != 200)
				{
					_authenticate_Result = AuthenticateResult.HttpError;
				}
				else
				{
					string header = _authenticateRequest.response.GetHeader("Error");
					if (header != string.Empty)
					{
						_authenticate_Result = AuthenticateResult.InvalidGameCredentials;
						_resultMessage = header;
						_requestState = Gamespy.Common.RequestState.Complete;
						break;
					}
					bool flag = false;
					_securityToken.LoginCert = new LoginCertificate();
					_securityToken.SessionToken = _authenticateRequest.response.GetHeader("SessionToken");
					string text = _authenticateRequest.response.Text;
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
								case "responseCode":
								{
									int authenticate_Result = xmlReader.ReadElementContentAsInt();
									_authenticate_Result = (AuthenticateResult)authenticate_Result;
									if (_authenticate_Result == AuthenticateResult.Success)
									{
										authenticated = true;
									}
									flag = true;
									break;
								}
								case "length":
									_securityToken.LoginCert.length = xmlReader.ReadElementContentAsInt();
									break;
								case "version":
									_securityToken.LoginCert.version = xmlReader.ReadElementContentAsInt();
									break;
								case "partnercode":
									_securityToken.LoginCert.partnercode = xmlReader.ReadElementContentAsInt();
									break;
								case "namespaceid":
									_securityToken.LoginCert.namespaceid = xmlReader.ReadElementContentAsInt();
									break;
								case "userid":
									_securityToken.LoginCert.userid = xmlReader.ReadElementContentAsInt();
									break;
								case "profileid":
									_securityToken.LoginCert.profileid = xmlReader.ReadElementContentAsInt();
									_securityToken.ProfileId = _securityToken.LoginCert.profileid;
									_authenticate_ProfileId = _securityToken.LoginCert.profileid;
									break;
								case "expiretime":
									_securityToken.LoginCert.expiretime = xmlReader.ReadElementContentAsInt();
									break;
								case "profilenick":
									_securityToken.LoginCert.profilenick = xmlReader.ReadElementContentAsString();
									break;
								case "uniquenick":
									_securityToken.LoginCert.uniquenick = xmlReader.ReadElementContentAsString();
									break;
								case "cdkeyhash":
									_securityToken.LoginCert.cdkeyhash = xmlReader.ReadElementContentAsString();
									break;
								case "peerkeymodulus":
									_securityToken.LoginCert.peerkeymodulus = xmlReader.ReadElementContentAsString();
									break;
								case "peerkeyexponent":
									_securityToken.LoginCert.peerkeyexponent = xmlReader.ReadElementContentAsString();
									break;
								case "serverdata":
									_securityToken.LoginCert.serverdata = xmlReader.ReadElementContentAsString();
									break;
								case "signature":
									_securityToken.LoginCert.signature = xmlReader.ReadElementContentAsString();
									break;
								case "timestamp":
									_securityToken.LoginCert.timestamp = xmlReader.ReadElementContentAsString();
									break;
								case "email":
									_securityToken.LoginCert.email = xmlReader.ReadElementContentAsString();
									break;
								case "peerkeyprivate":
								{
									string privateData = xmlReader.ReadElementContentAsString();
									_securityToken.Proof = PrivateDataToProof(privateData);
									break;
								}
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
					if (!flag)
					{
						_authenticate_Result = AuthenticateResult.ResponseParseError;
					}
				}
				_requestState = Gamespy.Common.RequestState.Complete;
				break;
			}
			return _requestState;
		}
	}
}
