using System;
using System.Collections.Generic;
using UnityEngine;

public class ListToSlotDragManager : UIHandlerComponent
{
	private class Slot
	{
		private const float kTouchDistance = 130f;

		private Transform mOriginalTransform;

		private GameObject mPositioner;

		private GameObject mSlot;

		private GameObject mRequired;

		private GameObject mContent;

		private int mDataIndex = -1;

		private string mButtonEventID;

		private bool mSelected;

		public bool selected
		{
			get
			{
				return mSelected;
			}
			set
			{
				mSelected = value;
				if (mSelected)
				{
					mPositioner.transform.localPosition = new Vector3(mPositioner.transform.localPosition.x, mPositioner.transform.localPosition.y, -1f);
				}
				else
				{
					mPositioner.transform.localPosition = new Vector3(mPositioner.transform.localPosition.x, mPositioner.transform.localPosition.y, 0f);
				}
			}
		}

		public Vector2 localPosition
		{
			get
			{
				if (mPositioner != null)
				{
					return new Vector2(mPositioner.transform.localPosition.x, mPositioner.transform.localPosition.y);
				}
				return default(Vector2);
			}
			set
			{
				if (mPositioner != null)
				{
					mPositioner.transform.localPosition = new Vector3(value.x, value.y, mPositioner.transform.localPosition.z);
				}
			}
		}

		public int dataIndex
		{
			get
			{
				return mDataIndex;
			}
		}

		public Slot(Transform t, GameObject slotPrefab, string buttonEventID)
		{
			mButtonEventID = buttonEventID;
			mOriginalTransform = t;
			mSlot = UnityEngine.Object.Instantiate(slotPrefab) as GameObject;
			mSlot.GetComponent<GluiElement_Equip_Ally>().adaptor.isSlot = true;
			SetSlotVisible(true);
			AttachToTransform(mSlot);
		}

		public void Destroy()
		{
			if (mSlot != null)
			{
				UnityEngine.Object.DestroyImmediate(mSlot);
				mSlot = null;
			}
			if (mContent != null)
			{
				UnityEngine.Object.DestroyImmediate(mContent);
				mContent = null;
			}
		}

		public void Set(GameObject go)
		{
			if (mContent != null)
			{
				UnityEngine.Object.DestroyImmediate(mContent);
			}
			mContent = go;
			if (mContent != null)
			{
				AttachToTransform(mContent);
			}
			SetSlotVisible(mContent == null);
		}

		public bool ContainsTouch(Vector2 pos)
		{
			float num = Vector2.Distance(pos, ObjectUtils.GetObjectScreenPosition(mSlot));
			return num <= 130f;
		}

		private void SetSlotVisible(bool v)
		{
			mSlot.SetActive(v);
			if (v)
			{
				mSlot.FindChild("Swap_Icon").SetActive(false);
				mSlot.FindChild("Required").SetActive(false);
			}
		}

		private void AttachToTransform(GameObject go)
		{
			if (mPositioner == null)
			{
				mPositioner = new GameObject("Positioner");
				mPositioner.transform.parent = mOriginalTransform;
				mPositioner.transform.localPosition = Vector3.zero;
				mPositioner.transform.localScale = new Vector3(1f, 1f, 1f);
				mPositioner.transform.localRotation = Quaternion.identity;
			}
			go.transform.parent = mPositioner.transform;
			go.transform.localPosition = Vector3.zero;
			go.transform.localScale = new Vector3(1f, 1f, 1f);
			go.transform.localRotation = Quaternion.identity;
			ObjectUtils.SetLayerRecursively(mPositioner, LayerMask.NameToLayer("GLUI"));
			WeakGlobalMonoBehavior<EquipMenuImpl>.Instance.RegisterOnReleaseEvent(go.FindChildComponent<GluiStandardButtonContainer>("Button_Option_Selected"), mButtonEventID);
		}
	}

	public delegate bool OnCheckAvailabilityDelegate(int index);

	private const string kListItemSelectedCmd = "LISTITEM:";

	private const string kDeleteSlotCmd = "DELSLOT:";

	private const float kIgnoreNextButtonDistanceTreshold = 10f;

	public OnCheckAvailabilityDelegate onCheckAvailability;

	private object[] mData;

	private bool mAllowDups;

	private GluiBouncyScrollList mScrollList;

	private GameObject mSlotPrefab;

