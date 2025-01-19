using System;
using System.IO;
using System.Text;
using Gamespy.Authentication;
using Gamespy.Common;
using HTTP;

namespace Gamespy.CloudStorage
{
	public class CloudFile
	{
		private Account.AuthSecurityToken _securityToken;

		private string _sakeUploadUri = ".sake.gamespy.com/sakefileserver/uploadstream.aspx";

		private string _sakeDownloadUri = ".sake.gamespy.com/SakeFileServer/download.aspx";

		private static int metrics_TotalUploads;

		private static int metrics_TotalUpdates;

		private static int metrics_TotalDownloads;

		private static float metrics_TimeSinceAuth;

		private static float metrics_AverageUploadsPerMinute;

		private static float metrics_AverageUpdatesPerMinute;

		private static float metrics_AverageDownloadsPerMinute;

		private static string filePath_LogCallMetrics;

		private static string filePath_LogRequests;

		private SakeRequestResult _result;

		private string _resultMessage;

		private string _fileId;

		private byte[] _fileData;

		private Gamespy.Common.RequestState _requestState;

		private Request _uploadFileRequest;

		private Request _downloadFileRequest;

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

		public string Upload_FileId
		{
			get
			{
				return _fileId;
			}
		}

		public byte[] Download_FileData
		{
			get
			{
				return _fileData;
			}
			set
			{
				_fileData = value;
			}
		}

		public CloudFile(Account.AuthSecurityToken securityToken, byte[] fileData)
		{
			if (string.IsNullOrEmpty(securityToken.GameName))
			{
				_result = SakeRequestResult.ConstructorError;
				_resultMessage = "AuthSecurityToken was not populated - ensure Authenticate is successful before constructing a CloudFile";
				_requestState = Gamespy.Common.RequestState.Complete;
				return;
			}
			_securityToken = securityToken;
			_fileData = fileData;
			if (Type.GetType("Mono.Runtime") != null && Debug.isDebugBuild)
			{
				_sakeUploadUri = "http://" + _securityToken.GameName + _sakeUploadUri;
			}
			else
			{
				_sakeUploadUri = "https://" + _securityToken.GameName + _sakeUploadUri;
			}
		}

		public CloudFile(Account.AuthSecurityToken securityToken, byte[] fileData, string fileId)
		{
			if (string.IsNullOrEmpty(securityToken.GameName))
			{
				_result = SakeRequestResult.ConstructorError;
				_resultMessage = "AuthSecurityToken was not populated - ensure Authenticate is successful before constructing a CloudFile";
				_requestState = Gamespy.Common.RequestState.Complete;
				return;
			}
			_securityToken = securityToken;
			_fileData = fileData;
			_fileId = fileId;
			if (Type.GetType("Mono.Runtime") != null && Debug.isDebugBuild)
			{
				_sakeUploadUri = "http://" + _securityToken.GameName + _sakeUploadUri;
			}
			else
			{
				_sakeUploadUri = "https://" + _securityToken.GameName + _sakeUploadUri;
			}
		}

		public CloudFile(Account.AuthSecurityToken securityToken, string fileId)
		{
			if (string.IsNullOrEmpty(securityToken.GameName))
			{
				_result = SakeRequestResult.ConstructorError;
				_resultMessage = "AuthSecurityToken was not populated - ensure Authenticate is successful before constructing a CloudFile";
				_requestState = Gamespy.Common.RequestState.Complete;
				return;
			}
			_securityToken = securityToken;
			_fileId = fileId;
			if (Type.GetType("Mono.Runtime") != null && Debug.isDebugBuild)
			{
				_sakeDownloadUri = "http://" + _securityToken.GameName + _sakeDownloadUri + "?fileid=" + _fileId + "&gameid=" + securityToken.GameId + "&pid=" + securityToken.ProfileId;
			}
			else
			{
				_sakeDownloadUri = "https://" + _securityToken.GameName + _sakeDownloadUri + "?fileid=" + _fileId + "&gameid=" + securityToken.GameId + "&pid=" + securityToken.ProfileId;
			}
		}

		private void BuildUploadRequest(Request uploadFileRequest, Account.AuthSecurityToken securityToken)
		{
			string text = Guid.NewGuid().ToString();
			string text2 = securityToken.GameId.ToString();
			string text3 = securityToken.ProfileId.ToString();
			string text4 = "---------------------------" + DateTime.Now.Ticks.ToString("x");
			string sessionToken = securityToken.SessionToken;
			byte[] bytes = Encoding.ASCII.GetBytes("\r\n--" + text4 + "\r\n");
			byte[] bytes2 = Encoding.ASCII.GetBytes("\r\n--" + text4 + "--\r\n");
			byte[] bytes3 = Encoding.ASCII.GetBytes("Content-Disposition: form-data; name=\"SakeStreamID\"\r\n\r\n" + text);
			byte[] bytes4 = Encoding.ASCII.GetBytes("Content-Disposition: form-data; name=\"gameid\"\r\n\r\n" + text2);
			byte[] bytes5 = Encoding.ASCII.GetBytes("Content-Disposition: form-data; name=\"pid\"\r\n\r\n" + text3);
			byte[] buffer = null;
			if (_fileId != null)
			{
				buffer = Encoding.ASCII.GetBytes("Content-Disposition: form-data; name=\"fileid\"\r\n\r\n" + _fileId);
			}
			byte[] bytes6 = Encoding.ASCII.GetBytes("Content-Disposition: form-data; name=\"file\"; filename=\"binfile.dat\"\r\nContent-Type: application/octet-stream\r\n\r\n");
			uploadFileRequest.AddHeader("Content-Type", "multipart/form-data; boundary=" + text4);
			uploadFileRequest.AddHeader("GameID", text2);
			uploadFileRequest.AddHeader("ProfileID", text3);
			uploadFileRequest.AddHeader("SessionToken", sessionToken);
			MemoryStream output = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(output);
			binaryWriter.Write(bytes);
			binaryWriter.Write(bytes3);
			binaryWriter.Write(bytes);
			binaryWriter.Write(bytes4);
			binaryWriter.Write(bytes);
			binaryWriter.Write(bytes5);
			if (_fileId != null)
			{
				binaryWriter.Write(bytes);
				binaryWriter.Write(buffer);
			}
			binaryWriter.Write(bytes);
			binaryWriter.Write(bytes6);
			binaryWriter.Write(_fileData);
			binaryWriter.Write(bytes2);
			binaryWriter.Seek(0, SeekOrigin.Begin);
			byte[] array = new byte[(int)binaryWriter.BaseStream.Length];
			binaryWriter.BaseStream.Read(array, 0, (int)binaryWriter.BaseStream.Length);
			uploadFileRequest.bytes = array;
		}

