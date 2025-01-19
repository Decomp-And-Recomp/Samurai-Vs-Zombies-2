using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Gamespy.Authentication;
using Gamespy.Common;
using HTTP;

namespace Gamespy.MetricsAndRankings
{
	public class ProcessedStats
	{
		public class PlayerRecord
		{
			public int profileId;

			public Record record;
		}

		public class QueryParams
		{
			public int pageIndex = -1;

			public int pageSize = -1;

			public int[] profileIds;
		}

		private enum QueryScope
		{
			Game = 0,
			Player = 1
		}

		private const string _queryPlayerStatsSoapAction = "\"http://gamespy.net/atlas/services/dataservice/2010/11/StatisticsService/GetPlayerStats\"";

		private const string _queryGameStatsSoapAction = "\"http://gamespy.net/atlas/services/dataservice/2010/11/StatisticsService/GetGameStats\"";

		private Account.AuthSecurityToken _securityToken;

		private string _atlasQueryingUri = ".comp.pubsvs.gamespy.com/ATLAS/DataService/2.0/StatisticsService.svc";

		private Gamespy.Common.RequestState _requestState;

		private int _rulesetVersion;

		private AtlasRequestResult _result;

		private string _resultMessage = string.Empty;

		private Request _queryStatsRequest;

		private Record _queryStats_GameRecord;

		private List<PlayerRecord> _queryStats_PlayerRecords = new List<PlayerRecord>();

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

		public Record QueryStats_GameRecord
		{
			get
			{
				return _queryStats_GameRecord;
			}
		}

		public List<PlayerRecord> QueryStats_PlayerRecords
		{
			get
			{
				return _queryStats_PlayerRecords;
			}
		}

		public ProcessedStats(Account.AuthSecurityToken securityToken, int rulesetVersion)
		{
			if (string.IsNullOrEmpty(securityToken.GameName))
			{
				_result = AtlasRequestResult.ConstructorError;
				_resultMessage = "AuthSecurityToken was not populated - ensure Authenticate is successful before constructing a ProcessedStats object";
				_requestState = Gamespy.Common.RequestState.Complete;
				return;
			}
			_securityToken = securityToken;
			_rulesetVersion = rulesetVersion;
			if (Type.GetType("Mono.Runtime") != null && Debug.isDebugBuild)
			{
				_atlasQueryingUri = "http://" + _securityToken.GameName + _atlasQueryingUri;
			}
			else
			{
				_atlasQueryingUri = "https://" + _securityToken.GameName + _atlasQueryingUri;
			}
		}

