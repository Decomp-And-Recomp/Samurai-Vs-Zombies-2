using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GluiCollections;
using UnityEngine;

[AddComponentMenu("Glui List/Bouncy Scroll List")]
public class GluiBouncyScrollList : GluiWidget, IInputContainer
{
	[Serializable]
	public enum Direction
	{
		Vertical = 0,
		Horizontal = 1
	}

	public delegate void OnSelectionChangedCallback(int dataIndex);

	public delegate void OnCellRenderedCallback(GameObject obj, int dataIndex);

	public Direction direction;

	public bool edgeAligned = true;

	public bool cellsRecycling = true;

	public bool redrawCellOnSelection;

	public float scrollTriggerTreshold = 12f;

	public bool useOcclusionMasks;

	public GluiAnchor anchorRight;

	public GluiAnchor anchorBottom;

	public GluiScrollIndicatorBase indicator;

	public bool allowMultitouch;

	private CellPool mCellPool;

	private Controller mCtrl;

	private Vector2? mLastShownPosition;

	private float mDeltaFromOrigin;

	private BouncyScrollInput mScrollInput;

	private GameObject mMover;

	private float[] mCellOffsets;

	private List<Cell> mActiveCells = new List<Cell>();

	private GluiInputMask inputMask = new GluiInputMask();

	private bool mForceRedrawOnNexttUpdate;

	private bool hasAskedControllerForInitialIndex;

	private float mIgnoreTouchesTimer;

	private int mSelection = -1;

	private float mLastIndicatorStart = -1f;

	private float mLastIndicatorEnd = -1f;

	public int Selection
	{
		get
		{
			return mSelection;
		}
		set
		{
			Select(value);
		}
	}

	public bool IsScrolling
	{
		get
		{
			if (mScrollInput == null)
			{
				return false;
			}
			return mScrollInput.IsScrolling;
		}
	}

	public bool IsDragging
	{
		get
		{
			if (mScrollInput == null)
			{
				return false;
			}
			return mScrollInput.IsDragging;
		}
	}

	private Rect Area
	{
		get
		{
			Rect result = new Rect(base.gameObject.transform.position.x - base.Size.x / 2f, base.gameObject.transform.position.y - base.Size.y / 2f, base.Size.x, base.Size.y);
			return result;
		}
	}

	[method: MethodImpl(32)]
	public event OnSelectionChangedCallback OnSelectionChanged;

	[method: MethodImpl(32)]
	public event OnCellRenderedCallback OnCellRendered;

	protected override void Awake()
	{
		base.Awake();
		mScrollInput = new BouncyScrollInput();
		mScrollInput.OnSimpleTouch = OnSimpleTouch;
		mScrollInput.ScrollTriggerTreshold = scrollTriggerTreshold;
		if (direction == Direction.Vertical)
		{
			mScrollInput.LockedX = true;
		}
		else
		{
			mScrollInput.LockedY = true;
		}
	}

	public override void Start()
	{
		base.Start();
		mScrollInput.Area = Area;
		if (!Application.isPlaying)
		{
			return;
		}
		mForceRedrawOnNexttUpdate = AdjustSizeFromAnchors();
		mMover = base.gameObject.FindChild("ScrollList_Mover");
		if (mMover == null)
		{
			mMover = new GameObject("ScrollList_Mover");
			AttachChildToParent(base.gameObject, mMover);
			if (useOcclusionMasks && mMover.GetComponent<GluiScrollListSetRenderQueue>() == null)
			{
				mMover.AddComponent<GluiScrollListSetRenderQueue>();
			}
		}
	}

	public override void OnDestroy()
	{
		if (mCellPool != null)
		{
			mCellPool.Destroy();
			mCellPool = null;
		}
		if (mActiveCells != null)
		{
			mActiveCells.Clear();
			mActiveCells = null;
		}
		base.OnDestroy();
	}

