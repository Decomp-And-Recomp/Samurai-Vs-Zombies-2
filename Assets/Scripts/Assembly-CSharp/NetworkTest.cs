using System.Collections;
using UnityEngine;

public class NetworkTest : MonoBehaviour
{
	public bool doLogin;

	private bool doCollectionIDFill;

	public bool doSeedRandomCollection;

	public bool doRetrieveMyCollection;

	public string[] collectionItems;

	public bool doSaveUserData;

	public bool doLoadUserData;

	public int itemToChallengeFor;

	public bool doFindPotentialChallenges;

	public bool doReportChallengeSuccess;

	public bool doReportDefenseSuccess;

	public bool doRemoveItem;

	private void Awake()
	{
		if (!Singleton<Profile>.Exists)
		{
			StartCoroutine(Singleton<Profile>.Instance.Init());
		}
	}

	private void Start()
	{
		doLogin = true;
	}

	private void Update()
	{
		if (doLogin)
		{
			doLogin = false;
		}
		if (doCollectionIDFill)
		{
			doCollectionIDFill = false;
			StartCoroutine(FillCollectionIDTable());
		}
		if (doSaveUserData)
		{
			doSaveUserData = false;
			Singleton<Profile>.Instance.MultiplayerData.Save();
		}
		if (doLoadUserData)
		{
			doLoadUserData = false;
		}
		if (doFindPotentialChallenges)
		{
			doFindPotentialChallenges = false;
		}
		if (doReportChallengeSuccess)
		{
			doReportChallengeSuccess = false;
		}
		if (doReportDefenseSuccess)
		{
			doReportDefenseSuccess = false;
		}
		if (doRemoveItem)
		{
			doRemoveItem = false;
		}
		if (doSeedRandomCollection)
		{
			doSeedRandomCollection = false;
		}
		if (doRetrieveMyCollection)
		{
			doRetrieveMyCollection = false;
		}
	}

	private IEnumerator FillCollectionIDTable()
	{
		GripField[] fields = new GripField[3]
		{
			new GripField("collectionID", GripField.GripFieldType.Int),
			new GripField("collection", GripField.GripFieldType.Byte),
			new GripField("item", GripField.GripFieldType.Byte)
		};
		int collectionID = 0;
		CollectionSchema[] collections = DataBundleRuntime.Instance.InitializeRecords<CollectionSchema>("Collections");
		for (int collection = 0; collection < collections.Length; collection++)
		{
			fields[1].mByte = (sbyte)collection;
			CollectionItemSchema[] items = collections[collection].items.InitializeRecords<CollectionItemSchema>();
			for (int item = 0; item < items.Length; item++)
			{
				fields[0].mInt = collectionID++;
				fields[2].mByte = (sbyte)item;
				bool createComplete = false;
				GripNetwork.CreateRecord("CollectionID", fields, delegate
				{
					createComplete = true;
				});
				while (!createComplete)
				{
					yield return null;
				}
			}
		}
	}
}
