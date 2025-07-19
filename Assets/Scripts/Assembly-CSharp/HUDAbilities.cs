using System;
using System.Collections.Generic;
using UnityEngine;

public class HUDAbilities : UIHandlerComponent
{
    private class Card
    {
        private int mIndex;

        private GameObject mGameObject;

        private GluiStandardButtonContainer mButtonRef;

        private GluiSprite mIconRef;

        private ProgressMeterRadial mMeterRef;

        private float mCooldownCurrent;

        private float mCooldownMax;

        private bool mEnabled = true;

        public bool isAvailable
        {
            get
            {
                return mCooldownCurrent >= mCooldownMax && mEnabled && WeakGlobalMonoBehavior<InGameImpl>.Instance.hero.CanUseAbility();
            }
            set
            {
                if (mCooldownMax >= 0f)
                {
                    mEnabled = value;
                    ResetCooldown();
                }
            }
        }

        public int Index
        {
            get
            {
                return mIndex;
            }
        }

        private float cooldownFactor
        {
            get
            {
                if (mCooldownMax == 0f)
                {
                    return 1f;
                }
                return mCooldownCurrent / mCooldownMax;
            }
        }

        public Card(int index, GameObject prefab, Transform trans, AbilitySchema abData)
        {
            mIndex = index;
            mCooldownMax = abData.cooldown;
            if (WeakGlobalMonoBehavior<InGameImpl>.Instance.HasMagicCharm())
            {
                CharmSchema charmSchema = Singleton<CharmsDatabase>.Instance[WeakGlobalMonoBehavior<InGameImpl>.Instance.activeCharm];
                mCooldownMax *= 1f - charmSchema.abilityCooldownReduction * 0.01f;
            }
            mGameObject = (GameObject)UnityEngine.Object.Instantiate(prefab);
            mGameObject.transform.parent = trans;
            mGameObject.transform.localPosition = Vector3.zero;
            mButtonRef = mGameObject.FindChildComponent<GluiStandardButtonContainer>("Button_AbilityHUD");
            mIconRef = mGameObject.FindChildComponent<GluiSprite>("SwapIcon_Ability");
            mMeterRef = mGameObject.FindChildComponent<ProgressMeterRadial>("Meter_CooldownOL");
            Texture2D texture = ((!BundleUtils.GetSystemLanguage().StartsWith("English") && !(abData.iconNoText == null)) ? abData.iconNoText : abData.icon);
            mIconRef.Texture = texture;
            mMeterRef.Texture = texture;
            WeakGlobalMonoBehavior<HUD>.Instance.RegisterOnPressEvent(mButtonRef, "USE_ABILITY:" + index);
        }

        public void Update(bool updateExpensiveVisuals)
        {
            if (mCooldownMax >= 0f)
            {
                mCooldownCurrent += Time.deltaTime;
                if (updateExpensiveVisuals)
                {
                    mMeterRef.Value = Mathf.Clamp(1f - cooldownFactor, 0f, 1f);
                }
            }
            else if (updateExpensiveVisuals)
            {
                mMeterRef.Value = 0f;
            }
            mButtonRef.Locked = !isAvailable;
        }

        public void Spend()
        {
            mCooldownCurrent = 0f;
            if (mCooldownMax < 0f)
            {
                mEnabled = false;
            }
        }

        public void Destroy()
        {
            if (mGameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(mGameObject);
                mGameObject = null;
            }
        }

        private void ResetCooldown()
        {
            mCooldownCurrent = mCooldownMax;
        }
    }

    private const string kAbilityCmd = "USE_ABILITY:";

    public const string kLegendaryStrikeCmd = "LEGEND_STRIKE";

    private bool mEnabled = true;

    private List<string> mAbilitiesIDs;

    private List<Transform> mAbilitiesLocations = new List<Transform>(6);

    private Transform mCharmAbilityLocation;

    private List<Card> mCards = new List<Card>(6);

    private HUDLegendaryStrike mLegendaryStrike;

    private List<int> mQueuedAbilities = new List<int>();

    public bool enabled
    {
        get
        {
            return mEnabled;
        }
        set
        {
            mEnabled = value;
            foreach (Card mCard in mCards)
            {
                mCard.isAvailable = value;
            }
        }
    }

