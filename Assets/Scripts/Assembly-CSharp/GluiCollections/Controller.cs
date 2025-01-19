using System;
using UnityEngine;

namespace GluiCollections
{
	[Serializable]
	public abstract class Controller : MonoBehaviour
	{
		public abstract int dataCount { get; }

		public virtual int initialIndexToShow
		{
			get
			{
				return 0;
			}
		}

		public GluiBouncyScrollList parentList { get; set; }

		public abstract string GetCellPrefabForDataIndex(int dataIndex);

		public abstract void ReloadData(object arg = null);

		public abstract object GetDataAtIndex(int index);

		public virtual bool IsSelectable(int dataIndex)
		{
			return false;
		}

		public virtual void OnDrawn(GluiElement_Base elem, int dataIndex)
		{
		}
	}
}
