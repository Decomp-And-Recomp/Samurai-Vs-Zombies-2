using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Effect Maestro/Effect Container")]
public class EffectContainer : MonoBehaviour
{
	public enum EffectDefaultPosition
	{
		UseEffectPosition = 0,
		CenterOnOwner = 1
	}

	public enum EffectState
	{
		Disabled = 0,
		Enabled = 1,
		Killed = 2,
		Enabling = 3,
		Disabling = 4,
		Killing = 5
	}

	public float effectZDepth = 1f;

	public GameObject effectPrefab;

	public GameObject effectInstantiated;

	public EffectDefaultPosition effectDefaultPosition;

	public bool startEnabled;

	private GameObject effect;

	private bool effectPositionAtDefault;

	protected Camera effectCamera;

	private GameObject owner;

	private Camera ownerCamera;

	private bool destroyObjectWhenDone;

	private List<EffectConductor> conductors = new List<EffectConductor>();

	protected EffectState effectState;

	public float delayBeforeStop;

	public float delayBeforeStart;

	public float delayBeforeKill;

	private float stateChangeTime;

	public GameObject Effect
	{
		get
		{
			return effect;
		}
		set
		{
			effect = value;
			if (effect != null)
			{
				effectCamera = ObjectUtils.FindFirstCamera(effect.layer);
				OnEffectAdded();
			}
		}
	}

	public Vector2 EffectScreenPosition
	{
		get
		{
			if (Effect == null)
			{
				return Vector2.zero;
			}
			return EffectCamera.WorldToScreenPoint(Effect.transform.position);
		}
	}

	public Camera EffectCamera
	{
		get
		{
			return effectCamera;
		}
	}

	public bool EffectDead
	{
		get
		{
			return effectState == EffectState.Killed || effectState == EffectState.Killing;
		}
	}

	public Camera OwnerCamera
	{
		get
		{
			if (ownerCamera == null)
			{
				ownerCamera = ObjectUtils.FindFirstCamera(base.gameObject.layer);
			}
			return ownerCamera;
		}
	}

	public bool DestroyObjectWhenDone
	{
		set
		{
			destroyObjectWhenDone = value;
		}
	}

	public GameObject Owner
	{
		get
		{
			return owner;
		}
		set
		{
			owner = value;
			ownerCamera = null;
		}
	}

	public List<EffectConductor> Conductors
	{
		get
		{
			return conductors;
		}
	}

	public void Awake()
	{
		owner = base.gameObject;
		if (effectPrefab != null)
		{
			Effect = (GameObject)Object.Instantiate(effectPrefab);
		}
		else if (effectInstantiated != null)
		{
			Effect = effectInstantiated;
		}
	}

	public void OnDestroy()
	{
		EffectKillNow(true);
	}

	public void Update()
	{
		switch (effectState)
		{
		case EffectState.Enabled:
			if (effectPositionAtDefault)
			{
				EffectMoveToDefault();
			}
			break;
		case EffectState.Disabling:
			if (Time.time > stateChangeTime)
			{
				EffectDisableNow();
			}
			break;
		case EffectState.Enabling:
			if (Time.time > stateChangeTime)
			{
				EffectEnableNow();
			}
			break;
		case EffectState.Killing:
			if (Time.time > stateChangeTime)
			{
				EffectKillNow();
			}
			break;
		case EffectState.Killed:
			break;
		}
	}

	protected void OnEffectAdded()
	{
		effectState = EffectState.Enabled;
		EffectMoveToDefault();
		if (startEnabled)
		{
			EffectEnableNow();
		}
		else
		{
			EffectDisableNow();
		}
	}

	public void EffectEnable()
	{
		if (!EffectDead)
		{
			if (delayBeforeStart == 0f)
			{
				EffectEnableNow();
			}
			else
			{
				EffectEnableDelayed();
			}
		}
	}

	private void EffectEnableDelayed()
	{
		if (effectState != EffectState.Enabling && effectState != EffectState.Enabled)
		{
			stateChangeTime = Time.time + delayBeforeStart;
			effectState = EffectState.Enabling;
		}
	}

	protected void EffectEnableNow()
	{
		if (effectState != EffectState.Enabled)
		{
			EffectUpdateVisibility(true);
			effectState = EffectState.Enabled;
		}
	}

	public void EffectDisable()
	{
		if (!EffectDead)
		{
			if (delayBeforeStop == 0f)
			{
				EffectDisableNow();
			}
			else
			{
				EffectDisableDelayed();
			}
		}
	}