    public HUDAbilities(GameObject uiParent)
    {
        int num = 1;
        while (true)
        {
            string name = string.Format("Locator_Ability_{0}", num);
            Transform transform = ObjectUtils.FindTransformInChildren(uiParent.transform, name);
            if (transform == null)
            {
                break;
            }
            mAbilitiesLocations.Add(transform);
            num++;
        }
        mCharmAbilityLocation = ObjectUtils.FindTransformInChildren(uiParent.transform, "Locator_Charm_1");
        Init();
    }

    public void Clear()
    {
        foreach (Card mCard in mCards)
        {
            mCard.Destroy();
        }
        mCards.Clear();
    }

    public void Init()
    {
        Clear();
        SharedResourceLoader.SharedResource cachedResource = ResourceCache.GetCachedResource("Assets/Game/Resources/UI/Prefabs/HUD/Card_Ability_HUD.prefab", 1);
        GameObject prefab = cachedResource.Resource as GameObject;
        mAbilitiesIDs = Singleton<Profile>.Instance.GetSelectedAbilities();
        string text = string.Empty;
        try
        {
            Singleton<AbilitiesDatabase>.Instance.LoadInGameData(mAbilitiesIDs, false);
            for (int i = 0; i < mAbilitiesIDs.Count; i++)
            {
                text = text + mAbilitiesIDs[i] + "; ";
                AbilitySchema schema = Singleton<AbilitiesDatabase>.Instance.GetSchema(mAbilitiesIDs[i]);
                Transform transform = null;
                switch (schema.id)
                {
                    case "Invincibility":
                    case "Destruction":
                    case "TagTeam":
                    case "Friendship":
                        transform = mCharmAbilityLocation;
                        break;
                    default:
                        transform = mAbilitiesLocations[mCards.Count];
                        break;
                }
                mCards.Add(new Card(i, prefab, transform, schema));
            }
        }
        catch (Exception)
        {
        }
        if (Singleton<Profile>.Instance.inMultiplayerWave)
        {
            mLegendaryStrike = HUDLegendaryStrike.Create(mAbilitiesLocations[0].gameObject);
        }
        else if (Singleton<Profile>.Instance.inDailyChallenge && Singleton<Profile>.Instance.dailyChallengeProceduralWaveSchema.maxTime > 0f)
        {
            HUDDailyChallenge.Create(mAbilitiesLocations[0].gameObject);
        }
    }

    public void Update(bool updateExpensiveVisuals)
    {
        if (Input.GetKeyUp(KeyCode.E) && mCards.Count > 0)
        {
            TryUsingAbility(0);
        }
        if (Input.GetKeyUp(KeyCode.Q) && mCards.Count > 0)
        {
            TryUsingAbility(1);
        }
        foreach (Card mCard in mCards)
        {
            mCard.Update(updateExpensiveVisuals);
        }
        if (mQueuedAbilities.Count > 0 && WeakGlobalMonoBehavior<InGameImpl>.Instance.hero.DoAbility(mAbilitiesIDs[mQueuedAbilities[0]]))
        {
            Card card = mCards[mQueuedAbilities[0]];
            card.isAvailable = true;
            card.Spend();
            mQueuedAbilities.RemoveAt(0);
        }
    }

    public bool OnUIEvent(string eventID)
    {
        if (eventID.Length > "USE_ABILITY:".Length && eventID.Substring(0, "USE_ABILITY:".Length) == "USE_ABILITY:")
        {
            TryUsingAbility(int.Parse(eventID.Substring("USE_ABILITY:".Length)));
            return true;
        }
        if (eventID == "LEGEND_STRIKE" && mLegendaryStrike != null)
        {
            mLegendaryStrike.Activate();
        }
        return false;
    }

    public void OnPause(bool pause)
    {
        if (mLegendaryStrike != null)
        {
            mLegendaryStrike.OnPause(pause);
        }
    }

    public void TryUsingAbility(int index)
    {
        Card card = mCards[index];
        if (card.isAvailable)
        {
            if (WeakGlobalMonoBehavior<InGameImpl>.Instance.hero.DoAbility(mAbilitiesIDs[index]))
            {
                card.Spend();
            }
            else if (QueueAbility(index))
            {
                card.isAvailable = false;
            }
        }
    }

    private bool QueueAbility(int index)
    {
        mQueuedAbilities.Add(index);
        return true;
    }

    public bool IsAvailable(string abilityID)
    {
        for (int i = 0; i < mAbilitiesIDs.Count; i++)
        {
            if (mAbilitiesIDs[i] == abilityID)
            {
                return mCards[i].isAvailable;
            }
        }
        return false;
    }
}
