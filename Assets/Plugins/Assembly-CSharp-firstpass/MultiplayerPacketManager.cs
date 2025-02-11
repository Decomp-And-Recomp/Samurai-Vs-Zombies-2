using System;
using System.IO;
using System.Runtime.CompilerServices;
using Glu.Plugins.ASocial;
using UnityEngine;

public class MultiplayerPacketManager : MonoBehaviour
{
	private static MultiplayerPacketManager instance;

	private bool mInitialized;

	private int mTotalPacketsSent;

	private int mTotalPacketsReceived;

	private float mTimeSincePreviousUpdate;

	private string receivedPacketsFileName = AJavaTools.GameInfo.GetExternalFilesPath() + "/ReceivedData.txt";

	private string sentPacketsFileName = AJavaTools.GameInfo.GetExternalFilesPath() + "/SentData.txt";

	public static MultiplayerPacketManager Instance
	{
		get
		{
			if (instance == null)
			{
				GameObject gameObject = new GameObject("MPM_GO");
				gameObject.AddComponent<MultiplayerPacketManager>();
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
				instance = gameObject.GetComponent<MultiplayerPacketManager>();
			}
			return instance;
		}
	}

	[method: MethodImpl(32)]
	public static event EventHandler<ReceivedPacketEventArgs> onPacketReceived;

	public void Init()
	{
		Packet.SendMessage = (Action<object, byte[], bool>)Delegate.Combine(Packet.SendMessage, new Action<object, byte[], bool>(SendMessage));
		mInitialized = true;
		Reset();
	}

	public void Reset()
	{
		mTotalPacketsSent = 0;
		mTotalPacketsReceived = 0;
		mTimeSincePreviousUpdate = 0f;
		if (Debug.isDebugBuild)
		{
			if (File.Exists(receivedPacketsFileName))
			{
				File.Delete(receivedPacketsFileName);
			}
			if (File.Exists(sentPacketsFileName))
			{
				File.Delete(sentPacketsFileName);
			}
			File.Create(receivedPacketsFileName).Close();
			File.Create(sentPacketsFileName).Close();
		}
	}

	private void Awake()
	{
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (mInitialized && Time.realtimeSinceStartup - mTimeSincePreviousUpdate > 5f)
		{
			mTimeSincePreviousUpdate = Time.realtimeSinceStartup;
			if (Debug.isDebugBuild)
			{
				Debug.LogWarning("mTotalPacketsReceived = " + mTotalPacketsReceived + " mTotalPacketsSent = " + mTotalPacketsSent);
			}
		}
	}

	private void OnDestory()
	{
		Packet.SendMessage = (Action<object, byte[], bool>)Delegate.Remove(Packet.SendMessage, new Action<object, byte[], bool>(SendMessage));
	}

	private void SendMessage(object sender, byte[] data, bool reliable)
	{
		mTotalPacketsSent++;
		if (Debug.isDebugBuild)
		{
			Packet packet = (Packet)sender;
			File.AppendAllText(sentPacketsFileName, string.Concat("[", mTotalPacketsSent, " ,", DateTime.Now, "] GO=", packet.GetGameObjectName(), " COMP=", packet.GetComponentName(), " Method=", packet.GetMethodName(), " GUID=", packet.NetworkOwnerIdentifier(), " Params=", packet.GetParameters(), "\n\n"));
		}
	}

	private void onMPMessageReceived(object sender, MessageArgs args)
	{
		mTotalPacketsReceived++;
		ReceivedPacketEventArgs receivedPacketEventArgs = new ReceivedPacketEventArgs(Packet.BuildPacketToRead(args.GetMessage()));
		MultiplayerPacketManager.onPacketReceived(this, receivedPacketEventArgs);
		if (Debug.isDebugBuild)
		{
			Packet packet = receivedPacketEventArgs.GetPacket();
			File.AppendAllText(receivedPacketsFileName, string.Concat("[", mTotalPacketsReceived, " ,", DateTime.Now, "] GO=", packet.GetGameObjectName(), " COMP=", packet.GetComponentName(), " Method=", packet.GetMethodName(), " GUID=", packet.NetworkOwnerIdentifier(), " Params=", packet.GetParameters(), "\n\n"));
		}
	}
}
