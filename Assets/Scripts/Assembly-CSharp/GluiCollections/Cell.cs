using UnityEngine;

namespace GluiCollections
{
	public class Cell
	{
		public GameObject Obj;

		public int DataIndex = -1;

		public Vector2 Size;

		public GluiElement_Base element;

		private bool mVisible;

		public Rect Area
		{
			get
			{
				Vector3 position = Obj.transform.position;
				return new Rect(position.x - Size.x / 2f, position.y - Size.y / 2f, Size.x, Size.y);
			}
		}

		public bool Visible
		{
			get
			{
				return mVisible;
			}
			set
			{
				if (Obj != null)
				{
					Obj.SetActive(value);
				}
				mVisible = value;
			}
		}
	}
}