	private void EffectDisableDelayed()
	{
		if (effectState != EffectState.Disabling && effectState != 0)
		{
			stateChangeTime = Time.time + delayBeforeStop;
			effectState = EffectState.Disabling;
		}
	}

	protected void EffectDisableNow()
	{
		if (effectState != 0 && effect != null)
		{
			EffectUpdateVisibility(false);
		}
		effectState = EffectState.Disabled;
	}

	public void EffectKill()
	{
		if (!EffectDead)
		{
			if (delayBeforeKill == 0f)
			{
				EffectKillNow();
			}
			else
			{
				EffectKillDelayed();
			}
		}
	}

	protected void EffectKillDelayed()
	{
		if (effectState != EffectState.Killing)
		{
			stateChangeTime = Time.time + delayBeforeKill;
			effectState = EffectState.Killing;
		}
	}

	public void EffectKillNow(bool destroyingContainer = false)
	{
		if (effect != null)
		{
			conductors.ForEach(delegate(EffectConductor conductor)
			{
				conductor.OnEffectKilled(destroyingContainer);
			});
			conductors.ForEach(delegate(EffectConductor conductor)
			{
				Object.Destroy(conductor);
			});
			conductors.Clear();
			EffectDisableNow();
			Object.Destroy(effect);
			if (destroyObjectWhenDone)
			{
				Object.Destroy(base.gameObject);
			}
			Object.Destroy(this);
			effect = null;
			effectState = EffectState.Killed;
		}
	}

	private void EffectUpdateVisibility(bool visibility)
	{
		if (!(effect == null))
		{
			bool flag = false;
			Component[] componentsInChildren = effect.transform.GetComponentsInChildren<ParticleSystem>();
			Component[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				ParticleSystem particleSystem = (ParticleSystem)array[i];
				flag = true;
				particleSystem.enableEmission = visibility;
			}
			Component[] componentsInChildren2 = effect.transform.GetComponentsInChildren(typeof(ParticleEmitter));
			Component[] array2 = componentsInChildren2;
			for (int j = 0; j < array2.Length; j++)
			{
				ParticleEmitter particleEmitter = (ParticleEmitter)array2[j];
				flag = true;
				particleEmitter.emit = visibility;
			}
			flag = true;
			if (visibility)
			{
				EffectMoveToDefault();
			}
			else
			{
				effect.transform.position = new Vector3(-99999f, -99999f, -99999f);
			}
			if (!flag)
			{
				effect.SetActive(visibility);
			}
		}
	}

	public void EffectMove(Vector2 position)
	{
		if (effectCamera != null)
		{
			EffectMoveToWorld(effectCamera.ScreenToWorldPoint(new Vector3(position.x, position.y, effectZDepth)));
		}
	}

	public void EffectMoveToScreen(Vector3 position)
	{
		if (effectCamera != null)
		{
			EffectMoveToWorld(effectCamera.ScreenToWorldPoint(new Vector3(position.x, position.y, effectZDepth)));
		}
	}

	public void EffectMoveToWorld(Vector3 position)
	{
		if (effect != null)
		{
			effect.transform.position = position;
			effectPositionAtDefault = false;
		}
	}

	protected void EffectMoveToDefault()
	{
		effectPositionAtDefault = true;
		EffectDefaultPosition effectDefaultPosition = this.effectDefaultPosition;
		if (effectDefaultPosition != EffectDefaultPosition.CenterOnOwner)
		{
			return;
		}
		Vector3 position;
		if (base.gameObject.layer == effect.layer)
		{
			position = base.gameObject.transform.position;
		}
		else
		{
			Camera camera = OwnerCamera;
			if (!(camera != null) || !(effectCamera != null))
			{
				return;
			}
			Vector3 position2 = camera.WorldToScreenPoint(base.gameObject.transform.position);
			position = effectCamera.ScreenToWorldPoint(position2);
		}
		position.z = effectZDepth;
		effect.transform.position = position;
	}

	public void EffectScale(float scale)
	{
		if (effect != null)
		{
			effect.transform.localScale = new Vector3(scale, scale, scale);
		}
	}

	public void AddConductor(EffectConductor conductor)
	{
		conductors.Add(conductor);
	}

	public void SetStartPosition(Vector2 position)
	{
		EffectMove(position);
		conductors.ForEach(delegate(EffectConductor conductor)
		{
			conductor.SetStartPosition(position);
		});
	}
}
