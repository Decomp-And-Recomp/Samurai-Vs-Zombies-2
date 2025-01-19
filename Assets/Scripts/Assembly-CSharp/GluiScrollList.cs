using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[AddComponentMenu("Glui/ScrollList")]
[ExecuteInEditMode]
public class GluiScrollList : GluiWidget, IInputContainer
{
	public enum WhenUserSelectsNone
	{
		SelectNone = 0,
		DoNothingAndBlockInput = 1,
		DoNothingAndIgnoreInput = 2
	}

	public enum InputHandling
	{
		SelectItemAndOverrideChildren = 0,
		PassInputToItems = 1
	}

	public enum Alignment
	{
		Left = 0,
		Center = 1,
		Right = 2
	}

	public enum SelectionSnap
	{
		Instant_Center = 0,
		None = 1
	}

	public enum Direction
	{
		Any = 0,
		Horizontal = 1,
		Vertical = 2
	}

	public delegate void FlingOverrideHandler(GluiCursorState state, ref Vector2 fling);

	public delegate void OffsetOverrideHandler(ref Vector2 offset);

	public Vector2 iconSize = new Vector2(100f, 100f);

	public Vector2 iconSpacing = new Vector2(10f, 10f);

	public float[] separatorSpacing = new float[2];

	public int maxRows = 10;

	public int maxCols = 10;

	public float flingDecay = 0.9f;

	public float dragStartDistance = 0.5f;

	public bool autoScroll;

	public float autoScrollVertical;

	public float autoScrollHorizontal;

	public float timeTillAutoScroll = 3f;

	public float itemDepth = 100f;

	public Direction scrollDirection;

	public bool useOcclusionMasks;

	public WhenUserSelectsNone whenUserSelectsNone = WhenUserSelectsNone.DoNothingAndBlockInput;

	public InputHandling inputHandling;

	public string actionOnSelect = string.Empty;

	public string soundOnSelect = "SOUND_GLUI_SCROLL_LIST_SELECT";

	public string persistSelectionAs = string.Empty;

	public string persistSelectionIndexAs = string.Empty;

	public string onSelectItem;

	public Alignment alignment = Alignment.Center;

	private GameObject iconAnchor;

	public GluiItemCatalog catalog = new GluiItemCatalog();

	public GluiScrollMotion scrollMotion;

	public GluiScrollArrange scrollArrange;

	public bool autoCenter;

	private Vector2 offset = new Vector2(0f, 0f);

	private bool dirty;

	private int touchItemFocus;

	private bool touchFocus;

	private GluiInputMask inputMask = new GluiInputMask();

	public Action PostClear;

	public Action<GameObject> PostShowItem;

	public Func<List<GluiItemCatalog.Item>, int, GluiItemCatalog.Item> GetItemOverride;

	public Func<List<GluiItemCatalog.Item>, List<GluiItemCatalog.Item>> GetItemsOverride;

	public Func<List<GluiItemCatalog.Item>, int> GetItemCountOverride;

	public FlingOverrideHandler FlingOverride;

	public OffsetOverrideHandler OffsetOverride;

	public List<GluiItemCatalog.Item> Items
	{
		get
		{
			if (GetItemsOverride != null)
			{
				return GetItemsOverride(catalog.Items);
			}
			return catalog.Items;
		}
	}

	public GameObject IconAnchor
	{
		get
		{
			return iconAnchor;
		}
	}

	public Vector2 Offset
	{
		get
		{
			return offset;
		}
	}

	public bool IsScrolling
	{
		get
		{
			return scrollMotion.Moving || scrollMotion.Dragging;
		}
	}

	public float TotalHeight
	{
		get
		{
			return scrollArrange.totalHeight;
		}
	}

	public int ItemCount
	{
		get
		{
			if (GetItemCountOverride != null)
			{
				return GetItemCountOverride(catalog.Items);
			}
			return catalog.Count;
		}
	}

	protected override ColliderType DefaultColliderType
	{
		get
		{
			return ColliderType.Auto_Box;
		}
	}

	public GluiScrollList()
	{
		base.Usable = true;
	}

	public void ResetPosition()
	{
		offset = Vector2.zero;
		scrollMotion.Clear();
		Reframe();
	}

	public virtual void Update()
	{
		if (!base.Enabled)
		{
			return;
		}
		if (dirty)
		{
			Reframe();
			dirty = false;
		}
		if (scrollMotion != null)
		{
			bool offsetChanged;
			scrollMotion.UpdateMotion(touchFocus, ref offset, out offsetChanged);
			if (offsetChanged)
			{
				SetOffset(offset);
			}
			if (!IsScrolling && autoCenter)
			{
				int index = FindClosestIndexToCenter();
				SnapTo(index, SelectionSnap.Instant_Center);
			}
		}
	}

	public void Clear()
	{
		catalog.Clear();
		if (PostClear != null)
		{
			PostClear();
		}
	}

	public GluiItemCatalog.Item AddObject(GameObject obj, GluiItemCatalog.Item.Type type, object data)
	{
		CheckAnchor();
		if (scrollArrange.Full(catalog))
		{
			return null;
		}
		obj.transform.parent = iconAnchor.transform;
		return catalog.Add(obj, type, data);
	}