		private void BuildQueryStatsRequest(Request queryStatsRequest, Account.AuthSecurityToken securityToken, string queryId, QueryScope queryScope, QueryParams queryParams)
		{
			int gameId = securityToken.GameId;
			string sessionToken = securityToken.SessionToken;
			int profileId = securityToken.ProfileId;
			LoginCertificate loginCert = securityToken.LoginCert;
			string proof = securityToken.Proof;
			string text = string.Empty;
			string value = string.Empty;
			switch (queryScope)
			{
			case QueryScope.Game:
				text = "GetGameStats";
				value = "\"http://gamespy.net/atlas/services/dataservice/2010/11/StatisticsService/GetGameStats\"";
				break;
			case QueryScope.Player:
				text = "GetPlayerStats";
				value = "\"http://gamespy.net/atlas/services/dataservice/2010/11/StatisticsService/GetPlayerStats\"";
				break;
			}
			queryStatsRequest.AddHeader("Content-Type", "text/xml");
			queryStatsRequest.AddHeader("SOAPAction", value);
			queryStatsRequest.AddHeader("GameID", gameId.ToString());
			queryStatsRequest.AddHeader("SessionToken", sessionToken);
			queryStatsRequest.AddHeader("ProfileID", profileId.ToString());
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:SOAP-ENC=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:gscd=\"http://gamespy.net/atlas/services/dataservice/2010/11\"><SOAP-ENV:Body><gscd:" + text + "><gscd:request><gscd:authcertificate><gscd:length>" + loginCert.length + "</gscd:length><gscd:version>" + loginCert.version + "</gscd:version><gscd:partnercode>" + loginCert.partnercode + "</gscd:partnercode><gscd:namespaceid>" + loginCert.namespaceid + "</gscd:namespaceid><gscd:userid>" + loginCert.userid + "</gscd:userid><gscd:profileid>" + loginCert.profileid + "</gscd:profileid><gscd:expiretime>" + loginCert.expiretime + "</gscd:expiretime><gscd:profilenick>" + loginCert.profilenick + "</gscd:profilenick><gscd:uniquenick>" + loginCert.uniquenick + "</gscd:uniquenick><gscd:cdkeyhash></gscd:cdkeyhash><gscd:peerkeymodulus>" + loginCert.peerkeymodulus + "</gscd:peerkeymodulus><gscd:peerkeyexponent>" + loginCert.peerkeyexponent + "</gscd:peerkeyexponent><gscd:serverdata>" + loginCert.serverdata + "</gscd:serverdata><gscd:signature>" + loginCert.signature + "</gscd:signature><gscd:timestamp>" + loginCert.timestamp + "</gscd:timestamp></gscd:authcertificate><gscd:authproof>" + proof + "</gscd:authproof><gscd:gameid>" + gameId + "</gscd:gameid><gscd:version>" + _rulesetVersion + "</gscd:version><gscd:queryid>" + queryId + "</gscd:queryid>");
			stringBuilder.Append("<gscd:parameters>");
			if (queryParams.pageIndex >= 0)
			{
				stringBuilder.Append("<gscd:parameter><gscd:name>pageindex</gscd:name><gscd:value>" + queryParams.pageIndex + "</gscd:value></gscd:parameter>");
			}
			if (queryParams.pageSize >= 0)
			{
				stringBuilder.Append("<gscd:parameter><gscd:name>pagesize</gscd:name><gscd:value>" + queryParams.pageSize + "</gscd:value></gscd:parameter>");
			}
			if (queryParams.profileIds != null && queryParams.profileIds.Length > 0)
			{
				stringBuilder.Append("<gscd:parameter>");
				if (queryParams.profileIds.Length == 1)
				{
					stringBuilder.Append("<gscd:name>profileid</gscd:name>");
				}
				else
				{
					stringBuilder.Append("<gscd:name>profileids</gscd:name>");
				}
				stringBuilder.Append("<gscd:value>");
				for (int i = 0; i < queryParams.profileIds.Length; i++)
				{
					if (i > 0)
					{
						stringBuilder.Append(",");
					}
					stringBuilder.Append(queryParams.profileIds[i]);
				}
				stringBuilder.Append("</gscd:value>");
				stringBuilder.Append("</gscd:parameter>");
			}
			stringBuilder.Append("</gscd:parameters></gscd:request></gscd:" + text + "></SOAP-ENV:Body></SOAP-ENV:Envelope>");
			queryStatsRequest.bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
		}

		private bool isValidQueryId(string queryId)
		{
			if (queryId.Length != 36 || queryId[8] != '-' || queryId[13] != '-' || queryId[18] != '-' || queryId[23] != '-')
			{
				return false;
			}
			return true;
		}

		public Gamespy.Common.RequestState QueryGameStats(string queryId)
		{
			return QueryStats(queryId, QueryScope.Game, null);
		}

		public Gamespy.Common.RequestState QueryPlayerStats(string queryId, QueryParams queryParams)
		{
			return QueryStats(queryId, QueryScope.Player, queryParams);
		}

