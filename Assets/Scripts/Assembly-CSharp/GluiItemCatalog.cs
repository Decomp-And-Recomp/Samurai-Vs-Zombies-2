using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GluiItemCatalog
{
	public class Item
	{
		public enum Type
		{
			Icon = 0,
			Separator = 1
		}

		public delegate void ClippedHandler(Item item);

		public GameObject gameObject;

		public Type type;

		public object data;

		public bool selectable = true;

		protected bool clipped;

		private HashSet<long> disabledList = new HashSet<long>();

		public bool Clipped
		{
			get
			{
				return clipped;
			}
			set
			{
				if (clipped == value)
				{
					return;
				}
				clipped = value;
				if (clipped)
				{
					if (this.OnClipped != null)
					{
						this.OnClipped(this);
					}
					Disable();
				}
				else
				{
					if (this.OnUnClipped != null)
					{
						this.OnUnClipped(this);
					}
					Enable();
				}
			}
		}

		[method: MethodImpl(32)]
		public event ClippedHandler OnClipped;

		[method: MethodImpl(32)]
		public event ClippedHandler OnUnClipped;

		private void Disable()
		{
			Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
			Renderer[] array = componentsInChildren;
			foreach (Renderer renderer in array)
			{
				if (renderer != null && renderer.enabled)
				{
					renderer.enabled = false;
					disabledList.Add(renderer.GetInstanceID());
				}
			}
		}

		private void Enable()
		{
			Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
			Renderer[] array = componentsInChildren;
			foreach (Renderer renderer in array)
			{
				if (renderer != null && disabledList.Contains(renderer.GetInstanceID()))
				{
					renderer.enabled = true;
				}
			}
			disabledList.Clear();
		}

		public override string ToString()
		{
			return string.Format("[Item] Type={0} GameObject={1} Selectable={2}", type.ToString(), (!(gameObject == null)) ? gameObject.name : "null", selectable.ToString());
		}
	}

	private List<Item> items = new List<Item>();

	public bool UseOcclusionMasks { get; set; }

	public List<Item> Items
	{
		get
		{
			return items;
		}
	}

	public int Count
	{
		get
		{
			return items.Count;
		}
	}

	public void Clear()
	{
		foreach (Item item in items)
		{
			ObjectUtils.DestroyImmediate(item.gameObject);
		}
		items.Clear();
	}

	public Item Add(GameObject obj, Item.Type type, object data)
	{
		if (UseOcclusionMasks)
		{
			obj.AddComponent<GluiScrollListSetRenderQueue>();
		}
		Item item = new Item();
		item.gameObject = obj;
		item.type = type;
		item.data = data;
		Item item2 = item;
		items.Add(item2);
		return item2;
	}

	public Item GetItem(int n)
	{
		if (n < 0 || n >= items.Count)
		{
			return null;
		}
		return items[n];
	}

	public int FindItem(InputCrawl crawl, List<InputTrace.HitInfo> childHits)
	{
		InputTrace.HitInfo hit;
		foreach (InputTrace.HitInfo childHit in childHits)
		{
			hit = childHit;
			Component component = hit.target.transform.GetComponent(typeof(GluiWidget));
			if (!(component == null) && ((GluiWidget)component).AllowInput)
			{
				int num = items.FindIndex((Item item) => item.gameObject == hit.target || hit.target.transform.IsChildOf(item.gameObject.transform));
				if (num != -1)
				{
					return num;
				}
			}
		}
		return -1;
	}

	public Item FindItem(object data)
	{
		return items.Find((Item thisItem) => thisItem.data == data);
	}

	public void SelectItem(Item selectedItem)
	{
		foreach (Item item in Items)
		{
			bool selected = item == selectedItem;
			GluiButtonContainerBase[] componentsInChildren = item.gameObject.transform.GetComponentsInChildren<GluiButtonContainerBase>();
			GluiButtonContainerBase[] array = componentsInChildren;
			foreach (GluiButtonContainerBase gluiButtonContainerBase in array)
			{
				gluiButtonContainerBase.Selected = selected;
			}
		}
	}
}