	private List<Slot> mSlots = new List<Slot>();

	private List<int> mSlotDataIndex = new List<int>();

	private DraggerGesture mDragGesture;

	private Vector2 mDragOrigin;

	private int mDraggedSlotIndex = -1;

	private bool mIgnoreNextSlotButtonEvent;

	private bool mSelectionRequired;

	public Action AutoSave { get; set; }

	public List<int> selection
	{
		get
		{
			return mSlotDataIndex;
		}
		set
		{
			mSlotDataIndex = value;
			while (mSlotDataIndex.Count > mSlots.Count)
			{
				mSlotDataIndex.RemoveAt(mSlotDataIndex.Count - 1);
			}
			RedrawAllSlots();
		}
	}

	public bool selectionRequired
	{
		get
		{
			return mSelectionRequired;
		}
		set
		{
			mSelectionRequired = value;
		}
	}

	public ListToSlotDragManager(List<Transform> slotTrans, GluiBouncyScrollList scrollList, object[] data, bool allowDups = false)
	{
		mScrollList = scrollList;
		mData = data;
		mAllowDups = allowDups;
		mScrollList.OnCellRendered += SetupCell;
		mDragGesture = new DraggerGesture(0.2f);
		mDragGesture.onStart = OnDragStart;
		mDragGesture.onDragMoveTo = OnDragMove;
		mDragGesture.onDrop = OnDrop;
		ClearSlots();
		CreateSlots(slotTrans);
	}

	public void Update(bool updateExpensiveVisuals)
	{
		mIgnoreNextSlotButtonEvent = false;
		mDragGesture.Update();
	}

	public bool OnUIEvent(string eventID)
	{
		if (eventID.Length > "LISTITEM:".Length && eventID.Substring(0, "LISTITEM:".Length) == "LISTITEM:")
		{
			int num = int.Parse(eventID.Substring("LISTITEM:".Length));
			WeakGlobalMonoBehavior<EquipMenuImpl>.Instance.commonInfoDisplay = mData[num];
			if (!mSelectionRequired)
			{
				AddListItemToSlot(num);
			}
			return true;
		}
		if (eventID.Length > "DELSLOT:".Length && eventID.Substring(0, "DELSLOT:".Length) == "DELSLOT:")
		{
			if (mIgnoreNextSlotButtonEvent)
			{
				mIgnoreNextSlotButtonEvent = false;
			}
			else if (mSelectionRequired)
			{
				ShowTopInfo(int.Parse(eventID.Substring("DELSLOT:".Length)));
			}
			else
			{
				DeleteSlot(int.Parse(eventID.Substring("DELSLOT:".Length)));
			}
			return true;
		}
		return false;
	}

	public void OnPause(bool pause)
	{
	}

	private void ClearSlots()
	{
		foreach (Slot mSlot in mSlots)
		{
			mSlot.Destroy();
		}
		mSlots.Clear();
	}

	private void CreateSlots(List<Transform> slotTrans)
	{
		if (mSlotPrefab == null)
		{
			mSlotPrefab = ResourceCache.GetCachedResource("UI/Prefabs/SelectSuite/Card_Item_Selected", 1).Resource as GameObject;
		}
		int num = 0;
		foreach (Transform slotTran in slotTrans)
		{
			Slot item = new Slot(slotTran, mSlotPrefab, "DELSLOT:" + num);
			mSlots.Add(item);
			num++;
		}
	}

	private void SetupCell(GameObject obj, int dataIndex)
	{
		WeakGlobalMonoBehavior<EquipMenuImpl>.Instance.RegisterOnReleaseEvent(obj.FindChildComponent<GluiStandardButtonContainer>("Button_Item"), "LISTITEM:" + dataIndex);
	}

	private void AddListItemToSlot(int listItemIndex)
	{
		if (onCheckAvailability != null && !onCheckAvailability(listItemIndex))
		{
			return;
		}
		if (mSlots.Count == 1)
		{
			mSlotDataIndex.Clear();
		}
		if (mSlotDataIndex.Count >= mSlots.Count)
		{
			return;
		}
		if (!mAllowDups)
		{
			foreach (int item in mSlotDataIndex)
			{
				if (item == listItemIndex)
				{
					return;
				}
			}
		}
		mSlotDataIndex.Add(listItemIndex);
		RedrawSlot(mSlotDataIndex.Count - 1);
		if (AutoSave != null)
		{
			AutoSave();
		}
	}