		public Gamespy.Common.RequestState Upload()
		{
			if (_requestState != Gamespy.Common.RequestState.Complete || _result != SakeRequestResult.ConstructorError)
			{
			}
			switch (_requestState)
			{
			case Gamespy.Common.RequestState.Beginning:
				if (_fileData == null)
				{
					_result = SakeRequestResult.NoFileDataError;
					_requestState = Gamespy.Common.RequestState.Complete;
					break;
				}
				try
				{
					_uploadFileRequest = new Request("POST", _sakeUploadUri);
					BuildUploadRequest(_uploadFileRequest, _securityToken);
				}
				catch (Exception ex)
				{
					_result = SakeRequestResult.ErrorCreatingRequest;
					_resultMessage = ex.GetType().ToString() + ": " + ex.Message;
					_requestState = Gamespy.Common.RequestState.Complete;
					break;
				}
				_uploadFileRequest.Send();
				_requestState = Gamespy.Common.RequestState.Pending;
				break;
			case Gamespy.Common.RequestState.Pending:
				if (_uploadFileRequest.state != HTTP.RequestState.Done)
				{
					break;
				}
				if (_uploadFileRequest.exception != null)
				{
					_result = SakeRequestResult.ErrorSendingRequest;
				}
				else if (_uploadFileRequest.response.status != 200)
				{
					_result = SakeRequestResult.HttpError;
				}
				else
				{
					string header = _uploadFileRequest.response.GetHeader("Error");
					if (header != string.Empty)
					{
						_result = SakeRequestResult.Error;
						_resultMessage = header;
						_requestState = Gamespy.Common.RequestState.Complete;
						break;
					}
					string header2 = _uploadFileRequest.response.GetHeader("Sake-File-Result");
					if (header2 == "0")
					{
						_fileId = _uploadFileRequest.response.GetHeader("Sake-File-Id");
					}
					else
					{
						_result = SakeRequestResult.Error;
					}
				}
				_requestState = Gamespy.Common.RequestState.Complete;
				break;
			}
			return _requestState;
		}

		private void BuildDownloadRequest(Request downloadFileRequest, Account.AuthSecurityToken securityToken)
		{
			string value = securityToken.GameId.ToString();
			string value2 = securityToken.ProfileId.ToString();
			string sessionToken = securityToken.SessionToken;
			downloadFileRequest.AddHeader("GameID", value);
			downloadFileRequest.AddHeader("ProfileID", value2);
			downloadFileRequest.AddHeader("SessionToken", sessionToken);
		}

		public Gamespy.Common.RequestState Download()
		{
			if (_requestState != Gamespy.Common.RequestState.Complete || _result != SakeRequestResult.ConstructorError)
			{
			}
			switch (_requestState)
			{
			case Gamespy.Common.RequestState.Beginning:
				if (_fileId == null)
				{
					_result = SakeRequestResult.NoFileDataError;
					_requestState = Gamespy.Common.RequestState.Complete;
					break;
				}
				try
				{
					_downloadFileRequest = new Request("GET", _sakeDownloadUri, true);
					BuildDownloadRequest(_downloadFileRequest, _securityToken);
				}
				catch (Exception ex)
				{
					_result = SakeRequestResult.ErrorCreatingRequest;
					_resultMessage = ex.GetType().ToString() + ": " + ex.Message;
					_requestState = Gamespy.Common.RequestState.Complete;
					break;
				}
				_downloadFileRequest.Send();
				_requestState = Gamespy.Common.RequestState.Pending;
				break;
			case Gamespy.Common.RequestState.Pending:
				if (_downloadFileRequest.state != HTTP.RequestState.Done)
				{
					break;
				}
				if (_downloadFileRequest.exception != null)
				{
					_result = SakeRequestResult.ErrorSendingRequest;
				}
				else if (_downloadFileRequest.response.status != 200)
				{
					_result = SakeRequestResult.HttpError;
				}
				else
				{
					string header = _downloadFileRequest.response.GetHeader("Error");
					if (header != string.Empty)
					{
						_result = SakeRequestResult.Error;
						_resultMessage = header;
						_requestState = Gamespy.Common.RequestState.Complete;
						break;
					}
					string header2 = _downloadFileRequest.response.GetHeader("Sake-File-Result");
					if (header2 == "0")
					{
						_fileData = _downloadFileRequest.response.bytes;
					}
					else
					{
						_result = SakeRequestResult.Error;
					}
				}
				_requestState = Gamespy.Common.RequestState.Complete;
				break;
			}
			return _requestState;
		}
	}
}
