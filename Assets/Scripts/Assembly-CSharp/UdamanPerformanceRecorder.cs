using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class UdamanPerformanceRecorder : IPerformanceRecorder
{
	private Dictionary<int, Stopwatch> stopWatches;

	public IDictionary<int, ICollection<float>> CounterValues { get; private set; }

	public UdamanPerformanceRecorder()
	{
		CounterValues = new Dictionary<int, ICollection<float>>();
		stopWatches = new Dictionary<int, Stopwatch>();
	}

	public void Start(int counterIndex)
	{
		if (!stopWatches.ContainsKey(counterIndex))
		{
			stopWatches.Add(counterIndex, new Stopwatch());
		}
		stopWatches[counterIndex].Start();
	}

	public void Stop(int counterIndex)
	{
		stopWatches[counterIndex].Stop();
	}

	public void Reset(int counterIndex)
	{
		stopWatches[counterIndex].Reset();
	}

	public void StoreValue(int counterIndex)
	{
		if (!CounterValues.ContainsKey(counterIndex))
		{
			CounterValues.Add(counterIndex, new List<float>());
		}
		CounterValues[counterIndex].Add((float)stopWatches[counterIndex].Elapsed.TotalMilliseconds);
	}

	public void StoreAll()
	{
		foreach (KeyValuePair<int, Stopwatch> stopWatch in stopWatches)
		{
			if (!CounterValues.ContainsKey(stopWatch.Key))
			{
				CounterValues.Add(stopWatch.Key, new List<float>());
			}
			CounterValues[stopWatch.Key].Add((float)stopWatch.Value.Elapsed.TotalMilliseconds);
		}
	}

	public float GetAverageValue(int counterIndex)
	{
		return CounterValues[counterIndex].Average();
	}

	public float GetMedianValue(int counterIndex)
	{
		List<float> list = CounterValues[counterIndex].ToList();
		list.Sort();
		int index = (list.Count + 1) / 2;
		int index2 = list.Count / 2;
		float num = list[index];
		float num2 = list[index2];
		return (num + num2) / 2f;
	}

	public float GetMinimumValue(int counterIndex)
	{
		return CounterValues[counterIndex].Min();
	}

	public float GetMaximumValue(int counterIndex)
	{
		return CounterValues[counterIndex].Max();
	}

	public void LogResults(IUdamanLogger logger)
	{
		foreach (int key in CounterValues.Keys)
		{
			UdamanPerformanceCounterIndex udamanPerformanceCounterIndex = (UdamanPerformanceCounterIndex)key;
			ICollection<float> source = CounterValues[key];
			float num = source.First();
			logger.LogMessage(string.Format("---------- Perf Counter Results for {0} - {1} ", udamanPerformanceCounterIndex, num));
		}
	}
}