		private Gamespy.Common.RequestState QueryStats(string queryId, QueryScope queryScope, QueryParams queryParams)
		{
			if (!isValidQueryId(queryId))
			{
				_result = AtlasRequestResult.InvalidQueryId;
				_requestState = Gamespy.Common.RequestState.Complete;
				return _requestState;
			}
			if (_requestState == Gamespy.Common.RequestState.Complete && _result != AtlasRequestResult.ConstructorError)
			{
				if (_queryStats_PlayerRecords.Count > 0)
				{
					_queryStats_PlayerRecords = new List<PlayerRecord>();
				}
				_requestState = Gamespy.Common.RequestState.Beginning;
				_result = AtlasRequestResult.Success;
				_resultMessage = string.Empty;
			}
			switch (_requestState)
			{
			case Gamespy.Common.RequestState.Beginning:
				if (queryParams == null)
				{
					queryParams = new QueryParams();
				}
				try
				{
					_queryStatsRequest = new Request("POST", _atlasQueryingUri);
					BuildQueryStatsRequest(_queryStatsRequest, _securityToken, queryId, queryScope, queryParams);
				}
				catch (Exception ex2)
				{
					_result = AtlasRequestResult.ErrorCreatingRequest;
					_resultMessage = ex2.GetType().ToString() + ": " + ex2.Message;
					_requestState = Gamespy.Common.RequestState.Complete;
					break;
				}
				_queryStatsRequest.Send();
				_requestState = Gamespy.Common.RequestState.Pending;
				break;
			case Gamespy.Common.RequestState.Pending:
				if (_queryStatsRequest.state != HTTP.RequestState.Done)
				{
					break;
				}
				if (_queryStatsRequest.exception != null)
				{
					_result = AtlasRequestResult.ErrorSendingRequest;
				}
				else if (_queryStatsRequest.response.status != 200)
				{
					_result = AtlasRequestResult.HttpError;
				}
				else
				{
					string header = _queryStatsRequest.response.GetHeader("Error");
					if (header != string.Empty)
					{
						_result = AtlasRequestResult.Error;
						_resultMessage = header;
						_requestState = Gamespy.Common.RequestState.Complete;
						break;
					}
					bool flag = false;
					string text = _queryStatsRequest.response.Text;
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
								case "number":
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
								case "p":
								{
									PlayerRecord playerRecord = new PlayerRecord();
									xmlReader.Read();
									if (xmlReader.Name == "pid")
									{
										playerRecord.profileId = xmlReader.ReadElementContentAsInt();
									}
									if (xmlReader.ReadToFollowing("s"))
									{
										List<Field> list2 = new List<Field>();
										while (xmlReader.NodeType != XmlNodeType.EndElement || !(xmlReader.Name == "p"))
										{
											if (xmlReader.Name == "s" && xmlReader.NodeType != XmlNodeType.EndElement)
											{
												Field field2 = new Field();
												while (xmlReader.MoveToNextAttribute())
												{
													if (xmlReader.Name == "n")
													{
														field2.Name = xmlReader.Value;
													}
													else if (xmlReader.Name == "v")
													{
														field2.ValueString = xmlReader.Value;
													}
													else if (xmlReader.Name == "t")
													{
														if (xmlReader.Value == "Int16")
														{
															field2.Type = FieldType.shortValue;
														}
														else if (xmlReader.Value == "Int32")
														{
															field2.Type = FieldType.intValue;
														}
														else if (xmlReader.Value == "Int64")
														{
															field2.Type = FieldType.int64Value;
														}
														else if (xmlReader.Value == "Single")
														{
															field2.Type = FieldType.floatValue;
														}
														else if (xmlReader.Value == "String")
														{
															field2.Type = FieldType.unicodeStringValue;
														}
														else if (xmlReader.Value == "Byte")
														{
															field2.Type = FieldType.byteValue;
														}
													}
												}
												list2.Add(field2);
											}
											xmlReader.Read();
										}
										playerRecord.record = new Record(list2);
									}
									_queryStats_PlayerRecords.Add(playerRecord);
									break;
								}
								case "stats":
								{
									if (!xmlReader.ReadToFollowing("s"))
									{
										break;
									}
									List<Field> list = new List<Field>();
									while (xmlReader.NodeType != XmlNodeType.EndElement || !(xmlReader.Name == "stats"))
									{
										if (xmlReader.Name == "s" && xmlReader.NodeType != XmlNodeType.EndElement)
										{
											Field field = new Field();
											while (xmlReader.MoveToNextAttribute())
											{
												if (xmlReader.Name == "n")
												{
													field.Name = xmlReader.Value;
												}
												else if (xmlReader.Name == "v")
												{
													field.ValueString = xmlReader.Value;
												}
												else if (xmlReader.Name == "t")
												{
													if (xmlReader.Value == "Int16")
													{
														field.Type = FieldType.shortValue;
													}
													else if (xmlReader.Value == "Int32")
													{
														field.Type = FieldType.intValue;
													}
													else if (xmlReader.Value == "Int64")
													{
														field.Type = FieldType.int64Value;
													}
													else if (xmlReader.Value == "Single")
													{
														field.Type = FieldType.floatValue;
													}
													else if (xmlReader.Value == "String")
													{
														field.Type = FieldType.unicodeStringValue;
													}
													else if (xmlReader.Value == "Byte")
													{
														field.Type = FieldType.byteValue;
													}
												}
											}
											list.Add(field);
										}
										xmlReader.Read();
									}
									_queryStats_GameRecord = new Record(list);
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
