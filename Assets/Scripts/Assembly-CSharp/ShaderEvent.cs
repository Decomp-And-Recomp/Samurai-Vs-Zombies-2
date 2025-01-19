using System.Collections.Generic;
using UnityEngine;

public class ShaderEvent
{
	protected bool mShouldDie;

	protected TypedWeakReference<GameObject> mObj;

	protected List<TypedWeakReference<Material>> mMaterials;

	protected List<Color> mStartingColors;

	public bool shouldDie
	{
		get
		{
			return mShouldDie;
		}
		set
		{
			mShouldDie = value;
		}
	}

	public GameObject gameObject
	{
		get
		{
			return (mObj == null) ? null : mObj.ptr;
		}
	}

	private ShaderEvent(GameObject obj, List<GameObject> objectsToIgnore)
	{
		mObj = new TypedWeakReference<GameObject>(obj);
		mMaterials = new List<TypedWeakReference<Material>>();
		mStartingColors = new List<Color>();
		if (!(obj != null))
		{
			return;
		}
		Renderer[] componentsInChildren = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			bool flag = true;
			if (objectsToIgnore != null)
			{
				foreach (GameObject item in objectsToIgnore)
				{
					if (item == renderer.gameObject)
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				Material[] materials = renderer.materials;
				foreach (Material ptr in materials)
				{
					mMaterials.Add(new TypedWeakReference<Material>(ptr));
				}
			}
		}
		componentsInChildren = obj.GetComponentsInChildren<MeshRenderer>();
		Renderer[] array2 = componentsInChildren;
		foreach (Renderer renderer2 in array2)
		{
			bool flag2 = true;
			if (objectsToIgnore != null)
			{
				foreach (GameObject item2 in objectsToIgnore)
				{
					if (item2 == renderer2.gameObject)
					{
						flag2 = false;
						break;
					}
				}
			}
			if (flag2)
			{
				Material[] materials2 = renderer2.materials;
				foreach (Material ptr2 in materials2)
				{
					mMaterials.Add(new TypedWeakReference<Material>(ptr2));
				}
			}
		}
	}

	public ShaderEvent(GameObject obj, List<GameObject> objectsToIgnore, Dictionary<int, Color> startingColors)
		: this(obj, objectsToIgnore)
	{
		if (startingColors != null)
		{
			SetStartingColors(startingColors);
		}
		else
		{
			SetStartingColors();
		}
	}

	public virtual void resetToBaseValues()
	{
	}

	public virtual void update()
	{
		if (mObj.ptr == null || !mObj.ptr.activeInHierarchy)
		{
			mShouldDie = true;
		}
	}

	protected void SetAllMaterialsToColor(Color color)
	{
		foreach (TypedWeakReference<Material> mMaterial in mMaterials)
		{
			if (!object.ReferenceEquals(mMaterial.ptr, null))
			{
				mMaterial.ptr.SetColor("_Color", color);
				mMaterial.ptr.SetColor("_MainColor", color);
				mMaterial.ptr.SetColor("_RimColor", color);
			}
		}
	}

	protected Color GetMaterialColor(int index)
	{
		if (mMaterials == null || mMaterials.Count <= index || object.ReferenceEquals(mMaterials[index].ptr, null))
		{
			return Color.white;
		}
		Material ptr = mMaterials[index].ptr;
		if (ptr.HasProperty("_RimColor"))
		{
			return ptr.GetColor("_RimColor");
		}
		if (ptr.HasProperty("_Color"))
		{
			return ptr.GetColor("_Color");
		}
		if (ptr.HasProperty("_MainColor"))
		{
			return ptr.GetColor("_MainColor");
		}
		return Color.white;
	}

	private void SetStartingColors()
	{
		mStartingColors.Clear();
		for (int i = 0; i < mMaterials.Count; i++)
		{
			Color materialColor = GetMaterialColor(i);
			mStartingColors.Add(materialColor);
		}
	}

	private void SetStartingColors(Dictionary<int, Color> startColors)
	{
		mStartingColors.Clear();
		for (int i = 0; i < mMaterials.Count; i++)
		{
			Color value;
			if (!startColors.TryGetValue(mMaterials[i].ptr.GetInstanceID(), out value))
			{
				value = Color.white;
			}
			mStartingColors.Add(value);
		}
	}

	protected void BlendAllMaterialsToColor(Color targetColor, float interpolant)
	{
		for (int i = 0; i < mMaterials.Count; i++)
		{
			if (!object.ReferenceEquals(mMaterials[i].ptr, null))
			{
				Color materialColor = GetMaterialColor(i);
				Color color = Color.Lerp(materialColor, targetColor, interpolant);
				mMaterials[i].ptr.SetColor("_Color", color);
				mMaterials[i].ptr.SetColor("_MainColor", color);
				mMaterials[i].ptr.SetColor("_RimColor", color);
			}
		}
	}

	protected void RevertAllMaterialsToAlpha(float interpolant)
	{
		for (int i = 0; i < mMaterials.Count; i++)
		{
			if (!object.ReferenceEquals(mMaterials[i].ptr, null))
			{
				Color materialColor = GetMaterialColor(i);
				Color color = new Color(materialColor.r, materialColor.g, materialColor.b, mStartingColors[i].a);
				mMaterials[i].ptr.SetColor("_Color", color);
				mMaterials[i].ptr.SetColor("_MainColor", color);
				mMaterials[i].ptr.SetColor("_RimColor", color);
			}
		}
	}

	protected void RevertAllMaterialsToColor(float interpolant)
	{
		for (int i = 0; i < mMaterials.Count; i++)
		{
			if (!object.ReferenceEquals(mMaterials[i].ptr, null))
			{
				Color color = mStartingColors[i];
				mMaterials[i].ptr.SetColor("_Color", color);
				mMaterials[i].ptr.SetColor("_MainColor", color);
				mMaterials[i].ptr.SetColor("_RimColor", color);
			}
		}
	}
}
