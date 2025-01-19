using UnityEngine;

public class GluiProcessSimulator : MonoBehaviour
{
	public bool run;

	public GameObject[] objects;

	public GluiStatePhase phaseToRun = GluiStatePhase.Init;

	public bool loop;

	private bool running;

	private GluiStateProcesses processes = new GluiStateProcesses();

	private void Awake()
	{
		if (!Application.isEditor)
		{
			Object.Destroy(this);
		}
	}

	private void Update()
	{
		if (run && !running)
		{
			running = true;
			processes.Clear();
			processes.Event_PhaseDone += HandleProcessesEvent_PhaseDone;
			if (objects != null)
			{
				GameObject[] array = objects;
				foreach (GameObject parentToScan in array)
				{
					processes.AddStateProcesses(parentToScan);
				}
			}
			processes.Start(phaseToRun, null);
		}
		else if (!run && running)
		{
			running = false;
			processes.Stop();
		}
	}

	private void HandleProcessesEvent_PhaseDone(GluiStatePhase phase)
	{
		running = false;
		if (!loop)
		{
			run = false;
		}
	}
}
