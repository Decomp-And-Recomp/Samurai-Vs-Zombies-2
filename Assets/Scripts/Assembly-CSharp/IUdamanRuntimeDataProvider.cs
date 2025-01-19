using System;
using System.Collections.Generic;

public interface IUdamanRuntimeDataProvider
{
	void Initialize(bool forceUpdate);

	int GetTableCount(Type schemaName);

	List<string> GetTableNames(Type schemaName);

	IEnumerable<string> EnumerateTables(Type schemaName);

	int GetTableLength(Type schemaName, string tableName);

	List<string> GetRecordKeys(Type schemaName, string tableName, bool bFullKey);

	IEnumerable<string> EnumerateRecordKeys(Type schemaName, string tableName);

	IEnumerable<DataBundleRuntime.DataBundleResourceInfo> EnumerateUnityObjectPaths(Type schemaName, string tableName, bool followRecordLinks);

	IEnumerable<DataBundleRuntime.DataBundleResourceInfo> EnumerateUnityObjectPaths(Type schemaName, string tableName, string recordName, bool followRecordLinks);

	T GetFieldValue<T>(Type schemaName, string tableName, string recordName, string fieldName, T defaultValue, bool queryAssetPath, string language = null);

	T InitializeRecord<T>(string tableName, string recordName);

	T[] InitializeRecords<T>(string tableName);
}
