public interface IGluiDataSource
{
	void Get_GluiData(string dataFilterKey, string dataFilterKeySecondary, GluiDataScan_AdditionalParameters additionalParameters, out object[] records);
}