	public GluiItemCatalog.Item AddObject(GameObject obj)
	{
		dirty = true;
		return AddObject(obj, GluiItemCatalog.Item.Type.Icon, null);
	}

	public void AddObjects(GameObject[] objArray)
	{
		foreach (GameObject obj in objArray)
		{
			AddObject(obj);
		}
		Reframe();
		dirty = true;
	}

	public GluiItemCatalog.Item AddIcon(Texture2D tex)
	{
		CheckAnchor();
		if (scrollArrange.Full(catalog))
		{
			return null;
		}
		GameObject gameObject = new GameObject();
		gameObject.transform.parent = iconAnchor.transform;
		gameObject.name = "icon_" + catalog.Count;
		GluiSprite gluiSprite = gameObject.AddComponent(typeof(GluiSprite)) as GluiSprite;
		gluiSprite.Texture = tex;
		gluiSprite.Size = iconSize;
		gluiSprite.Refresh();
		return AddObject(gameObject);
	}

	public void AddIcons(Texture2D[] texArray)
	{
		foreach (Texture2D tex in texArray)
		{
			AddIcon(tex);
		}
		Reframe();
	}

	public void Reframe()
	{
		scrollArrange.DistributeIcons(catalog);
		SetOffset(offset);
		scrollArrange.ClipItems(catalog);
	}

	public GluiItemCatalog.Item GetItem(int n)
	{
		if (GetItemOverride != null)
		{
			return GetItemOverride(catalog.Items, n);
		}
		return catalog.GetItem(n);
	}

	private void CheckAnchor()
	{
		if (iconAnchor == null)
		{
			iconAnchor = new GameObject();
			iconAnchor.transform.parent = base.transform;
			iconAnchor.name = "icon_anchor";
			iconAnchor.transform.localPosition = new Vector3(0f, 0f, itemDepth);
		}
	}

	private void SetOffset(Vector2 ofs)
	{
		if (OffsetOverride != null)
		{
			OffsetOverride(ref ofs);
			offset = ofs;
		}
		else
		{
			offset = ofs;
			float num = scrollArrange.totalWidth - base.Size.x + iconSpacing.x;
			if (num < 0f)
			{
				num = 0f;
			}
			float num2 = scrollArrange.totalHeight - base.Size.y + iconSpacing.y;
			if (num2 < 0f)
			{
				num2 = 0f;
			}
			offset.x = Mathf.Clamp(offset.x, 0f - num, 0f);
			offset.y = Mathf.Clamp(offset.y, 0f, num2);
		}
		if (iconAnchor != null)
		{
			Vector3 localPosition = offset;
			localPosition.z = itemDepth;
			iconAnchor.transform.localPosition = localPosition;
		}
		scrollArrange.ClipItems(catalog);
	}

	public void SnapTo(int index, SelectionSnap snap)
	{
		GluiItemCatalog.Item item = catalog.GetItem(index);
		if (item != null && snap == SelectionSnap.Instant_Center)
		{
			SetOffset(new Vector2(0f - item.gameObject.transform.localPosition.x, 0f - item.gameObject.transform.localPosition.y));
		}
	}

	public void Select(int index)
	{
		OnSelected(index, false);
	}

	private void OnSelected(int index, bool userSelection)
	{
		if (index != -1)
		{
			GluiSendMessageSupport.CallHandler(base.Handler, onSelectItem, index);
			if (persistSelectionAs != string.Empty)
			{
				SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save(persistSelectionAs, catalog.Items[index].data);
			}
			if (persistSelectionIndexAs != string.Empty)
			{
				SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save(persistSelectionIndexAs, index);
			}
			GluiActionSender.SendGluiAction(actionOnSelect, base.gameObject, catalog.Items[index].data);
			if (userSelection)
			{
				GluiSoundSender.SendGluiSound(soundOnSelect, base.gameObject);
			}
		}
		else if (whenUserSelectsNone == WhenUserSelectsNone.SelectNone || !userSelection)
		{
			GluiSendMessageSupport.CallHandler(base.Handler, onSelectItem, -1);
			if (persistSelectionAs != string.Empty)
			{
				SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save(persistSelectionAs, null);
			}
			if (persistSelectionIndexAs != string.Empty)
			{
				SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Save(persistSelectionIndexAs, -1);
			}
			GluiActionSender.SendGluiAction(actionOnSelect, base.gameObject, null);
		}
	}

	public void SelectItemObject(int index)
	{
		if (inputHandling == InputHandling.SelectItemAndOverrideChildren)
		{
			GluiItemCatalog.Item item = GetItem(index);
			catalog.SelectItem(item);
		}
		Reframe();
	}

	public void RestorePersistentSelection(int defaultIndex)
	{
		if (persistSelectionAs != string.Empty)
		{
			GluiItemCatalog.Item item = catalog.FindItem(SingletonSpawningMonoBehaviour<GluiPersistentDataCache>.Instance.Find(persistSelectionAs).tag);
			if (item != null)
			{
				Select(catalog.Items.IndexOf(item));
			}
		}
		Select(defaultIndex);
	}

