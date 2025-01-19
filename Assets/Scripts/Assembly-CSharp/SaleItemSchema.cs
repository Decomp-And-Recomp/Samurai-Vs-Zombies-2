using System.Collections.Generic;

[DataBundleClass]
public class SaleItemSchema
{
	[DataBundleKey]
	public int index;

	[DataBundleField(ColumnWidth = 200)]
	public string item;

	public float percentOff;

	[DataBundleField(ColumnWidth = 200)]
	public string replacementItem;

	public SaleEventSchema SaleEvent { get; set; }

	public static float FindActiveSaleForItem(string itemID)
	{
		if (DataBundleRuntime.Instance != null && !string.IsNullOrEmpty(itemID))
		{
			List<SaleEventSchema> list = SaleEventSchema.FindActiveSales();
			foreach (SaleEventSchema item in list)
			{
				if (item.SaleItems == null)
				{
					continue;
				}
				foreach (SaleItemSchema saleItem in item.SaleItems)
				{
					if (string.Equals(saleItem.item, itemID))
					{
						return saleItem.percentOff;
					}
				}
			}
		}
		return 0f;
	}

	public static SaleItemSchema FindActiveSaleDataForItem(string itemID)
	{
		if (DataBundleRuntime.Instance != null && !string.IsNullOrEmpty(itemID))
		{
			List<SaleEventSchema> list = SaleEventSchema.FindActiveSales();
			foreach (SaleEventSchema item in list)
			{
				if (item.SaleItems == null)
				{
					continue;
				}
				foreach (SaleItemSchema saleItem in item.SaleItems)
				{
					if (string.Equals(saleItem.item, itemID))
					{
						return saleItem;
					}
				}
			}
		}
		return null;
	}
}
