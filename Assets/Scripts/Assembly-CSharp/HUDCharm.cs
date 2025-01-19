using UnityEngine;

public class HUDCharm : UIHandlerComponent
{
	private GluiSprite mSpriteRef;

	public HUDCharm(GameObject uiParent)
	{
		mSpriteRef = uiParent.FindChildComponent<GluiSprite>("Swap_Sprite_Charm");
		CharmSchema charmSchema = Singleton<CharmsDatabase>.Instance[WeakGlobalMonoBehavior<InGameImpl>.Instance.activeCharm];
		Texture2D texture2D = ((charmSchema == null) ? null : ((!(charmSchema.hudIcon == null)) ? charmSchema.hudIcon : charmSchema.icon));
		if (texture2D != null)
		{
			mSpriteRef.Texture = texture2D;
			mSpriteRef.gameObject.SetActive(true);
		}
		else
		{
			mSpriteRef.gameObject.SetActive(false);
		}
	}

	public void Update(bool updateExpensiveVisuals)
	{
	}

	public bool OnUIEvent(string eventID)
	{
		return false;
	}

	public void OnPause(bool pause)
	{
	}
}