	public int FindClosestIndexToCenter()
	{
		float num = -1f;
		int result = -1;
		int num2 = 0;
		foreach (GluiItemCatalog.Item item in catalog.Items)
		{
			float num3 = Vector2.Distance(offset, new Vector2(0f - item.gameObject.transform.localPosition.x, 0f - item.gameObject.transform.localPosition.y));
			if (num < 0f || num > num3)
			{
				result = num2;
				num = num3;
			}
			num2++;
		}
		return result;
	}

	public override void HandleInput(InputCrawl crawl, out InputRouter.InputResponse response)
	{
		if (!base.AllowInput)
		{
			response = InputRouter.InputResponse.Passthrough;
			return;
		}
		if (whenUserSelectsNone == WhenUserSelectsNone.DoNothingAndIgnoreInput)
		{
			response = InputRouter.InputResponse.Passthrough;
		}
		else
		{
			response = InputRouter.InputResponse.Handled;
		}
		List<InputTrace.HitInfo> list = crawl.Find_OriginalHits_ChildrenOf(base.gameObject);
		InputEvent.EEventType? childEvent = null;
		switch (crawl.inputEvent.EventType)
		{
		case InputEvent.EEventType.OnCursorDown:
			touchItemFocus = catalog.FindItem(crawl, list);
			OnCursorDown(crawl, ref response, ref childEvent);
			break;
		case InputEvent.EEventType.OnCursorUp:
			OnCursorUp(crawl, ref response, ref childEvent);
			break;
		case InputEvent.EEventType.OnCursorMove:
			OnCursorMove(crawl, ref response, ref childEvent);
			break;
		case InputEvent.EEventType.OnCursorExit:
			if (!scrollMotion.Dragging)
			{
				childEvent = InputEvent.EEventType.OnCursorExit;
			}
			break;
		}
		if (childEvent.HasValue && inputHandling == InputHandling.PassInputToItems)
		{
			list.Remove(crawl.CurrentHit);
			if (list.Count > 0)
			{
				crawl.QueueSubCrawl(childEvent.Value, list, crawl.OriginalHits, base.transform);
			}
		}
	}

	private void OnCursorDown(InputCrawl crawl, ref InputRouter.InputResponse response, ref InputEvent.EEventType? childEvent)
	{
		scrollMotion.Clear();
		scrollMotion.ResetDrag(crawl.inputEvent.Position);
		touchFocus = true;
		response = InputRouter.InputResponse.Handled;
		childEvent = InputEvent.EEventType.OnCursorDown;
	}

	private void OnCursorUp(InputCrawl crawl, ref InputRouter.InputResponse response, ref InputEvent.EEventType? childEvent)
	{
		if (touchFocus)
		{
			SingletonMonoBehaviour<InputManager>.Instance.ClearFocusedObject(base.gameObject);
			if (!scrollMotion.Dragging && (touchItemFocus != -1 || whenUserSelectsNone != WhenUserSelectsNone.DoNothingAndIgnoreInput))
			{
				OnSelected(touchItemFocus, true);
				childEvent = InputEvent.EEventType.OnCursorUp;
			}
			touchFocus = false;
			response = InputRouter.InputResponse.Handled;
		}
	}

	private void OnCursorMove(InputCrawl crawl, ref InputRouter.InputResponse response, ref InputEvent.EEventType? childEvent)
	{
		if (touchFocus)
		{
			if (!scrollMotion.Dragging)
			{
				childEvent = InputEvent.EEventType.OnCursorMove;
			}
			bool dragStarted;
			scrollMotion.Touching(crawl.inputEvent.Position, scrollDirection, out dragStarted);
			if (dragStarted)
			{
				childEvent = InputEvent.EEventType.OnCursorExit;
				SingletonMonoBehaviour<InputManager>.Instance.SetFocusedObject(base.gameObject);
			}
			response = InputRouter.InputResponse.Handled;
		}
	}

	public virtual void FilterInput(InputCrawl crawl, GameObject objectToFilter, out InputRouter.InputResponse response)
	{
		inputMask.FilterInput(crawl, objectToFilter, out response, base.gameObject);
	}

	protected override void Awake()
	{
		base.Awake();
		if (scrollMotion == null)
		{
			scrollMotion = new GluiScrollMotion(flingDecay, autoScroll, autoScrollVertical, autoScrollHorizontal, timeTillAutoScroll);
			scrollMotion.dragStartDistance = dragStartDistance;
		}
		scrollArrange = base.gameObject.GetComponent(typeof(GluiScrollArrange)) as GluiScrollArrange;
		if (scrollArrange == null)
		{
			scrollArrange = base.gameObject.AddComponent(typeof(GluiScrollArrange)) as GluiScrollArrange;
		}
		scrollArrange.Init(iconSize, iconSpacing, separatorSpacing, maxRows, maxCols, this);
		catalog.UseOcclusionMasks = useOcclusionMasks;
	}
}
