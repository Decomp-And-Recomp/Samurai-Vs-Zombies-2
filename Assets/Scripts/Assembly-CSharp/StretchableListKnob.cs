using UnityEngine;

public class StretchableListKnob : GluiScrollIndicatorBase
{
	public GluiBouncyScrollList.Direction direction;

	private GluiNSlice mKnob;

	private float mFullSize;

	private Vector3 mOriginalPosition;

	private void Start()
	{
		mKnob = base.gameObject.GetComponent<GluiNSlice>();
		if (mKnob != null)
		{
			mOriginalPosition = mKnob.transform.localPosition;
			if (direction == GluiBouncyScrollList.Direction.Horizontal)
			{
				mFullSize = mKnob.Size.x;
			}
			else
			{
				mFullSize = mKnob.Size.y;
			}
		}
	}

	public override void OnScrollChanged(float viewStart, float viewEnd)
	{
		if (!(mKnob == null))
		{
			float num = (viewEnd - viewStart) * mFullSize;
			float num2 = ((viewEnd - viewStart) / 2f + viewStart - 0.5f) * mFullSize;
			if (direction == GluiBouncyScrollList.Direction.Horizontal)
			{
				mKnob.Size = new Vector2(num, mKnob.Size.y);
				mKnob.transform.localPosition = new Vector3(num2 + mOriginalPosition.x, mKnob.transform.localPosition.y, mKnob.transform.localPosition.z);
			}
			else
			{
				mKnob.Size = new Vector2(mKnob.Size.x, num);
				mKnob.transform.localPosition = new Vector3(mKnob.transform.localPosition.x, num2 + mOriginalPosition.y, mKnob.transform.localPosition.z);
			}
		}
	}
}
