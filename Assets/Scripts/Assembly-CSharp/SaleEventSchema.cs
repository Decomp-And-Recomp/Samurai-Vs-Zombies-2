using System;
using System.Collections.Generic;

[DataBundleClass]
public class SaleEventSchema
{
	[DataBundleKey]
	public string name;

	[DataBundleSchemaFilter(typeof(SaleItemSchema), false)]
	public DataBundleRecordTable items;

	public int startYear;

	public int startMonth;

	public int startDay;

	public int startHour;

	public int startMinute;

	public int endYear;

	public int endMonth;

	public int endDay;

	public int endHour;

	public int endMinute;

	private DateTime? startDate;

	private DateTime? endDate;

	public DateTime StartDate
	{
		get
		{
			if (startYear == 0)
			{
				return DateTime.MinValue;
			}
			if (!startDate.HasValue)
			{
				startDate = new DateTime(startYear, startMonth, startDay, startHour, startMinute, 0, DateTimeKind.Utc);
			}
			return startDate.Value;
		}
	}

	public DateTime EndDate
	{
		get
		{
			if (startYear == 0)
			{
				return DateTime.MinValue;
			}
			if (!endDate.HasValue)
			{
				endDate = new DateTime(endYear, endMonth, endDay, endHour, endMinute, 0, DateTimeKind.Utc);
			}
			return endDate.Value;
		}
	}

	public bool IsActive
	{
		get
		{
			DateTime now = ApplicationUtilities.Now;
			return !StartDate.Equals(EndDate) && now.CompareTo(StartDate) > 0 && now.CompareTo(EndDate) < 0;
		}
	}

	public List<SaleItemSchema> SaleItems { get; private set; }

	public static string UdamanTableName
	{
		get
		{
			return "SaleEventTable";
		}
	}

	public static List<SaleEventSchema> SaleEventTable { get; private set; }

	public static void Init()
	{
		if (DataBundleRuntime.Instance == null)
		{
			return;
		}
		SaleEventTable = new List<SaleEventSchema>(DataBundleRuntime.Instance.InitializeRecords<SaleEventSchema>(UdamanTableName));
		foreach (SaleEventSchema item in SaleEventTable)
		{
			if (DataBundleRecordTable.IsNullOrEmpty(item.items))
			{
				continue;
			}
			item.SaleItems = new List<SaleItemSchema>(item.items.InitializeRecords<SaleItemSchema>());
			foreach (SaleItemSchema saleItem in item.SaleItems)
			{
				saleItem.SaleEvent = item;
			}
		}
	}

	public static List<SaleEventSchema> FindActiveSales()
	{
		if (SaleEventTable == null)
		{
			Init();
		}
		List<SaleEventSchema> list = new List<SaleEventSchema>();
		if (SaleEventTable != null)
		{
			foreach (SaleEventSchema item in SaleEventTable)
			{
				if (!DataBundleRecordTable.IsNullOrEmpty(item.items) && item.IsActive)
				{
					list.Add(item);
				}
			}
		}
		return list;
	}
}
