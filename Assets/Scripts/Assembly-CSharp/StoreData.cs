using System;
using System.Collections.Generic;
using UnityEngine;

public class StoreData
{
	public class Item : IDisposable
	{
		public string id = string.Empty;

		public string iconPath;

		public string title = string.Empty;

		public Details details = new Details();

		public string unlockTitle = string.Empty;

		public string unlockCondition = string.Empty;

		public bool isFoundInPresent;

		public List<string> containedInPresent = new List<string>();

		public List<string> bundleContent = new List<string>();

		public int amount = 1;

		public Cost cost;

		public bool isEvent;

		public bool locked;

		public bool maxlevel;

		public bool isConsumable;

		public bool isNew;

		public int unlockAtWave;

		public int availableAtWave;

		public bool triggersOwnPopup;

		public Action customButtonAction;

		public int packAmount;

		public DealPackSchema dealPack;

		public Cost packCost;

		public bool packShowBonus = true;

		public int packDiscount = 20;

		public Action packPurchaseFunc;

		public Action packOverrideFunc;

		public Cost cost3;

		public string analyticsEvent;

		public Dictionary<string, object> analyticsParams;

		public int _originalSortIndex;

		public int _sortGroup;

		private Action mPurchaseFunc;

		private bool alreadyDisposed;

		public bool isNovelty
		{
			get
			{
				foreach (string novelty in Singleton<Profile>.Instance.novelties)
				{
					if (novelty == id)
					{
						return true;
					}
				}
				return false;
			}
		}

		public Texture2D icon { get; set; }

		public Texture2D secondIcon { get; set; }

		public Item(Action execFunc)
		{
			mPurchaseFunc = execFunc;
		}

		public void Apply(int packId)
		{
			if (mPurchaseFunc != null)
			{
				if (!SingletonSpawningMonoBehaviour<ApplicationUtilities>.Instance.SNTPTime.SNTPSuccessful && id == "mpshield")
				{
					return;
				}
				if (id == "mpshield")
				{
					if (packId == 2 && packPurchaseFunc != null)
					{
						packPurchaseFunc();
					}
					else if (packId == 1 && packOverrideFunc != null)
					{
						packOverrideFunc();
					}
					else
					{
						mPurchaseFunc();
					}
				}
				else
				{
					mPurchaseFunc();
				}
			}
			else if (dealPack != null)
			{
				StoreAvailability.CashInDealPack(dealPack);
			}
			bool flag = cost.currency == Cost.Currency.Hard;
			string st = ((!flag) ? "SC" : "HC");
			string st2 = ((!flag) ? "SC_PURCHASE" : "HC_PURCHASE");
			string text = ((analyticsParams == null || !analyticsParams.ContainsKey("ItemName")) ? id : analyticsParams["ItemName"].ToString());
			if (!string.IsNullOrEmpty(analyticsEvent))
			{
				if (analyticsParams == null)
				{
					analyticsParams = new Dictionary<string, object>();
				}
				analyticsParams["Cost"] = cost.price;
				analyticsParams["Currency"] = cost.currencyAnalyticCode;
				analyticsParams["PlayerLevel"] = Singleton<Profile>.Instance.playerLevel;
				analyticsParams["WaveNumber"] = Singleton<Profile>.Instance.wave_SinglePlayerGame;
				analyticsParams["MPWavesWon"] = Singleton<Profile>.Instance.mpWavesWon;
				Singleton<Analytics>.Instance.LogEvent(analyticsEvent, analyticsParams);
				Singleton<Analytics>.Instance.KontagentEvent(text, st2, analyticsEvent, Singleton<Profile>.Instance.wave_SinglePlayerGame, cost.price, Analytics.KParam("PlayerLevel", Singleton<Profile>.Instance.playerLevel.ToString()), Analytics.KParam("MPWavesWon", Singleton<Profile>.Instance.mpWavesWon.ToString()));
			}
			if (dealPack != null)
			{
				Singleton<Analytics>.Instance.KontagentEvent(text, "DealPackPurchased", st, Singleton<Profile>.Instance.wave_SinglePlayerGame, cost.price, Analytics.KParam("PlayerLevel", Singleton<Profile>.Instance.playerLevel.ToString()), Analytics.KParam("MPWavesWon", Singleton<Profile>.Instance.mpWavesWon.ToString()), Analytics.KParam("PackContents", dealPack.items));
			}
			if (flag && Singleton<Profile>.Instance.JustMadeHC_IAP)
			{
				Singleton<Profile>.Instance.JustMadeHC_IAP = false;
				Singleton<Analytics>.Instance.KontagentEvent(text, "FirstPurchaseFollowingHC_IAP", "HC", Singleton<Profile>.Instance.wave_SinglePlayerGame, cost.price, Analytics.KParam("PlayerLevel", Singleton<Profile>.Instance.playerLevel.ToString()), Analytics.KParam("MPWavesWon", Singleton<Profile>.Instance.mpWavesWon.ToString()));
			}
			else if (!flag && Singleton<Profile>.Instance.JustMadeSC_IAP)
			{
				Singleton<Profile>.Instance.JustMadeSC_IAP = false;
				Singleton<Analytics>.Instance.KontagentEvent(text, "FirstPurchaseFollowingSC_IAP", "SC", Singleton<Profile>.Instance.wave_SinglePlayerGame, cost.price, Analytics.KParam("PlayerLevel", Singleton<Profile>.Instance.playerLevel.ToString()), Analytics.KParam("MPWavesWon", Singleton<Profile>.Instance.mpWavesWon.ToString()));
			}
			if (analyticsParams != null && analyticsParams.ContainsKey("ItemName"))
			{
				Singleton<Analytics>.Instance.KontagentEvent("Sink", "SINK_SOURCE", st, Singleton<Profile>.Instance.wave_SinglePlayerGame, -amount, Analytics.KParam("SinkName", text));
			}
		}

