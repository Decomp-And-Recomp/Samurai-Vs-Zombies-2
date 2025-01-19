using System.Collections.Generic;
using UnityEngine;

public class HUDConsumables : UIHandlerComponent
{
	private class Card
	{
		private GameObject mParentObject;

		private GluiStandardButtonContainer mButtonRef;

		private GluiSprite mIconRef;

		private GluiText mTextRefQty;

		private float mCooldownMax;

		private float mCooldownCurrent;

		public string consumeID = string.Empty;

		private int quantity = -1;

		private bool wasAvailable;

		public bool isAvailable
		{
			get
			{
				return mCooldownCurrent >= mCooldownMax && Singleton<Profile>.Instance.GetNumPotions(consumeID) > 0;
			}
		}

		public Card(GameObject uiParent, string uiID, string consumeID, float cooldownMax)
		{
			this.consumeID = consumeID;
			mCooldownMax = cooldownMax;
			PotionSchema potionSchema = Singleton<PotionsDatabase>.Instance[consumeID];
			mParentObject = uiParent.FindChild("HUD_" + uiID);
			mButtonRef = mParentObject.FindChildComponent<GluiStandardButtonContainer>("Button_" + uiID);
			mIconRef = mParentObject.FindChildComponent<GluiSprite>("Art_" + uiID);
			mTextRefQty = mParentObject.FindChildComponent<GluiText>("Text_Qty");
			mIconRef.Texture = potionSchema.icon;
			WeakGlobalMonoBehavior<HUD>.Instance.RegisterOnPressEvent(mButtonRef, "CONSUME:" + consumeID);
			if (!isAvailable)
			{
				mButtonRef.Locked = true;
			}
		}

		public void Update(bool updateExpensiveVisuals)
		{
			mCooldownCurrent = Mathf.Min(mCooldownCurrent + Time.deltaTime, mCooldownMax);
			int numPotions = Singleton<Profile>.Instance.GetNumPotions(consumeID);
			if (quantity != numPotions)
			{
				quantity = numPotions;
				mTextRefQty.Text = numPotions.ToString();
			}
			bool flag = isAvailable;
			if (wasAvailable != flag)
			{
				wasAvailable = flag;
				mButtonRef.Locked = !flag;
			}
		}

		public void Hide()
		{
			mParentObject.SetActive(false);
		}

		public void Execute()
		{
			if (!(WeakGlobalMonoBehavior<InGameImpl>.Instance.hero.health > 0f))
			{
				return;
			}
			if (isAvailable)
			{
				if (Singleton<PotionsDatabase>.Instance.Execute(consumeID))
				{
					Singleton<Profile>.Instance.SetNumPotions(consumeID, Singleton<Profile>.Instance.GetNumPotions(consumeID) - 1);
					mCooldownCurrent = 0f;
					Singleton<Profile>.Instance.globalPlayerRating += 20;
					switch (consumeID)
					{
					case "healthPotion":
						Singleton<PlayStatistics>.Instance.data.sushiUsed++;
						Singleton<Achievements>.Instance.IncrementAchievement("UseSushi1", 1);
						Singleton<Achievements>.Instance.IncrementAchievement("UseSushi2", 1);
						break;
					case "leadershipPotion":
						Singleton<PlayStatistics>.Instance.data.teaUsed++;
						Singleton<Achievements>.Instance.IncrementAchievement("UseTea1", 1);
						Singleton<Achievements>.Instance.IncrementAchievement("UseTea2", 1);
						break;
					}
				}
			}
			else if (Singleton<Profile>.Instance.wave_SinglePlayerGame != 1 || Singleton<Profile>.Instance.GetWaveLevel(1) > 1 || Singleton<Profile>.Instance.inMultiplayerWave)
			{
				WeakGlobalMonoBehavior<InGameImpl>.Instance.gamePaused = true;
				GluiActionSender.SendGluiAction("POPUP_CONFIRMPURCHASE", WeakGlobalMonoBehavior<HUD>.Instance.gameObject, StoreAvailability.GetPotion(consumeID));
			}
		}
	}

	private const string kConsumeCmd = "CONSUME:";

	private List<Card> mCards = new List<Card>(2);

	public HUDConsumables(GameObject uiParent)
	{
		mCards.Add(new Card(uiParent, "Sushi", "healthPotion", 0f));
		mCards.Add(new Card(uiParent, "Tea", "leadershipPotion", 0f));
		int wavePlayed = Singleton<PlayStatistics>.Instance.data.wavePlayed;
		if (Singleton<Profile>.Instance.MultiplayerData.IsMultiplayerGameSessionActive() || wavePlayed != 1 || Singleton<Profile>.Instance.GetWaveLevel(wavePlayed) > 1)
		{
			return;
		}
		foreach (Card mCard in mCards)
		{
			mCard.Hide();
		}
		mCards.Clear();
	}

	public void Update(bool updateExpensiveVisuals)
	{
		foreach (Card mCard in mCards)
		{
			mCard.Update(updateExpensiveVisuals);
		}
	}

	public bool OnUIEvent(string eventID)
	{
		if (eventID.Length > "CONSUME:".Length && eventID.Substring(0, "CONSUME:".Length) == "CONSUME:")
		{
			TryConsume(eventID.Substring("CONSUME:".Length));
			return true;
		}
		return false;
	}

	public void OnPause(bool pause)
	{
	}

	private void TryConsume(string consumeID)
	{
		foreach (Card mCard in mCards)
		{
			if (mCard.consumeID == consumeID)
			{
				mCard.Execute();
			}
		}
	}
}
