using System.Collections.Generic;
using UnityEngine;

public class ColorTable
{
	public string colorTableName;

	public List<Color> colors = new List<Color>();

	public bool IsInitialized
	{
		get
		{
			return colors != null && colors.Count > 0;
		}
	}

	public string tableName
	{
		get
		{
			return colorTableName;
		}
	}

	public int Count
	{
		get
		{
			return colors.Count;
		}
	}

	public float? ColorKeyToT(int iKey)
	{
		if (colors.Count <= 0)
		{
			return null;
		}
		iKey = Mathf.Clamp(iKey, 0, colors.Count);
		return (float)iKey / (float)colors.Count;
	}

	public void initializeColorTable(string udamanTableName)
	{
		if (!string.IsNullOrEmpty(udamanTableName) && colorTableName != udamanTableName)
		{
			colorTableName = udamanTableName;
			PullColorsFromColorTable();
		}
	}

	public Color GetColorKey(int i)
	{
		return colors[i];
	}

	public void PullColorsFromColorTable()
	{
		colors.Clear();
		ColorSliderSchema[] array = DataBundleRuntime.Instance.InitializeRecords<ColorSliderSchema>(colorTableName);
		ColorSliderSchema[] array2 = array;
		foreach (ColorSliderSchema colorSliderSchema in array2)
		{
			AddColor(new Color((float)colorSliderSchema.r / 255f, (float)colorSliderSchema.g / 255f, (float)colorSliderSchema.b / 255f));
		}
	}

	private void AddColor(Color color)
	{
		colors.Add(color);
	}

	public Color sectionalLerp(float t)
	{
		if (colors.Count < 2)
		{
			return Color.magenta;
		}
		float num = colors.Count - 1;
		int num2 = Mathf.Min(Mathf.FloorToInt(t * num), colors.Count - 2);
		int num3 = Mathf.Min(num2 + 1, colors.Count - 1);
		float num4 = (float)num2 / num;
		float num5 = (float)num3 / num;
		float t2 = (t - num4) / (num5 - num4);
		Color color = colors[num2];
		Color color2 = colors[num3];
		return new Color(Mathf.Lerp(color.r, color2.r, t2), Mathf.Lerp(color.g, color2.g, t2), Mathf.Lerp(color.b, color2.b, t2), Mathf.Lerp(color.a, color2.a, t2));
	}
}
