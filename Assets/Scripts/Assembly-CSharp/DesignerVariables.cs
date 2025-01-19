using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class DesignerVariables : SingletonSpawningMonoBehaviour<DesignerVariables>
{
	public enum Operation
	{
		Constant = 0,
		RangeAB = 1,
		PickABC = 2
	}

	public class Param
	{
		public string A;

		public string B;

		public string C;
	}

	private Dictionary<string, Param> mMapDebugVars;

	private List<GameObject> lstListeners;

	protected override void Awake()
	{
		base.Awake();
		mMapDebugVars = new Dictionary<string, Param>();
		lstListeners = new List<GameObject>();
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		UnityEngine.Random.seed = DateTime.Now.Second;
	}

	public DesignerVariablesSchema GetVariableRecord(string strVariableName)
	{
		if (DataBundleRuntime.Instance == null || !DataBundleRuntime.Instance.Initialized)
		{
			return null;
		}
		DesignerVariablesSchema designerVariablesSchema = DataBundleRuntime.Instance.InitializeRecord<DesignerVariablesSchema>("DesignerVariables", strVariableName);
		if (designerVariablesSchema != null)
		{
			ReplaceRecordWithDebugOverride(strVariableName, designerVariablesSchema);
		}
		return designerVariablesSchema;
	}

	private void ReplaceRecordWithDebugOverride(string strVariableName, DesignerVariablesSchema record)
	{
		if (mMapDebugVars != null && mMapDebugVars.Count != 0)
		{
			strVariableName = strVariableName.ToLower();
			if (mMapDebugVars.ContainsKey(strVariableName))
			{
				Param param = mMapDebugVars[strVariableName];
				record.A = param.A;
				record.B = param.B;
				record.C = param.C;
			}
		}
	}

	public Param GetVariableParam(string strVariableName)
	{
		DesignerVariablesSchema designerVariablesSchema = DataBundleRuntime.Instance.InitializeRecord<DesignerVariablesSchema>("DesignerVariables", strVariableName);
		if (designerVariablesSchema == null)
		{
			return null;
		}
		Param param = new Param();
		strVariableName = strVariableName.ToLower();
		if (mMapDebugVars.ContainsKey(strVariableName.ToLower()))
		{
			Param param2 = mMapDebugVars[strVariableName];
			param.A = param2.A;
			param.B = param2.B;
			param.C = param2.C;
		}
		else
		{
			param.A = designerVariablesSchema.A;
			param.B = designerVariablesSchema.B;
			param.C = designerVariablesSchema.C;
		}
		return param;
	}

	public string GetFullVariableNameFromShortcutName(string strShortCutName)
	{
		DesignerVariablesSchema[] array = DataBundleRuntime.Instance.InitializeRecords<DesignerVariablesSchema>("DesignerVariables");
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].VariableName.ToLower().StartsWith(strShortCutName.ToLower()))
			{
				return array[i].VariableName;
			}
		}
		return string.Empty;
	}

	public void DumpAllVariableNames()
	{
		string text = string.Empty;
		DesignerVariablesSchema[] array = DataBundleRuntime.Instance.InitializeRecords<DesignerVariablesSchema>("DesignerVariables");
		for (int i = 0; i < array.Length; i++)
		{
			text = text + array[i].VariableName + ", ";
		}
	}

	public T GetVariable<T>(string strVariableName, [Optional] T defaultValue)
	{
		T returnVal;
		if (TryGetVariable<T>(strVariableName, out returnVal))
		{
			return returnVal;
		}
		return defaultValue;
	}

	public bool TryGetVariable<T>(string strVariableName, out T returnVal)
	{
		returnVal = default(T);
		DesignerVariablesSchema variableRecord = GetVariableRecord(strVariableName);
		if (variableRecord == null)
		{
			return false;
		}
		switch (variableRecord.operation)
		{
		case Operation.RangeAB:
			return Operation_RangeAB<T>(variableRecord, out returnVal);
		case Operation.Constant:
			return Operation_Const<T>(variableRecord, out returnVal);
		case Operation.PickABC:
			return Operation_PickABC<T>(variableRecord, out returnVal);
		default:
			returnVal = default(T);
			return false;
		}
	}

	public void SetVariable(string strVariableName, string A, string B, string C)
	{
		DesignerVariablesSchema variableRecord = GetVariableRecord(strVariableName);
		if (variableRecord != null)
		{
			Param param = new Param();
			param.A = A;
			param.B = B;
			param.C = C;
			strVariableName = strVariableName.ToLower();
			if (!mMapDebugVars.ContainsKey(strVariableName))
			{
				mMapDebugVars.Add(strVariableName.ToLower(), param);
			}
			else
			{
				mMapDebugVars[strVariableName] = param;
			}
			NotifyListenersOfVariableChange(strVariableName);
		}
	}

	public void RegisterListener(GameObject o)
	{
		bool flag = false;
		foreach (GameObject lstListener in lstListeners)
		{
			if (o == lstListener)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			lstListeners.Add(o);
		}
	}

	public void NotifyListenersOfVariableChange(string strVariableName)
	{
		foreach (GameObject lstListener in lstListeners)
		{
			lstListener.SendMessage("OnDesignerVariableValueChange", strVariableName);
		}
	}

	public void UnregisterListener(GameObject o)
	{
		foreach (GameObject lstListener in lstListeners)
		{
			if (o == lstListener)
			{
				lstListeners.Remove(lstListener);
				break;
			}
		}
	}

	public bool Operation_PickABC<T>(DesignerVariablesSchema record, out T returnVal)
	{
		T[] array = new T[3];
		if (ParseRecord<T>(record, out array[0], out array[1], out array[2]))
		{
			int num = UnityEngine.Random.Range(0, 2);
			returnVal = array[num];
			return true;
		}
		returnVal = default(T);
		return false;
	}

	public bool Operation_RangeAB<T>(DesignerVariablesSchema record, out T returnVal)
	{
		T valueA;
		T valueB;
		if (ParseRecord<T>(record, out valueA, out valueB))
		{
			Type typeFromHandle = typeof(T);
			if (typeFromHandle == typeof(int))
			{
				int num = UnityEngine.Random.Range((int)(object)valueA, (int)(object)valueB);
				returnVal = (T)(object)num;
				return true;
			}
			if (typeFromHandle == typeof(float))
			{
				float num2 = UnityEngine.Random.Range((float)(object)valueA, (float)(object)valueB);
				returnVal = (T)(object)num2;
				return true;
			}
		}
		returnVal = default(T);
		return false;
	}

	public bool Operation_Const<T>(DesignerVariablesSchema record, out T returnVal)
	{
		if (typeof(T) == typeof(Vector2))
		{
			float x = float.Parse(record.A);
			float y = float.Parse(record.B);
			Vector2 vector = new Vector2(x, y);
			returnVal = (T)(object)vector;
			return true;
		}
		if (typeof(T) == typeof(Vector3))
		{
			float x2 = float.Parse(record.A);
			float y2 = float.Parse(record.B);
			float z = float.Parse(record.C);
			Vector3 vector2 = new Vector3(x2, y2, z);
			returnVal = (T)(object)vector2;
			return true;
		}
		if (typeof(T) == typeof(string))
		{
			returnVal = (T)(object)record.A;
			return true;
		}
		if (typeof(T) == typeof(int) || typeof(T) == typeof(float) || typeof(T) == typeof(bool))
		{
			string text = record.A;
			if (typeof(T) == typeof(int))
			{
				int num = text.IndexOf(".");
				if (num != -1)
				{
					text = text.Remove(num, text.Length - num);
				}
			}
			T val = (T)Convert.ChangeType(text, typeof(T));
			returnVal = val;
			return true;
		}
		returnVal = default(T);
		return false;
	}

	private bool ParseRecord<T>(DesignerVariablesSchema record, out T valueA)
	{
		Type typeFromHandle = typeof(T);
		bool flag;
		if (typeFromHandle == typeof(int))
		{
			int result;
			flag = int.TryParse(record.A, out result);
			valueA = (T)(object)result;
		}
		else if (typeFromHandle == typeof(float))
		{
			float result2;
			flag = float.TryParse(record.A, out result2);
			valueA = (T)(object)result2;
		}
		else
		{
			flag = false;
			valueA = default(T);
		}
		if (!flag)
		{
		}
		return flag;
	}

	private bool ParseRecord<T>(DesignerVariablesSchema record, out T valueA, out T valueB)
	{
		Type typeFromHandle = typeof(T);
		bool flag;
		if (typeFromHandle == typeof(int))
		{
			int result;
			int result2 = default(int);
			flag = int.TryParse(record.A, out result) && int.TryParse(record.B, out result2);
			valueA = (T)(object)result;
			valueB = (T)(object)result2;
		}
		else if (typeFromHandle == typeof(float))
		{
			float result3;
			float result4 = default(float);
			flag = float.TryParse(record.A, out result3) && float.TryParse(record.B, out result4);
			valueA = (T)(object)result3;
			valueB = (T)(object)result4;
		}
		else
		{
			flag = false;
			valueA = default(T);
			valueB = default(T);
		}
		if (!flag)
		{
		}
		return flag;
	}

	private bool ParseRecord<T>(DesignerVariablesSchema record, out T valueA, out T valueB, out T valueC)
	{
		Type typeFromHandle = typeof(T);
		bool flag;
		if (typeFromHandle == typeof(int))
		{
			int result;
			int result2 = default(int);
			int result3;
			flag = int.TryParse(record.A, out result) && int.TryParse(record.B, out result2) && int.TryParse(record.C, out result3);
			valueA = (T)(object)result;
			valueB = (T)(object)result2;
			valueC = (T)(object)result2;
		}
		else if (typeFromHandle == typeof(float))
		{
			float result4;
			float result5 = default(float);
			float result6 = default(float);
			flag = float.TryParse(record.A, out result4) && float.TryParse(record.B, out result5) && float.TryParse(record.B, out result6);
			valueA = (T)(object)result4;
			valueB = (T)(object)result5;
			valueC = (T)(object)result6;
		}
		else
		{
			flag = false;
			valueA = default(T);
			valueB = default(T);
			valueC = default(T);
		}
		if (!flag)
		{
		}
		return flag;
	}

	private string ErrorPrefix(DesignerVariablesSchema record)
	{
		return "DesignerVariables: Record [" + record.VariableName + "] operation " + record.operation;
	}
}