		public override bool Equals(object o)
		{
			if (o == null)
			{
				return false;
			}
			Item item = (Item)o;
			return icon == item.icon && title == item.title && details == item.details && cost == item.cost;
		}

		public override int GetHashCode()
		{
			return icon.GetHashCode() + title.GetHashCode() + details.GetHashCode() + cost.GetHashCode();
		}

		public void LoadIcon(string path)
		{
			SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource(path, 1);
			if (cachedResource != null)
			{
				icon = cachedResource.Resource as Texture2D;
				iconPath = path;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool isDisposing)
		{
			if (!alreadyDisposed)
			{
				if (isDisposing)
				{
				}
				icon = null;
				alreadyDisposed = true;
			}
		}

		~Item()
		{
			Dispose(false);
		}

		public static bool operator ==(Item a, Item b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(Item a, Item b)
		{
			return !a.Equals(b);
		}
	}

	public class ItemsListSorter : IComparer<Item>
	{
		public int Compare(Item a, Item b)
		{
			if (a._sortGroup == b._sortGroup)
			{
				if (a.locked == b.locked)
				{
					if (a.unlockAtWave == b.unlockAtWave)
					{
						return a._originalSortIndex - b._originalSortIndex;
					}
					return a.unlockAtWave - b.unlockAtWave;
				}
				if (a.locked)
				{
					return 1;
				}
				return -1;
			}
			return a._sortGroup - b._sortGroup;
		}
	}

	public class Details
	{
		private List<KeyValuePair<string, string[]>> mStats = new List<KeyValuePair<string, string[]>>();

		private string mDescription = string.Empty;

		private bool mDescriptionIsSmall;

		private string leftColumnTitle = string.Empty;

		private string rightColumnTitle = string.Empty;

		public string Name { get; set; }

		public string Description
		{
			get
			{
				return mDescription;
			}
		}

		public bool DescriptionIsSmall
		{
			get
			{
				return mDescriptionIsSmall;
			}
		}

		public List<KeyValuePair<string, string[]>> Stats
		{
			get
			{
				return mStats;
			}
		}

		public int LevelA { get; set; }

		public int LevelB { get; set; }

		public string ColumnTitleA
		{
			get
			{
				return leftColumnTitle;
			}
		}

		public string ColumnTitleB
		{
			get
			{
				return rightColumnTitle;
			}
		}

		public int? MaxLevel { get; set; }

		public int? Count { get; set; }

		public bool UpgradeDisplayAsText { get; set; }

		public void AddStat(string iconFile, string val1, string val2)
		{
			if (!(val1 == "0") || !(val2 == "0"))
			{
				string[] value = new string[2] { val1, val2 };
				mStats.Add(new KeyValuePair<string, string[]>(iconFile, value));
			}
		}

		public void AddDescription(string description)
		{
			mDescription = description;
			mDescriptionIsSmall = false;
		}

		public void AddSmallDescription(string description)
		{
			mDescription = description;
			mDescriptionIsSmall = true;
		}

		public void SetColumns(int leftLevel, int rightLevel)
		{
			leftColumnTitle = string.Format(StringUtils.GetStringFromStringRef("MenuFixedStrings", "stat_level"), leftLevel.ToString());
			rightColumnTitle = string.Format(StringUtils.GetStringFromStringRef("MenuFixedStrings", "stat_level"), rightLevel.ToString());
			LevelA = leftLevel;
			LevelB = rightLevel;
		}
	}
}
