using System.Collections.Generic;

public interface IPerformanceRecorder
{
	IDictionary<int, ICollection<float>> CounterValues { get; }

	void Start(int counterIndex);

	void Stop(int counterIndex);

	void Reset(int counterIndex);

	void StoreValue(int counterIndex);

	float GetAverageValue(int counterIndex);

	float GetMedianValue(int counterIndex);

	float GetMinimumValue(int counterIndex);

	float GetMaximumValue(int counterIndex);

	void LogResults(IUdamanLogger logger);
}
