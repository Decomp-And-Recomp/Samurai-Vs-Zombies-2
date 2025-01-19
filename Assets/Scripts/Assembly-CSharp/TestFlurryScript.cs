using System.Collections.Generic;
using UnityEngine;

public class TestFlurryScript : MonoBehaviour
{
	private void Start()
	{
		if (Debug.isDebugBuild)
		{
			CFlurry.SetDebugLogEnabled(true);
		}
		string apiKey = "DEWSU3QN23BA7JXMH5PS";
		CFlurry.StartSession(apiKey);
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("info1", "1st parameter");
		dictionary.Add("eventValue", 10);
		dictionary.Add("externalRef", this);
		dictionary.Add("info2", -4.7);
		dictionary.Add("info3", "5th parameter");
		Dictionary<string, object> eventParams = dictionary;
		CFlurry.LogEvent("GENERAL_EVT_TYPE_TEST1", eventParams);
		dictionary = new Dictionary<string, object>();
		dictionary.Add("CustomInfo1", "1st parameter");
		dictionary.Add("CustomInfo2", 10);
		dictionary.Add("CustomInfo3", this);
		dictionary.Add("CustomInfo4", -4.7);
		dictionary.Add("CustomInfo5", "5th parameter");
		Dictionary<string, object> eventParams2 = dictionary;
		CFlurry.LogEvent("GENERAL_EVT_TYPE_TEST2", eventParams2);
		Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
		dictionary2.Add("CustomInfo1", "helloworld");
		CFlurry.LogEvent("GENERAL_EVT_TYPE_TEST3", dictionary2);
		CFlurry.StopSession();
	}

	private void Update()
	{
		if (Input.GetKey(KeyCode.Escape))
		{
			Application.Quit();
		}
	}
}