	private void ShowTopInfo(int slotIndex)
	{
		if (slotIndex < mSlotDataIndex.Count)
		{
			OnUIEvent("LISTITEM:" + mSlotDataIndex[slotIndex]);
		}
	}

	private void DeleteSlot(int slotIndex)
	{
		if (slotIndex < mSlotDataIndex.Count)
		{
			mSlotDataIndex.RemoveAt(slotIndex);
			for (int i = slotIndex; i <= mSlotDataIndex.Count; i++)
			{
				RedrawSlot(i);
			}
			if (AutoSave != null)
			{
				AutoSave();
			}
		}
	}

	private void RedrawSlot(int slotIndex)
	{
		Slot slot = mSlots[slotIndex];
		if (slotIndex >= mSlotDataIndex.Count)
		{
			slot.Set(null);
			return;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(mSlotPrefab) as GameObject;
		GluiElement_Equip_Ally component = gameObject.GetComponent<GluiElement_Equip_Ally>();
		component.adaptor.isSlot = true;
		GluiElement_Equip_Ally component2 = gameObject.GetComponent<GluiElement_Equip_Ally>();
		component2.adaptor.SetData(mData[mSlotDataIndex[slotIndex]]);
		slot.Set(gameObject);
	}

	private void RedrawAllSlots()
	{
		for (int i = 0; i < mSlots.Count; i++)
		{
			RedrawSlot(i);
		}
	}

	private void OnDragStart(Vector2 pos)
	{
		mDraggedSlotIndex = -1;
		int num = 0;
		foreach (Slot mSlot in mSlots)
		{
			if (mSlot.ContainsTouch(pos))
			{
				mDraggedSlotIndex = num;
				break;
			}
			num++;
		}
		if (mDraggedSlotIndex >= mSlotDataIndex.Count)
		{
			mDraggedSlotIndex = -1;
		}
		if (mDraggedSlotIndex >= 0)
		{
			mDragOrigin = GetScreenToWorldPosition(pos);
			mSlots[mDraggedSlotIndex].selected = true;
		}
	}

	private void OnDragMove(Vector2 pos)
	{
		if (mDraggedSlotIndex >= 0)
		{
			Vector2 a = GetScreenToWorldPosition(pos) - mDragOrigin;
			a.y = 0f;
			mSlots[mDraggedSlotIndex].localPosition = new Vector2(a.x, a.y);
			if (Vector2.Distance(a, mDragOrigin) >= 10f)
			{
				mIgnoreNextSlotButtonEvent = true;
			}
		}
	}

	private void OnDrop(Vector2 pos)
	{
		if (mDraggedSlotIndex < 0)
		{
			return;
		}
		mSlots[mDraggedSlotIndex].localPosition = default(Vector2);
		int num = 0;
		int num2 = -1;
		foreach (Slot mSlot in mSlots)
		{
			if (mSlot.ContainsTouch(pos))
			{
				num2 = num;
				break;
			}
			num++;
		}
		if (num2 < 0)
		{
			num2 = mDraggedSlotIndex;
		}
		MoveSlot(mDraggedSlotIndex, num2);
		mSlots[mDraggedSlotIndex].selected = false;
		mDraggedSlotIndex = -1;
	}

	private void MoveSlot(int indexFrom, int indexTo)
	{
		if (indexTo > mSlotDataIndex.Count)
		{
			indexTo = mSlotDataIndex.Count;
		}
		if (indexFrom != indexTo)
		{
			int item = mSlotDataIndex[indexFrom];
			mSlotDataIndex.RemoveAt(indexFrom);
			if (indexTo >= mSlotDataIndex.Count)
			{
				mSlotDataIndex.Add(item);
			}
			else
			{
				mSlotDataIndex.Insert(indexTo, item);
			}
			RedrawAllSlots();
		}
	}

	private static Vector2 GetScreenToWorldPosition(Vector2 pos)
	{
		Camera camera = ObjectUtils.FindFirstCamera(LayerMask.NameToLayer("GLUI"));
		if (camera != null)
		{
			Vector3 vector = camera.ScreenToWorldPoint(new Vector3(pos.x, pos.y, 0f));
			return new Vector2(vector.x, vector.y);
		}
		return Vector2.zero;
	}
}
