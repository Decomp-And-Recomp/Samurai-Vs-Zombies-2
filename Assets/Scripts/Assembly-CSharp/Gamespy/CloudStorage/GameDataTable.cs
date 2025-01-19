using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using System.Xml;
using Gamespy.Authentication;
using Gamespy.Common;
using HTTP;

namespace Gamespy.CloudStorage
{
	public class GameDataTable
	{
		private const string _createRecordSoapAction = "\"http://gamespy.net/sake/CreateRecord\"";

		private const string _updateRecordSoapAction = "\"http://gamespy.net/sake/UpdateRecord\"";

		private const string _deleteRecordSoapAction = "\"http://gamespy.net/sake/DeleteRecord\"";

		private const string _searchSoapAction = "\"http://gamespy.net/sake/SearchForRecords\"";

		private const string _getMyRecordsSoapAction = "\"http://gamespy.net/sake/GetMyRecords\"";

		private const string _readAndLockRecordSoapAction = "\"http://gamespy.net/sake/ExecuteAdapter\"";

		private const string _updateAndUnlockRecordSoapAction = "\"http://gamespy.net/sake/ExecuteAdapter\"";

		private const string _getRecordCountSoapAction = "\"http://gamespy.net/sake/GetRecordCount\"";

		private PollSearchesRequestState _pollSearchesRequestState;

		private long _pollingStartTime;

		private Account.AuthSecurityToken _securityToken;

		private string _sakeUri = ".sake.gamespy.com/SakeStorageServer/Public/StorageServer.asmx";

		private string _sakeLockMethodsUri = ".sake.gamespy.com/SakeStorageServer/Public/StorageServer.asmx";

		private Gamespy.Common.RequestState _requestState;

		private string _tableName;

		private static int metrics_TotalCreates;

		private static int metrics_TotalSearches;

		private static int metrics_TotalGetMyRecords;

		private static int metrics_TotalUpdates;

		private static int metrics_TotalDeletes;

		private static int metrics_TotalCounts;

		private static int metrics_TotalReadLocks;

		private static int metrics_TotalUpdateUnlocks;

		private static int metrics_TotalCalls;

		private static float metrics_TimeSinceAuth;

		private static float metrics_AverageCreatesPerMinute;

		private static float metrics_AverageSearchesPerMinute;

		private static float metrics_AverageGetMyRecordsPerMinute;

		private static float metrics_AverageUpdatesPerMinute;

		private static float metrics_AverageDeletesPerMinute;

		private static float metrics_AverageCountsPerMinute;

		private static float metrics_AverageReadLocksPerMinute;

		private static float metrics_AverageUpdateUnlocksPerMinute;

		private static float metrics_AverageCallsPerMinute;

		private static string filePath_LogCallMetrics;

		private static string filePath_LogRequests;

		private SakeRequestResult _result;

		private string _resultMessage;

		private Request _createRecordRequest;

		private int _createRecord_RecordId = -1;

		private Request _updateRecordRequest;

		private Request _deleteRecordRequest;

		private Request _searchRequest;

		private List<Record> _search_Records = new List<Record>();

		private Request _getMyRecordsRequest;

		private List<Record> _getMyRecords_Records = new List<Record>();

		private Request _readAndLockRecordRequest;

		private Record _readAndLock_Record;

		private Request _updateAndUnlockRecordRequest;

		private int _getRecordCount_Count = -1;

		private Request _getRecordCountRequest;

		public SakeRequestResult Result
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

		public int CreateRecord_RecordId
		{
			get
			{
				return _createRecord_RecordId;
			}
		}

		public List<Record> Search_Records
		{
			get
			{
				return _search_Records;
			}
		}

		public List<Record> GetMyRecords_Records
		{
			get
			{
				return _getMyRecords_Records;
			}
		}

		public Record ReadAndLock_Record
		{
			get
			{
				return _readAndLock_Record;
			}
		}

		public int GetRecordCount_Count
		{
			get
			{
				return _getRecordCount_Count;
			}
		}

		public GameDataTable(Account.AuthSecurityToken securityToken, string tableName)
		{
			if (string.IsNullOrEmpty(securityToken.GameName))
			{
				_result = SakeRequestResult.ConstructorError;
				_resultMessage = "AuthSecurityToken was not populated - ensure Authenticate is successful before constructing a GameDataTable";
				_requestState = Gamespy.Common.RequestState.Complete;
				return;
			}
			_securityToken = securityToken;
			if (Type.GetType("Mono.Runtime") != null && Debug.isDebugBuild)
			{
				_sakeUri = "http://" + _securityToken.GameName + _sakeUri;
				_sakeLockMethodsUri = "http://" + _securityToken.GameName + _sakeLockMethodsUri;
			}
			else
			{
				_sakeUri = "https://" + _securityToken.GameName + _sakeUri;
				_sakeLockMethodsUri = "https://" + _securityToken.GameName + _sakeLockMethodsUri;
			}
			_tableName = tableName;
		}

		private string WriteXmlSafeString(string convertMe)
		{
			StringBuilder stringBuilder = new StringBuilder();
			convertMe = SecurityElement.Escape(convertMe);
			byte[] bytes = Encoding.UTF8.GetBytes(convertMe);
			for (int i = 0; i < bytes.Length; i++)
			{
				if (bytes[i] < 32 || bytes[i] > 127)
				{
					stringBuilder.AppendFormat("&#x{0:x2};", bytes[i]);
				}
				else
				{
					stringBuilder.Append((char)bytes[i]);
				}
			}
			return stringBuilder.ToString();
		}