	public void Update()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (mCtrl == null || ((mCellOffsets == null || mCellOffsets.Length == 0) && mCtrl.dataCount > 0))
		{
			Redraw();
			return;
		}
		if (mScrollInput != null)
		{
			mScrollInput.Area = Area;
			if (mIgnoreTouchesTimer > 0f)
			{
				mIgnoreTouchesTimer = Mathf.Max(0f, mIgnoreTouchesTimer - GluiTime.deltaTime);
				mScrollInput.TouchesEnabled = false;
			}
			else
			{
				mScrollInput.TouchesEnabled = true;
			}
		}
		if (mCtrl != null && mScrollInput != null)
		{
			UpdateScrolling();
		}
		if (mForceRedrawOnNexttUpdate)
		{
			mForceRedrawOnNexttUpdate = false;
			Redraw();
		}
		UpdateIndicator();
	}

	public void Redraw(object arg = null)
	{
		if (mCtrl == null)
		{
			AcquireController();
		}
		if (mCtrl != null)
		{
			mCtrl.ReloadData(arg);
			ForceRedrawList();
		}
	}

	public void CenterOnItem(int index)
	{
		if (index >= 0 && mCellOffsets.Length != 0)
		{
			float num;
			try
			{
				num = mCellOffsets[index];
			}
			catch
			{
				return;
			}
			if (direction == Direction.Horizontal)
			{
				float x = Mathf.Clamp(num - Area.width / 2f + mCellPool.GetAny(mCtrl.GetCellPrefabForDataIndex(index)).Size.x / 2f, 0f, mScrollInput.ScrollMax.x);
				mScrollInput.ScrollPosition = new Vector2(x, 0f);
			}
			else
			{
				float y = Mathf.Clamp(num - Area.height / 2f + mCellPool.GetAny(mCtrl.GetCellPrefabForDataIndex(index)).Size.y / 2f, 0f, mScrollInput.ScrollMax.y);
				mScrollInput.ScrollPosition = new Vector2(0f, y);
			}
		}
	}

	public virtual void FilterInput(InputCrawl crawl, GameObject objectToFilter, out InputRouter.InputResponse response)
	{
		inputMask.FilterInput(crawl, objectToFilter, out response, base.gameObject);
	}

	public override void HandleInput(InputCrawl crawl, out InputRouter.InputResponse response)
	{
		if (!allowMultitouch && crawl.inputEvent.CursorIndex != 0)
		{
			response = InputRouter.InputResponse.Blocked;
			return;
		}
		mScrollInput.RecordInput(crawl.inputEvent);
		base.HandleInput(crawl, out response);
	}

	private void AcquireController()
	{
		if (!(mCtrl == null))
		{
			return;
		}
		mCtrl = base.gameObject.GetComponent<Controller>();
		if (!(mCtrl == null))
		{
			mCtrl.parentList = this;
			if (mCellPool != null)
			{
				mCellPool.Destroy();
			}
			mCellPool = new CellPool();
		}
	}

	private void OnSimpleTouch(Vector2 pos)
	{
		foreach (Cell mActiveCell in mActiveCells)
		{
			if (mCtrl.IsSelectable(mActiveCell.DataIndex) && mActiveCell.Area.Contains(pos))
			{
				Select(mActiveCell.DataIndex);
				break;
			}
		}
	}

	private void DeactivateAllCells()
	{
		mCellPool.DeactivateAll();
		mActiveCells.Clear();
	}

	public void ForceRedrawList()
	{
		DeactivateAllCells();
		if (direction == Direction.Horizontal)
		{
			float x = Mathf.Max(0f, CalcFullLength() - Area.width);
			mScrollInput.ScrollMax = new Vector2(x, 0f);
		}
		else
		{
			float y = Mathf.Max(0f, CalcFullLength() - Area.height);
			mScrollInput.ScrollMax = new Vector2(0f, y);
		}
		UpdateScrolling();
	}

	private float CalcFullLength()
	{
		int dataCount = mCtrl.dataCount;
		float num = 0f;
		if (mCellOffsets == null || mCellOffsets.Length < dataCount)
		{
			mCellOffsets = new float[dataCount];
		}
		for (int i = 0; i < dataCount; i++)
		{
			Cell any = mCellPool.GetAny(mCtrl.GetCellPrefabForDataIndex(i));
			if (direction == Direction.Horizontal)
			{
				mCellOffsets[i] = num;
				num += any.Size.x;
			}
			else
			{
				mCellOffsets[i] = num;
				num += any.Size.y;
			}
		}
		if (direction == Direction.Horizontal)
		{
			mDeltaFromOrigin = 0f - base.Size.x / 2f;
		}
		else
		{
			mDeltaFromOrigin = base.Size.y / 2f;
		}
		if (!edgeAligned)
		{
			if (direction == Direction.Horizontal)
			{
				if (base.Size.x > num)
				{
					mDeltaFromOrigin = 0f - num / 2f;
				}
			}
			else if (base.Size.y > num)
			{
				mDeltaFromOrigin = num / 2f;
			}
		}
		return num;
	}

	private void UpdateScrolling()
	{
		if (!(mMover == null) && mScrollInput != null)
		{
			if (!hasAskedControllerForInitialIndex)
			{
				hasAskedControllerForInitialIndex = true;
				CenterOnItem(mCtrl.initialIndexToShow);
			}
			mScrollInput.Update();
			if (mScrollInput.IsDragging)
			{
				SingletonMonoBehaviour<InputManager>.Instance.SetFocusedObject(base.gameObject);
			}
			else
			{
				SingletonMonoBehaviour<InputManager>.Instance.ClearFocusedObject(base.gameObject);
			}
			if (direction == Direction.Horizontal)
			{
				RefreshList(new Vector2(0f - mScrollInput.VisualScrollPosition.x, 0f));
			}
			else
			{
				RefreshList(new Vector2(0f, mScrollInput.VisualScrollPosition.y));
			}
		}
	}

	private void RefreshList(Vector2 newPos)
	{
		float z = mMover.transform.localPosition.z;
		mMover.transform.localPosition = new Vector3(newPos.x, newPos.y, z);
		if (mLastShownPosition.HasValue && mLastShownPosition.Value == newPos && mActiveCells.Count != 0)
		{
			return;
		}
		mLastShownPosition = newPos;
		if (cellsRecycling)
		{
			Rect area = Area;
			for (int num = mActiveCells.Count - 1; num >= 0; num--)
			{
				Cell cell = mActiveCells[num];
				Rect area2 = cell.Area;
				if (!area2.Intersects(area))
				{
					cell.Visible = false;
					mActiveCells.RemoveAt(num);
				}
			}
		}
		int num2 = (cellsRecycling ? FindOffset((direction != Direction.Horizontal) ? newPos.y : (0f - newPos.x)) : 0);
		for (int i = num2; i < mCtrl.dataCount; i++)
		{
			if (cellsRecycling)
			{
				if (direction == Direction.Horizontal)
				{
					if (mCellOffsets[i] + newPos.x > Area.width)
					{
						break;
					}
				}
				else if (newPos.y - mCellOffsets[i] < 0f - Area.height)
				{
					break;
				}
			}
			if (!IsActiveCell(i))
			{
				ActivateCell(i);
			}
		}
	}

	private int FindOffset(float target)
	{
		if (mCellOffsets != null)
		{
			for (int i = 0; i < mCellOffsets.Length; i++)
			{
				if (target <= mCellOffsets[i])
				{
					return Mathf.Clamp(i - 1, 0, mCellOffsets.Length);
				}
			}
		}
		return 0;
	}

	private Cell FindActiveCell(int dataIndex)
	{
		foreach (Cell mActiveCell in mActiveCells)
		{
			if (mActiveCell.DataIndex == dataIndex)
			{
				return mActiveCell;
			}
		}
		return null;
	}

	private bool IsActiveCell(int dataIndex)
	{
		return FindActiveCell(dataIndex) != null;
	}

	private void ActivateCell(int dataIndex)
	{
		Cell available = mCellPool.GetAvailable(mCtrl.GetCellPrefabForDataIndex(dataIndex));
		AttachChildToParent(mMover, available.Obj);
		mActiveCells.Add(available);
		available.DataIndex = dataIndex;
		available.Visible = true;
		try
		{
			DrawCell(available, dataIndex, dataIndex == mSelection);
		}
		catch (Exception)
		{
		}
		if (mCellOffsets != null && mCellOffsets.Length > dataIndex)
		{
			Vector3 localPosition = ((direction != Direction.Horizontal) ? new Vector3(0f, 0f - mCellOffsets[dataIndex] + mDeltaFromOrigin - available.Size.y / 2f, 0f) : new Vector3(mCellOffsets[dataIndex] + mDeltaFromOrigin + available.Size.x / 2f, 0f, 0f));
			available.Obj.transform.localPosition = localPosition;
			if (GameObject.Find("Panel_Revive(Clone)") != null)
			{
				available.Obj.transform.localPosition = new Vector3(localPosition.x, localPosition.y - 60f, localPosition.z);
			}
		}
	}

	private void Select(int newSelection)
	{
		if (newSelection == mSelection)
		{
			return;
		}
		if (redrawCellOnSelection && mSelection >= 0)
		{
			Cell cell = FindActiveCell(mSelection);
			if (cell != null)
			{
				DrawCell(cell, mSelection, false);
			}
		}
		mSelection = newSelection;
		if (redrawCellOnSelection && mSelection >= 0)
		{
			Cell cell2 = FindActiveCell(mSelection);
			if (cell2 != null)
			{
				DrawCell(cell2, mSelection, true);
			}
		}
		if (this.OnSelectionChanged != null)
		{
			this.OnSelectionChanged(mSelection);
		}
	}

	private void DrawCell(Cell c, int dataIndex, bool selected)
	{
		c.element.SetGluiCustomElementData(mCtrl.GetDataAtIndex(dataIndex));
		mCtrl.OnDrawn(c.element, dataIndex);
		if (this.OnCellRendered != null)
		{
			this.OnCellRendered(c.Obj, dataIndex);
		}
	}

	private bool AdjustSizeFromAnchors()
	{
		bool result = false;
		if (anchorRight != null)
		{
			float num = anchorRight.transform.position.x - (base.Size.x / 2f + base.transform.position.x);
			if (num != 0f)
			{
				result = true;
				base.Size = new Vector2(base.Size.x + num, base.Size.y);
				base.transform.position = new Vector3(base.transform.position.x + num / 2f, base.transform.position.y, base.transform.position.z);
			}
		}
		if (anchorBottom != null)
		{
			float num2 = anchorBottom.transform.position.y - (base.Size.y / 2f + base.transform.position.y);
			if (num2 != 0f)
			{
				result = true;
				base.Size = new Vector2(base.Size.x, base.Size.y + num2);
				base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y + num2 / 2f, base.transform.position.z);
			}
		}
		return result;
	}

	private void UpdateIndicator()
	{
		if (!(indicator == null) && mScrollInput != null)
		{
			float num;
			float value;
			if (direction == Direction.Horizontal)
			{
				num = mScrollInput.VisualScrollPosition.x / (mScrollInput.ScrollMax.x + base.Size.x);
				value = num + base.Size.x / (mScrollInput.ScrollMax.x + base.Size.x);
			}
			else
			{
				num = mScrollInput.VisualScrollPosition.y / (mScrollInput.ScrollMax.y + base.Size.y);
				value = num + base.Size.y / (mScrollInput.ScrollMax.y + base.Size.y);
			}
			num = Mathf.Clamp(num, 0f, 1f);
			value = Mathf.Clamp(value, 0f, 1f);
			if (num != mLastIndicatorStart || value != mLastIndicatorEnd)
			{
				mLastIndicatorStart = num;
				mLastIndicatorEnd = value;
				indicator.OnScrollChanged(mLastIndicatorStart, mLastIndicatorEnd);
			}
		}
	}

	private static void AttachChildToParent(GameObject parent, GameObject child)
	{
		Vector3 localScale = child.transform.localScale;
		Vector3 localPosition = child.transform.localPosition;
		child.transform.parent = parent.transform;
		child.transform.localPosition = localPosition;
		child.transform.localScale = localScale;
	}
}