		private string RecordToSoapString(Record record)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < record.Fields.Count; i++)
			{
				stringBuilder.Append("<ns1:RecordField><ns1:name>" + record.Fields[i].Name + "</ns1:name><ns1:value>");
				if (record.Fields[i].Type == FieldType.binaryDataValue)
				{
					string text = Convert.ToBase64String(record.Fields[i].ValueArray);
					stringBuilder.Append("<ns1:" + record.Fields[i].Type.ToString() + "><ns1:value>" + text + "</ns1:value></ns1:" + record.Fields[i].Type.ToString() + ">");
				}
				else
				{
					stringBuilder.Append("<ns1:" + record.Fields[i].Type.ToString() + "><ns1:value>" + WriteXmlSafeString(record.Fields[i].ValueString) + "</ns1:value></ns1:" + record.Fields[i].Type.ToString() + ">");
				}
				stringBuilder.Append("</ns1:value></ns1:RecordField>");
			}
			return stringBuilder.ToString();
		}

		private void BuildCreateRecordRequest(Request createRecordRequest, Account.AuthSecurityToken securityToken, string tableName, Record recordToCreate)
		{
			int gameId = securityToken.GameId;
			string sessionToken = securityToken.SessionToken;
			int profileId = securityToken.ProfileId;
			LoginCertificate loginCert = securityToken.LoginCert;
			string proof = securityToken.Proof;
			createRecordRequest.AddHeader("Content-Type", "text/xml");
			createRecordRequest.AddHeader("SOAPAction", "\"http://gamespy.net/sake/CreateRecord\"");
			createRecordRequest.AddHeader("GameID", gameId.ToString());
			createRecordRequest.AddHeader("SessionToken", sessionToken);
			createRecordRequest.AddHeader("ProfileID", profileId.ToString());
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:SOAP-ENC=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:ns1=\"http://gamespy.net/sake\"><SOAP-ENV:Body><ns1:CreateRecord><ns1:gameid>" + gameId + "</ns1:gameid><ns1:certificate><ns1:length>" + loginCert.length + "</ns1:length><ns1:version>" + loginCert.version + "</ns1:version><ns1:partnercode>" + loginCert.partnercode + "</ns1:partnercode><ns1:namespaceid>" + loginCert.namespaceid + "</ns1:namespaceid><ns1:userid>" + loginCert.userid + "</ns1:userid><ns1:profileid>" + loginCert.profileid + "</ns1:profileid><ns1:expiretime>" + loginCert.expiretime + "</ns1:expiretime><ns1:profilenick>" + loginCert.profilenick + "</ns1:profilenick><ns1:uniquenick>" + loginCert.uniquenick + "</ns1:uniquenick><ns1:cdkeyhash></ns1:cdkeyhash><ns1:peerkeymodulus>" + loginCert.peerkeymodulus + "</ns1:peerkeymodulus><ns1:peerkeyexponent>" + loginCert.peerkeyexponent + "</ns1:peerkeyexponent><ns1:serverdata>" + loginCert.serverdata + "</ns1:serverdata><ns1:signature>" + loginCert.signature + "</ns1:signature><ns1:timestamp>" + loginCert.timestamp + "</ns1:timestamp></ns1:certificate><ns1:proof>" + proof + "</ns1:proof><ns1:tableid>" + tableName + "</ns1:tableid><ns1:values>");
			stringBuilder.Append(RecordToSoapString(recordToCreate));
			stringBuilder.Append("</ns1:values></ns1:CreateRecord></SOAP-ENV:Body></SOAP-ENV:Envelope>");
			createRecordRequest.bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
		}

		private void BuildUpdateRecordRequest(Request updateRecordRequest, Account.AuthSecurityToken securityToken, string tableName, int updateRecordId, Record recordToUpdate)
		{
			int gameId = securityToken.GameId;
			string sessionToken = securityToken.SessionToken;
			int profileId = securityToken.ProfileId;
			LoginCertificate loginCert = securityToken.LoginCert;
			string proof = securityToken.Proof;
			updateRecordRequest.AddHeader("Content-Type", "text/xml");
			updateRecordRequest.AddHeader("SOAPAction", "\"http://gamespy.net/sake/UpdateRecord\"");
			updateRecordRequest.AddHeader("GameID", gameId.ToString());
			updateRecordRequest.AddHeader("SessionToken", sessionToken);
			updateRecordRequest.AddHeader("ProfileID", profileId.ToString());
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:SOAP-ENC=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:ns1=\"http://gamespy.net/sake\"><SOAP-ENV:Body><ns1:UpdateRecord><ns1:gameid>" + gameId + "</ns1:gameid><ns1:certificate><ns1:length>" + loginCert.length + "</ns1:length><ns1:version>" + loginCert.version + "</ns1:version><ns1:partnercode>" + loginCert.partnercode + "</ns1:partnercode><ns1:namespaceid>" + loginCert.namespaceid + "</ns1:namespaceid><ns1:userid>" + loginCert.userid + "</ns1:userid><ns1:profileid>" + loginCert.profileid + "</ns1:profileid><ns1:expiretime>" + loginCert.expiretime + "</ns1:expiretime><ns1:profilenick>" + loginCert.profilenick + "</ns1:profilenick><ns1:uniquenick>" + loginCert.uniquenick + "</ns1:uniquenick><ns1:cdkeyhash></ns1:cdkeyhash><ns1:peerkeymodulus>" + loginCert.peerkeymodulus + "</ns1:peerkeymodulus><ns1:peerkeyexponent>" + loginCert.peerkeyexponent + "</ns1:peerkeyexponent><ns1:serverdata>" + loginCert.serverdata + "</ns1:serverdata><ns1:signature>" + loginCert.signature + "</ns1:signature><ns1:timestamp>" + loginCert.timestamp + "</ns1:timestamp></ns1:certificate><ns1:proof>" + proof + "</ns1:proof><ns1:tableid>" + tableName + "</ns1:tableid><ns1:recordid>" + updateRecordId + "</ns1:recordid><ns1:values>");
			stringBuilder.Append(RecordToSoapString(recordToUpdate));
			stringBuilder.Append("</ns1:values></ns1:UpdateRecord></SOAP-ENV:Body></SOAP-ENV:Envelope>");
			updateRecordRequest.bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
		}

		private void BuildDeleteRecordRequest(Request deleteRecordRequest, Account.AuthSecurityToken securityToken, string tableName, int deleteRecordId)
		{
			int gameId = securityToken.GameId;
			string sessionToken = securityToken.SessionToken;
			int profileId = securityToken.ProfileId;
			LoginCertificate loginCert = securityToken.LoginCert;
			string proof = securityToken.Proof;
			deleteRecordRequest.AddHeader("Content-Type", "text/xml");
			deleteRecordRequest.AddHeader("SOAPAction", "\"http://gamespy.net/sake/DeleteRecord\"");
			deleteRecordRequest.AddHeader("GameID", gameId.ToString());
			deleteRecordRequest.AddHeader("SessionToken", sessionToken);
			deleteRecordRequest.AddHeader("ProfileID", profileId.ToString());
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:SOAP-ENC=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:ns1=\"http://gamespy.net/sake\"><SOAP-ENV:Body><ns1:DeleteRecord><ns1:gameid>" + gameId + "</ns1:gameid><ns1:certificate><ns1:length>" + loginCert.length + "</ns1:length><ns1:version>" + loginCert.version + "</ns1:version><ns1:partnercode>" + loginCert.partnercode + "</ns1:partnercode><ns1:namespaceid>" + loginCert.namespaceid + "</ns1:namespaceid><ns1:userid>" + loginCert.userid + "</ns1:userid><ns1:profileid>" + loginCert.profileid + "</ns1:profileid><ns1:expiretime>" + loginCert.expiretime + "</ns1:expiretime><ns1:profilenick>" + loginCert.profilenick + "</ns1:profilenick><ns1:uniquenick>" + loginCert.uniquenick + "</ns1:uniquenick><ns1:cdkeyhash></ns1:cdkeyhash><ns1:peerkeymodulus>" + loginCert.peerkeymodulus + "</ns1:peerkeymodulus><ns1:peerkeyexponent>" + loginCert.peerkeyexponent + "</ns1:peerkeyexponent><ns1:serverdata>" + loginCert.serverdata + "</ns1:serverdata><ns1:signature>" + loginCert.signature + "</ns1:signature><ns1:timestamp>" + loginCert.timestamp + "</ns1:timestamp></ns1:certificate><ns1:proof>" + proof + "</ns1:proof><ns1:tableid>" + tableName + "</ns1:tableid><ns1:recordid>" + deleteRecordId + "</ns1:recordid></ns1:DeleteRecord></SOAP-ENV:Body></SOAP-ENV:Envelope>");
			deleteRecordRequest.bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
		}

		private void BuildSearchRequest(Request searchRequest, Account.AuthSecurityToken securityToken, string tableName, string[] fieldNames, string sqlStyleFilter, string sqlStyleSort, int[] profileIds, int numRecordsPerPage, int pageNumber)
		{
			int gameId = securityToken.GameId;
			string sessionToken = securityToken.SessionToken;
			int profileId = securityToken.ProfileId;
			LoginCertificate loginCert = securityToken.LoginCert;
			string proof = securityToken.Proof;
			string text = WriteXmlSafeString(sqlStyleFilter);
			searchRequest.AddHeader("Content-Type", "text/xml");
			searchRequest.AddHeader("SOAPAction", "\"http://gamespy.net/sake/SearchForRecords\"");
			searchRequest.AddHeader("GameID", gameId.ToString());
			searchRequest.AddHeader("SessionToken", sessionToken);
			searchRequest.AddHeader("ProfileID", profileId.ToString());
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:SOAP-ENC=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:ns1=\"http://gamespy.net/sake\"><SOAP-ENV:Body><ns1:SearchForRecords><ns1:gameid>" + gameId + "</ns1:gameid><ns1:certificate><ns1:length>" + loginCert.length + "</ns1:length><ns1:version>" + loginCert.version + "</ns1:version><ns1:partnercode>" + loginCert.partnercode + "</ns1:partnercode><ns1:namespaceid>" + loginCert.namespaceid + "</ns1:namespaceid><ns1:userid>" + loginCert.userid + "</ns1:userid><ns1:profileid>" + loginCert.profileid + "</ns1:profileid><ns1:expiretime>" + loginCert.expiretime + "</ns1:expiretime><ns1:profilenick>" + loginCert.profilenick + "</ns1:profilenick><ns1:uniquenick>" + loginCert.uniquenick + "</ns1:uniquenick><ns1:cdkeyhash></ns1:cdkeyhash><ns1:peerkeymodulus>" + loginCert.peerkeymodulus + "</ns1:peerkeymodulus><ns1:peerkeyexponent>" + loginCert.peerkeyexponent + "</ns1:peerkeyexponent><ns1:serverdata>" + loginCert.serverdata + "</ns1:serverdata><ns1:signature>" + loginCert.signature + "</ns1:signature><ns1:timestamp>" + loginCert.timestamp + "</ns1:timestamp></ns1:certificate><ns1:proof>" + proof + "</ns1:proof><ns1:tableid>" + tableName + "</ns1:tableid><ns1:filter>" + text + "</ns1:filter><ns1:sort>" + sqlStyleSort + "</ns1:sort><ns1:offset>" + (pageNumber - 1) * numRecordsPerPage + "</ns1:offset><ns1:max>" + numRecordsPerPage + "</ns1:max>");
			if (profileIds != null)
			{
				stringBuilder.Append("<ns1:ownerids>");
				for (int i = 0; i < profileIds.Length; i++)
				{
					stringBuilder.Append("<ns1:int>" + profileIds[i] + "</ns1:int>");
				}
				stringBuilder.Append("</ns1:ownerids>");
			}
			stringBuilder.Append("<ns1:fields>");
			for (int j = 0; j < fieldNames.Length; j++)
			{
				stringBuilder.Append("<ns1:string>" + fieldNames[j] + "</ns1:string>");
			}
			stringBuilder.Append("</ns1:fields></ns1:SearchForRecords></SOAP-ENV:Body></SOAP-ENV:Envelope>");
			searchRequest.bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
		}

		private void BuildGetMyRecordsRequest(Request getMyRecordsRequest, Account.AuthSecurityToken securityToken, string tableName, string[] fieldNames)
		{
			int gameId = securityToken.GameId;
			string sessionToken = securityToken.SessionToken;
			int profileId = securityToken.ProfileId;
			LoginCertificate loginCert = securityToken.LoginCert;
			string proof = securityToken.Proof;
			getMyRecordsRequest.AddHeader("Content-Type", "text/xml");
			getMyRecordsRequest.AddHeader("SOAPAction", "\"http://gamespy.net/sake/GetMyRecords\"");
			getMyRecordsRequest.AddHeader("GameID", gameId.ToString());
			getMyRecordsRequest.AddHeader("SessionToken", sessionToken);
			getMyRecordsRequest.AddHeader("ProfileID", profileId.ToString());
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:SOAP-ENC=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:ns1=\"http://gamespy.net/sake\"><SOAP-ENV:Body><ns1:GetMyRecords><ns1:gameid>" + gameId + "</ns1:gameid><ns1:certificate><ns1:length>" + loginCert.length + "</ns1:length><ns1:version>" + loginCert.version + "</ns1:version><ns1:partnercode>" + loginCert.partnercode + "</ns1:partnercode><ns1:namespaceid>" + loginCert.namespaceid + "</ns1:namespaceid><ns1:userid>" + loginCert.userid + "</ns1:userid><ns1:profileid>" + loginCert.profileid + "</ns1:profileid><ns1:expiretime>" + loginCert.expiretime + "</ns1:expiretime><ns1:profilenick>" + loginCert.profilenick + "</ns1:profilenick><ns1:uniquenick>" + loginCert.uniquenick + "</ns1:uniquenick><ns1:cdkeyhash></ns1:cdkeyhash><ns1:peerkeymodulus>" + loginCert.peerkeymodulus + "</ns1:peerkeymodulus><ns1:peerkeyexponent>" + loginCert.peerkeyexponent + "</ns1:peerkeyexponent><ns1:serverdata>" + loginCert.serverdata + "</ns1:serverdata><ns1:signature>" + loginCert.signature + "</ns1:signature><ns1:timestamp>" + loginCert.timestamp + "</ns1:timestamp></ns1:certificate><ns1:proof>" + proof + "</ns1:proof><ns1:tableid>" + tableName + "</ns1:tableid>");
			stringBuilder.Append("<ns1:fields>");
			for (int i = 0; i < fieldNames.Length; i++)
			{
				stringBuilder.Append("<ns1:string>" + fieldNames[i] + "</ns1:string>");
			}
			stringBuilder.Append("</ns1:fields></ns1:GetMyRecords></SOAP-ENV:Body></SOAP-ENV:Envelope>");
			getMyRecordsRequest.bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
		}

		private void BuildReadAndLockRecordRequest(Request readAndLockRequest, Account.AuthSecurityToken securityToken, string tableName, int ownerId)
		{
			int gameId = securityToken.GameId;
			string sessionToken = securityToken.SessionToken;
			int profileId = securityToken.ProfileId;
			LoginCertificate loginCert = securityToken.LoginCert;
			string proof = securityToken.Proof;
			readAndLockRequest.AddHeader("Content-Type", "text/xml");
			readAndLockRequest.AddHeader("SOAPAction", "\"http://gamespy.net/sake/ExecuteAdapter\"");
			readAndLockRequest.AddHeader("GameID", gameId.ToString());
			readAndLockRequest.AddHeader("SessionToken", sessionToken);
			readAndLockRequest.AddHeader("ProfileID", profileId.ToString());
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><SOAP-ENV:Body><ExecuteAdapter xmlns=\"http://gamespy.net/sake\"><gameid>" + gameId + "</gameid><certificate><length>" + loginCert.length + "</length><version>" + loginCert.version + "</version><partnercode>" + loginCert.partnercode + "</partnercode><namespaceid>" + loginCert.namespaceid + "</namespaceid><userid>" + loginCert.userid + "</userid><profileid>" + loginCert.profileid + "</profileid><expiretime>" + loginCert.expiretime + "</expiretime><profilenick>" + loginCert.profilenick + "</profilenick><uniquenick>" + loginCert.uniquenick + "</uniquenick><cdkeyhash></cdkeyhash><peerkeymodulus>" + loginCert.peerkeymodulus + "</peerkeymodulus><peerkeyexponent>" + loginCert.peerkeyexponent + "</peerkeyexponent><serverdata>" + loginCert.serverdata + "</serverdata><signature>" + loginCert.signature + "</signature><timestamp>" + loginCert.timestamp + "</timestamp></certificate><proof>" + proof + "</proof><configid>" + tableName + "</configid><adapterName>ReadAndLock</adapterName><parameters><FieldValue><Name>OwnerID</Name><Value xsi:type=\"xsd:int\">" + ownerId + "</Value></FieldValue></parameters></ExecuteAdapter></SOAP-ENV:Body></SOAP-ENV:Envelope>");
			readAndLockRequest.bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
		}

		private void BuildUpdateAndUnlockRecordRequest(Request updateAndUnlockRecordRequest, Account.AuthSecurityToken securityToken, string tableName, int updateRecordId, Record recordToUpdate)
		{
			int gameId = securityToken.GameId;
			string sessionToken = securityToken.SessionToken;
			int profileId = securityToken.ProfileId;
			LoginCertificate loginCert = securityToken.LoginCert;
			string proof = securityToken.Proof;
			updateAndUnlockRecordRequest.AddHeader("Content-Type", "text/xml");
			updateAndUnlockRecordRequest.AddHeader("SOAPAction", "\"http://gamespy.net/sake/ExecuteAdapter\"");
			updateAndUnlockRecordRequest.AddHeader("GameID", gameId.ToString());
			updateAndUnlockRecordRequest.AddHeader("SessionToken", sessionToken);
			updateAndUnlockRecordRequest.AddHeader("ProfileID", profileId.ToString());
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><SOAP-ENV:Body><ExecuteAdapter xmlns=\"http://gamespy.net/sake\"><gameid>" + gameId + "</gameid><certificate><length>" + loginCert.length + "</length><version>" + loginCert.version + "</version><partnercode>" + loginCert.partnercode + "</partnercode><namespaceid>" + loginCert.namespaceid + "</namespaceid><userid>" + loginCert.userid + "</userid><profileid>" + loginCert.profileid + "</profileid><expiretime>" + loginCert.expiretime + "</expiretime><profilenick>" + loginCert.profilenick + "</profilenick><uniquenick>" + loginCert.uniquenick + "</uniquenick><cdkeyhash></cdkeyhash><peerkeymodulus>" + loginCert.peerkeymodulus + "</peerkeymodulus><peerkeyexponent>" + loginCert.peerkeyexponent + "</peerkeyexponent><serverdata>" + loginCert.serverdata + "</serverdata><signature>" + loginCert.signature + "</signature><timestamp>" + loginCert.timestamp + "</timestamp></certificate><proof>" + proof + "</proof><configid>" + tableName + "</configid><adapterName>UpdateAndUnlock</adapterName><parameters><FieldValue><Name>recordid</Name><Value xsi:type=\"xsd:int\">" + updateRecordId + "</Value></FieldValue>");
			if (recordToUpdate != null)
			{
				for (int i = 0; i < recordToUpdate.Fields.Count; i++)
				{
					stringBuilder.Append("<FieldValue><Name>" + recordToUpdate.Fields[i].Name + "</Name><Value xsi:type=\"xsd:");
					if (recordToUpdate.Fields[i].Type == FieldType.intValue)
					{
						stringBuilder.Append("int");
					}
					else if (recordToUpdate.Fields[i].Type == FieldType.int64Value)
					{
						stringBuilder.Append("long");
					}
					else if (recordToUpdate.Fields[i].Type == FieldType.shortValue)
					{
						stringBuilder.Append("short");
					}
					else if (recordToUpdate.Fields[i].Type == FieldType.dateAndTimeValue)
					{
						stringBuilder.Append("dateTime");
					}
					else if (recordToUpdate.Fields[i].Type == FieldType.byteValue)
					{
						stringBuilder.Append("byte");
					}
					else if (recordToUpdate.Fields[i].Type == FieldType.floatValue)
					{
						stringBuilder.Append("decimal");
					}
					else if (recordToUpdate.Fields[i].Type == FieldType.booleanValue)
					{
						stringBuilder.Append("boolean");
					}
					else if (recordToUpdate.Fields[i].Type == FieldType.binaryDataValue)
					{
						stringBuilder.Append("base64Binary");
					}
					else
					{
						stringBuilder.Append("string");
					}
					stringBuilder.Append("\">" + WriteXmlSafeString(recordToUpdate.Fields[i].ValueString) + "</Value></FieldValue>");
				}
			}
			stringBuilder.Append("</parameters></ExecuteAdapter></SOAP-ENV:Body></SOAP-ENV:Envelope>");
			updateAndUnlockRecordRequest.bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
		}

		private void BuildGetRecordCountRequest(Request getRecordCountRequest, Account.AuthSecurityToken securityToken, string tableName, string sqlStyleFilter)
		{
			int gameId = securityToken.GameId;
			string sessionToken = securityToken.SessionToken;
			int profileId = securityToken.ProfileId;
			LoginCertificate loginCert = securityToken.LoginCert;
			string proof = securityToken.Proof;
			string text = WriteXmlSafeString(sqlStyleFilter);
			getRecordCountRequest.AddHeader("Content-Type", "text/xml");
			getRecordCountRequest.AddHeader("SOAPAction", "\"http://gamespy.net/sake/GetRecordCount\"");
			getRecordCountRequest.AddHeader("GameID", gameId.ToString());
			getRecordCountRequest.AddHeader("SessionToken", sessionToken);
			getRecordCountRequest.AddHeader("ProfileID", profileId.ToString());
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:SOAP-ENC=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:ns1=\"http://gamespy.net/sake\"><SOAP-ENV:Body><ns1:GetRecordCount><ns1:gameid>" + gameId + "</ns1:gameid><ns1:certificate><ns1:length>" + loginCert.length + "</ns1:length><ns1:version>" + loginCert.version + "</ns1:version><ns1:partnercode>" + loginCert.partnercode + "</ns1:partnercode><ns1:namespaceid>" + loginCert.namespaceid + "</ns1:namespaceid><ns1:userid>" + loginCert.userid + "</ns1:userid><ns1:profileid>" + loginCert.profileid + "</ns1:profileid><ns1:expiretime>" + loginCert.expiretime + "</ns1:expiretime><ns1:profilenick>" + loginCert.profilenick + "</ns1:profilenick><ns1:uniquenick>" + loginCert.uniquenick + "</ns1:uniquenick><ns1:cdkeyhash></ns1:cdkeyhash><ns1:peerkeymodulus>" + loginCert.peerkeymodulus + "</ns1:peerkeymodulus><ns1:peerkeyexponent>" + loginCert.peerkeyexponent + "</ns1:peerkeyexponent><ns1:serverdata>" + loginCert.serverdata + "</ns1:serverdata><ns1:signature>" + loginCert.signature + "</ns1:signature><ns1:timestamp>" + loginCert.timestamp + "</ns1:timestamp></ns1:certificate><ns1:proof>" + proof + "</ns1:proof><ns1:tableid>" + tableName + "</ns1:tableid><ns1:filter>" + text + "</ns1:filter></ns1:GetRecordCount></SOAP-ENV:Body></SOAP-ENV:Envelope>");
			getRecordCountRequest.bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
		}

		public Gamespy.Common.RequestState CreateRecord(Record recordToCreate)
		{
			if (_requestState == Gamespy.Common.RequestState.Complete && _result != SakeRequestResult.ConstructorError)
			{
				_requestState = Gamespy.Common.RequestState.Beginning;
				_result = SakeRequestResult.Success;
				_resultMessage = string.Empty;
			}
			switch (_requestState)
			{
			case Gamespy.Common.RequestState.Beginning:
				try
				{
					_createRecordRequest = new Request("POST", _sakeUri);
					BuildCreateRecordRequest(_createRecordRequest, _securityToken, _tableName, recordToCreate);
				}
				catch (Exception ex2)
				{
					_result = SakeRequestResult.ErrorCreatingRequest;
					_resultMessage = ex2.GetType().ToString() + ": " + ex2.Message;
					_requestState = Gamespy.Common.RequestState.Complete;
					break;
				}
				_createRecordRequest.Send();
				_requestState = Gamespy.Common.RequestState.Pending;
				break;
			case Gamespy.Common.RequestState.Pending:
				if (_createRecordRequest.state != HTTP.RequestState.Done)
				{
					break;
				}
				if (_createRecordRequest.exception != null)
				{
					_result = SakeRequestResult.ErrorSendingRequest;
				}
				else if (_createRecordRequest.response.status != 200 && _createRecordRequest.response.status != 500)
				{
					_result = SakeRequestResult.HttpError;
				}
				else
				{
					string header = _createRecordRequest.response.GetHeader("Error");
					if (header != string.Empty)
					{
						_result = SakeRequestResult.Error;
						_resultMessage = header;
						_requestState = Gamespy.Common.RequestState.Complete;
						break;
					}
					bool flag = false;
					string text = _createRecordRequest.response.Text;
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
								case "CreateRecordResult":
								{
									string value = xmlReader.ReadElementContentAsString();
									_result = (SakeRequestResult)(int)Enum.Parse(typeof(SakeRequestResult), value);
									flag = true;
									break;
								}
								case "recordid":
									_createRecord_RecordId = xmlReader.ReadElementContentAsInt();
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
					if (_createRecordRequest.response.status == 500)
					{
						_result = SakeRequestResult.HttpError;
					}
					else if (!flag)
					{
						_result = SakeRequestResult.ResponseParseError;
					}
				}
				_requestState = Gamespy.Common.RequestState.Complete;
				break;
			}
			return _requestState;
		}

		public Gamespy.Common.RequestState UpdateRecord(int recordId, Record recordToUpdate)
		{
			if (_requestState == Gamespy.Common.RequestState.Complete && _result != SakeRequestResult.ConstructorError)
			{
				_requestState = Gamespy.Common.RequestState.Beginning;
				_result = SakeRequestResult.Success;
				_resultMessage = string.Empty;
			}
			switch (_requestState)
			{
			case Gamespy.Common.RequestState.Beginning:
				try
				{
					_updateRecordRequest = new Request("POST", _sakeUri);
					BuildUpdateRecordRequest(_updateRecordRequest, _securityToken, _tableName, recordId, recordToUpdate);
				}
				catch (Exception ex2)
				{
					_result = SakeRequestResult.ErrorCreatingRequest;
					_resultMessage = ex2.GetType().ToString() + ": " + ex2.Message;
					_requestState = Gamespy.Common.RequestState.Complete;
					break;
				}
				_updateRecordRequest.Send();
				_requestState = Gamespy.Common.RequestState.Pending;
				break;
			case Gamespy.Common.RequestState.Pending:
				if (_updateRecordRequest.state != HTTP.RequestState.Done)
				{
					break;
				}
				if (_updateRecordRequest.exception != null)
				{
					_result = SakeRequestResult.ErrorSendingRequest;
				}
				else if (_updateRecordRequest.response.status != 200 && _updateRecordRequest.response.status != 500)
				{
					_result = SakeRequestResult.HttpError;
				}
				else
				{
					string header = _updateRecordRequest.response.GetHeader("Error");
					if (header != string.Empty)
					{
						_result = SakeRequestResult.Error;
						_resultMessage = header;
						_requestState = Gamespy.Common.RequestState.Complete;
						break;
					}
					bool flag = false;
					string text = _updateRecordRequest.response.Text;
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
								case "UpdateRecordResult":
								{
									string value = xmlReader.ReadElementContentAsString();
									_result = (SakeRequestResult)(int)Enum.Parse(typeof(SakeRequestResult), value);
									flag = true;
									break;
								}
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
					if (_updateRecordRequest.response.status == 500)
					{
						_result = SakeRequestResult.HttpError;
					}
					else if (!flag)
					{
						_result = SakeRequestResult.ResponseParseError;
					}
				}
				_requestState = Gamespy.Common.RequestState.Complete;
				break;
			}
			return _requestState;
		}

		public Gamespy.Common.RequestState DeleteRecord(int recordId)
		{
			if (_requestState == Gamespy.Common.RequestState.Complete && _result != SakeRequestResult.ConstructorError)
			{
				_requestState = Gamespy.Common.RequestState.Beginning;
				_result = SakeRequestResult.Success;
				_resultMessage = string.Empty;
			}
			switch (_requestState)
			{
			case Gamespy.Common.RequestState.Beginning:
				try
				{
					_deleteRecordRequest = new Request("POST", _sakeUri);
					BuildDeleteRecordRequest(_deleteRecordRequest, _securityToken, _tableName, recordId);
				}
				catch (Exception ex2)
				{
					_result = SakeRequestResult.ErrorCreatingRequest;
					_resultMessage = ex2.GetType().ToString() + ": " + ex2.Message;
					_requestState = Gamespy.Common.RequestState.Complete;
					break;
				}
				_deleteRecordRequest.Send();
				_requestState = Gamespy.Common.RequestState.Pending;
				break;
			case Gamespy.Common.RequestState.Pending:
				if (_deleteRecordRequest.state != HTTP.RequestState.Done)
				{
					break;
				}
				if (_deleteRecordRequest.exception != null)
				{
					_result = SakeRequestResult.ErrorSendingRequest;
				}
				else if (_deleteRecordRequest.response.status != 200 && _deleteRecordRequest.response.status != 500)
				{
					_result = SakeRequestResult.HttpError;
				}
				else
				{
					string header = _deleteRecordRequest.response.GetHeader("Error");
					if (header != string.Empty)
					{
						_result = SakeRequestResult.Error;
						_resultMessage = header;
						_requestState = Gamespy.Common.RequestState.Complete;
						break;
					}
					bool flag = false;
					string text = _deleteRecordRequest.response.Text;
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
								case "DeleteRecordResult":
								{
									string value = xmlReader.ReadElementContentAsString();
									_result = (SakeRequestResult)(int)Enum.Parse(typeof(SakeRequestResult), value);
									flag = true;
									break;
								}
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
					if (_deleteRecordRequest.response.status == 500)
					{
						_result = SakeRequestResult.HttpError;
					}
					else if (!flag)
					{
						_result = SakeRequestResult.ResponseParseError;
					}
				}
				_requestState = Gamespy.Common.RequestState.Complete;
				break;
			}
			return _requestState;
		}

		public Gamespy.Common.RequestState Search(string[] fieldNames, string sqlStyleFilter, string sqlStyleSort, int[] profileIds, int numRecordsPerPage, int pageNumber)
		{
			if (_requestState == Gamespy.Common.RequestState.Complete && _result != SakeRequestResult.ConstructorError)
			{
				_requestState = Gamespy.Common.RequestState.Beginning;
				_result = SakeRequestResult.Success;
				_search_Records = new List<Record>();
				_resultMessage = string.Empty;
			}
			switch (_requestState)
			{
			case Gamespy.Common.RequestState.Beginning:
				if (sqlStyleFilter == null)
				{
					sqlStyleFilter = string.Empty;
				}
				if (sqlStyleSort == null)
				{
					sqlStyleSort = string.Empty;
				}
				try
				{
					_searchRequest = new Request("POST", _sakeUri);
					BuildSearchRequest(_searchRequest, _securityToken, _tableName, fieldNames, sqlStyleFilter, sqlStyleSort, profileIds, numRecordsPerPage, pageNumber);
				}
				catch (Exception ex2)
				{
					_result = SakeRequestResult.ErrorCreatingRequest;
					_resultMessage = ex2.GetType().ToString() + ": " + ex2.Message;
					_requestState = Gamespy.Common.RequestState.Complete;
					break;
				}
				_searchRequest.Send();
				_requestState = Gamespy.Common.RequestState.Pending;
				break;
			case Gamespy.Common.RequestState.Pending:
				if (_searchRequest.state != HTTP.RequestState.Done)
				{
					break;
				}
				if (_searchRequest.exception != null)
				{
					_result = SakeRequestResult.ErrorSendingRequest;
				}
				else if (_searchRequest.response.status != 200 && _searchRequest.response.status != 500)
				{
					_result = SakeRequestResult.HttpError;
				}
				else
				{
					string header = _searchRequest.response.GetHeader("Error");
					if (header != string.Empty)
					{
						_result = SakeRequestResult.Error;
						_resultMessage = header;
						_requestState = Gamespy.Common.RequestState.Complete;
						break;
					}
					bool flag = false;
					string text = _searchRequest.response.Text;
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
								case "SearchForRecordsResult":
								{
									string value = xmlReader.ReadElementContentAsString();
									_result = (SakeRequestResult)(int)Enum.Parse(typeof(SakeRequestResult), value);
									flag = true;
									break;
								}
								case "ArrayOfRecordValue":
								{
									List<Field> list = new List<Field>();
									int num = 0;
									while (xmlReader.NodeType != XmlNodeType.EndElement || !(xmlReader.Name == "ArrayOfRecordValue"))
									{
										xmlReader.Read();
										if (!(xmlReader.Name == "RecordValue") || xmlReader.NodeType == XmlNodeType.EndElement)
										{
											continue;
										}
										Field field = new Field();
										field.Name = fieldNames[num];
										if (xmlReader.IsEmptyElement)
										{
											field.Type = FieldType.nullValue;
											field.ValueString = string.Empty;
										}
										else
										{
											xmlReader.Read();
											field.Type = (FieldType)(int)Enum.Parse(typeof(FieldType), xmlReader.Name);
											xmlReader.Read();
											if (xmlReader.Name == "value")
											{
												field.ValueString = xmlReader.ReadElementContentAsString();
											}
											if (field.Type == FieldType.binaryDataValue)
											{
												field.ValueArray = Convert.FromBase64String(field.ValueString);
											}
										}
										list.Add(field);
										num++;
									}
									Record item = new Record(list);
									_search_Records.Add(item);
									break;
								}
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
					if (_searchRequest.response.status == 500)
					{
						_result = SakeRequestResult.HttpError;
					}
					else if (!flag)
					{
						_result = SakeRequestResult.ResponseParseError;
					}
				}
				_requestState = Gamespy.Common.RequestState.Complete;
				break;
			}
			return _requestState;
		}

		public Gamespy.Common.RequestState GetMyRecords(string[] fieldNames)
		{
			if (_requestState == Gamespy.Common.RequestState.Complete && _result != SakeRequestResult.ConstructorError)
			{
				_requestState = Gamespy.Common.RequestState.Beginning;
				_result = SakeRequestResult.Success;
				_getMyRecords_Records = new List<Record>();
				_resultMessage = string.Empty;
			}
			switch (_requestState)
			{
			case Gamespy.Common.RequestState.Beginning:
				try
				{
					_getMyRecordsRequest = new Request("POST", _sakeUri);
					BuildGetMyRecordsRequest(_getMyRecordsRequest, _securityToken, _tableName, fieldNames);
				}
				catch (Exception ex2)
				{
					_result = SakeRequestResult.ErrorCreatingRequest;
					_resultMessage = ex2.GetType().ToString() + ": " + ex2.Message;
					_requestState = Gamespy.Common.RequestState.Complete;
					break;
				}
				_getMyRecordsRequest.Send();
				_requestState = Gamespy.Common.RequestState.Pending;
				break;
			case Gamespy.Common.RequestState.Pending:
				if (_getMyRecordsRequest.state != HTTP.RequestState.Done)
				{
					break;
				}
				if (_getMyRecordsRequest.exception != null)
				{
					_result = SakeRequestResult.ErrorSendingRequest;
				}
				else if (_getMyRecordsRequest.response.status != 200 && _getMyRecordsRequest.response.status != 500)
				{
					_result = SakeRequestResult.HttpError;
				}
				else
				{
					string header = _getMyRecordsRequest.response.GetHeader("Error");
					if (header != string.Empty)
					{
						_result = SakeRequestResult.Error;
						_resultMessage = header;
						_requestState = Gamespy.Common.RequestState.Complete;
						break;
					}
					bool flag = false;
					string text = _getMyRecordsRequest.response.Text;
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
								case "GetMyRecordsResult":
								{
									string value = xmlReader.ReadElementContentAsString();
									_result = (SakeRequestResult)(int)Enum.Parse(typeof(SakeRequestResult), value);
									flag = true;
									break;
								}
								case "ArrayOfRecordValue":
								{
									List<Field> list = new List<Field>();
									int num = 0;
									while (xmlReader.NodeType != XmlNodeType.EndElement || !(xmlReader.Name == "ArrayOfRecordValue"))
									{
										xmlReader.Read();
										if (!(xmlReader.Name == "RecordValue") || xmlReader.NodeType == XmlNodeType.EndElement)
										{
											continue;
										}
										Field field = new Field();
										field.Name = fieldNames[num];
										if (xmlReader.IsEmptyElement)
										{
											field.Type = FieldType.nullValue;
											field.ValueString = string.Empty;
										}
										else
										{
											xmlReader.Read();
											field.Type = (FieldType)(int)Enum.Parse(typeof(FieldType), xmlReader.Name);
											xmlReader.Read();
											if (xmlReader.Name == "value")
											{
												field.ValueString = xmlReader.ReadElementContentAsString();
											}
											if (field.Type == FieldType.binaryDataValue)
											{
												field.ValueArray = Convert.FromBase64String(field.ValueString);
											}
										}
										list.Add(field);
										num++;
									}
									Record item = new Record(list);
									_getMyRecords_Records.Add(item);
									break;
								}
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
					if (_getMyRecordsRequest.response.status == 500)
					{
						_result = SakeRequestResult.HttpError;
					}
					else if (!flag)
					{
						_result = SakeRequestResult.ResponseParseError;
					}
				}
				_requestState = Gamespy.Common.RequestState.Complete;
				break;
			}
			return _requestState;
		}

		public Gamespy.Common.RequestState ReadAndLockRecord(int ownerId)
		{
			if (_requestState == Gamespy.Common.RequestState.Complete && _result != SakeRequestResult.ConstructorError)
			{
				_requestState = Gamespy.Common.RequestState.Beginning;
				_result = SakeRequestResult.Success;
				_resultMessage = string.Empty;
			}
			switch (_requestState)
			{
			case Gamespy.Common.RequestState.Beginning:
				try
				{
					_readAndLockRecordRequest = new Request("POST", _sakeLockMethodsUri);
					BuildReadAndLockRecordRequest(_readAndLockRecordRequest, _securityToken, _tableName, ownerId);
				}
				catch (Exception ex2)
				{
					_result = SakeRequestResult.ErrorCreatingRequest;
					_resultMessage = ex2.GetType().ToString() + ": " + ex2.Message;
					_requestState = Gamespy.Common.RequestState.Complete;
					break;
				}
				_readAndLockRecordRequest.Send();
				_requestState = Gamespy.Common.RequestState.Pending;
				break;
			case Gamespy.Common.RequestState.Pending:
				if (_readAndLockRecordRequest.state != HTTP.RequestState.Done)
				{
					break;
				}
				if (_readAndLockRecordRequest.exception != null)
				{
					_result = SakeRequestResult.ErrorSendingRequest;
				}
				else if (_readAndLockRecordRequest.response.status != 200 && _readAndLockRecordRequest.response.status != 500)
				{
					_result = SakeRequestResult.HttpError;
				}
				else
				{
					string header = _readAndLockRecordRequest.response.GetHeader("Error");
					if (header != string.Empty)
					{
						_result = SakeRequestResult.Error;
						_resultMessage = header;
						_requestState = Gamespy.Common.RequestState.Complete;
						break;
					}
					bool flag = false;
					string text = _readAndLockRecordRequest.response.Text;
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
								case "ExecuteAdapterResult":
								{
									string value = xmlReader.ReadElementContentAsString();
									_result = (SakeRequestResult)(int)Enum.Parse(typeof(SakeRequestResult), value);
									flag = true;
									break;
								}
								case "Records":
								{
									if (xmlReader.IsEmptyElement)
									{
										_readAndLock_Record = null;
										xmlReader.Read();
										break;
									}
									xmlReader.Read();
									if (!(xmlReader.Name == "Record") || xmlReader.IsEmptyElement)
									{
										break;
									}
									xmlReader.Read();
									if (!(xmlReader.Name == "Columns") || xmlReader.IsEmptyElement || xmlReader.NodeType == XmlNodeType.EndElement)
									{
										break;
									}
									List<Field> list = new List<Field>();
									while (!(xmlReader.Name == "Columns") || xmlReader.NodeType != XmlNodeType.EndElement)
									{
										xmlReader.Read();
										if (xmlReader.Name == "anyType" && !xmlReader.IsEmptyElement && xmlReader.NodeType != XmlNodeType.EndElement)
										{
											Field field = new Field();
											xmlReader.Read();
											if (xmlReader.Name == "Name")
											{
												field.Name = xmlReader.ReadElementContentAsString();
											}
											if (xmlReader.Name == "Value")
											{
												field.ValueString = xmlReader.ReadElementContentAsString();
											}
											if (field.Name.ToLower() == "result" && field.ValueString.ToLower() == "locked")
											{
												_result = SakeRequestResult.RecordLocked;
											}
											list.Add(field);
										}
									}
									_readAndLock_Record = new Record(list);
									break;
								}
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
					if (_readAndLockRecordRequest.response.status == 500)
					{
						_result = SakeRequestResult.HttpError;
					}
					else if (!flag)
					{
						_result = SakeRequestResult.ResponseParseError;
					}
				}
				_requestState = Gamespy.Common.RequestState.Complete;
				break;
			}
			return _requestState;
		}

		public Gamespy.Common.RequestState UpdateAndUnlockRecord(int recordId, Record recordToUpdate)
		{
			if (_requestState == Gamespy.Common.RequestState.Complete && _result != SakeRequestResult.ConstructorError)
			{
				_requestState = Gamespy.Common.RequestState.Beginning;
				_result = SakeRequestResult.Success;
				_resultMessage = string.Empty;
			}
			switch (_requestState)
			{
			case Gamespy.Common.RequestState.Beginning:
				try
				{
					_updateAndUnlockRecordRequest = new Request("POST", _sakeLockMethodsUri);
					BuildUpdateAndUnlockRecordRequest(_updateAndUnlockRecordRequest, _securityToken, _tableName, recordId, recordToUpdate);
				}
				catch (Exception ex2)
				{
					_result = SakeRequestResult.ErrorCreatingRequest;
					_resultMessage = ex2.GetType().ToString() + ": " + ex2.Message;
					_requestState = Gamespy.Common.RequestState.Complete;
					break;
				}
				_updateAndUnlockRecordRequest.Send();
				_requestState = Gamespy.Common.RequestState.Pending;
				break;
			case Gamespy.Common.RequestState.Pending:
				if (_updateAndUnlockRecordRequest.state != HTTP.RequestState.Done)
				{
					break;
				}
				if (_updateAndUnlockRecordRequest.exception != null)
				{
					_result = SakeRequestResult.ErrorSendingRequest;
				}
				else if (_updateAndUnlockRecordRequest.response.status != 200 && _updateAndUnlockRecordRequest.response.status != 500)
				{
					_result = SakeRequestResult.HttpError;
				}
				else
				{
					string header = _updateAndUnlockRecordRequest.response.GetHeader("Error");
					if (header != string.Empty)
					{
						_result = SakeRequestResult.Error;
						_resultMessage = header;
						_requestState = Gamespy.Common.RequestState.Complete;
						break;
					}
					bool flag = false;
					string text = _updateAndUnlockRecordRequest.response.Text;
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
								case "ExecuteAdapterResult":
								{
									string value = xmlReader.ReadElementContentAsString();
									_result = (SakeRequestResult)(int)Enum.Parse(typeof(SakeRequestResult), value);
									flag = true;
									break;
								}
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
					if (_updateAndUnlockRecordRequest.response.status == 500)
					{
						_result = SakeRequestResult.HttpError;
					}
					else if (!flag)
					{
						_result = SakeRequestResult.ResponseParseError;
					}
				}
				_requestState = Gamespy.Common.RequestState.Complete;
				break;
			}
			return _requestState;
		}

		public Gamespy.Common.RequestState GetRecordCount(string sqlStyleFilter)
		{
			if (_requestState == Gamespy.Common.RequestState.Complete && _result != SakeRequestResult.ConstructorError)
			{
				_requestState = Gamespy.Common.RequestState.Beginning;
				_result = SakeRequestResult.Success;
				_getRecordCount_Count = -1;
				_resultMessage = string.Empty;
			}
			switch (_requestState)
			{
			case Gamespy.Common.RequestState.Beginning:
				try
				{
					_getRecordCountRequest = new Request("POST", _sakeUri);
					BuildGetRecordCountRequest(_getRecordCountRequest, _securityToken, _tableName, sqlStyleFilter);
				}
				catch (Exception ex2)
				{
					_result = SakeRequestResult.ErrorCreatingRequest;
					_resultMessage = ex2.GetType().ToString() + ": " + ex2.Message;
					_requestState = Gamespy.Common.RequestState.Complete;
					break;
				}
				_getRecordCountRequest.Send();
				_requestState = Gamespy.Common.RequestState.Pending;
				break;
			case Gamespy.Common.RequestState.Pending:
				if (_getRecordCountRequest.state != HTTP.RequestState.Done)
				{
					break;
				}
				if (_getRecordCountRequest.exception != null)
				{
					_result = SakeRequestResult.ErrorSendingRequest;
				}
				else if (_getRecordCountRequest.response.status != 200 && _getRecordCountRequest.response.status != 500)
				{
					_result = SakeRequestResult.HttpError;
				}
				else
				{
					string header = _getRecordCountRequest.response.GetHeader("Error");
					if (header != string.Empty)
					{
						_result = SakeRequestResult.Error;
						_resultMessage = header;
						_requestState = Gamespy.Common.RequestState.Complete;
						break;
					}
					bool flag = false;
					string text = _getRecordCountRequest.response.Text;
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
								case "GetRecordCountResult":
								{
									string value = xmlReader.ReadElementContentAsString();
									_result = (SakeRequestResult)(int)Enum.Parse(typeof(SakeRequestResult), value);
									flag = true;
									break;
								}
								case "count":
									_getRecordCount_Count = xmlReader.ReadElementContentAsInt();
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
					if (_getRecordCountRequest.response.status == 500)
					{
						_result = SakeRequestResult.HttpError;
					}
					else if (!flag)
					{
						_result = SakeRequestResult.ResponseParseError;
					}
				}
				_requestState = Gamespy.Common.RequestState.Complete;
				break;
			}
			return _requestState;
		}

		public PollSearchesRequestState PollSearches(List<string> keyNames, int[] profileIds, int pollInterval)
		{
			if (!(_securityToken.GameName == "MobShootios") && !(_securityToken.GameName == "MobShootand") && !(_securityToken.GameName == "GluTestiph") && !(_securityToken.GameName == "gmtest") && !(_securityToken.GameName == "mobmatchios"))
			{
				_result = SakeRequestResult.ServiceDisabled;
				_resultMessage = "Poll Searching is not a supported feature for your game title";
				return PollSearchesRequestState.Complete;
			}
			switch (_pollSearchesRequestState)
			{
			case PollSearchesRequestState.SearchRequestPending:
			{
				string[] fieldNames = keyNames.ToArray();
				_requestState = Search(fieldNames, string.Empty, string.Empty, profileIds, profileIds.Length, 1);
				if (_result == SakeRequestResult.Success && _requestState == Gamespy.Common.RequestState.Complete)
				{
					_pollSearchesRequestState = PollSearchesRequestState.UpdateReceived;
				}
				break;
			}
			case PollSearchesRequestState.UpdateReceived:
				_pollingStartTime = DateTime.Now.Ticks;
				_pollSearchesRequestState = PollSearchesRequestState.PollingInterval;
				break;
			case PollSearchesRequestState.PollingInterval:
			{
				long ticks = DateTime.Now.Ticks;
				if (ticks - _pollingStartTime > pollInterval * 10000000)
				{
					_pollSearchesRequestState = PollSearchesRequestState.SearchRequestPending;
					_result = SakeRequestResult.Success;
					_pollingStartTime = DateTime.Now.Ticks;
				}
				break;
			}
			case PollSearchesRequestState.CompleteWithError:
				_pollSearchesRequestState = PollSearchesRequestState.Complete;
				break;
			case PollSearchesRequestState.Complete:
				_pollSearchesRequestState = PollSearchesRequestState.SearchRequestPending;
				_requestState = Gamespy.Common.RequestState.Complete;
				break;
			}
			return _pollSearchesRequestState;
		}

		public PollSearchesRequestState StopPollSearches()
		{
			_pollSearchesRequestState = PollSearchesRequestState.Complete;
			return _pollSearchesRequestState;
		}
	}
}
