using System.Collections.Generic;
using UnityEngine;

public class GluiStateProcessesBase
{
	public class ProcessPhaseList
	{
		public GluiStatePhase phase;

		private List<GluiProcessBase> processes = new List<GluiProcessBase>();

		public int processesRunning;

		public List<GluiProcessBase> Processes
		{
			get
			{
				return processes;
			}
		}

		public void Interrupt()
		{
			if (processesRunning <= 0)
			{
				return;
			}
			foreach (GluiProcessBase process in processes)
			{
				process.ProcessInterrupt();
			}
			processesRunning = 0;
		}

		public void Stop()
		{
			if (processesRunning <= 0)
			{
				return;
			}
			foreach (GluiProcessBase process in processes)
			{
				process.ProcessDone();
			}
			processesRunning = 0;
		}

		public void Pause(bool pause)
		{
			foreach (GluiProcessBase process in processes)
			{
				process.ProcessPause(pause);
			}
		}
	}

	protected List<ProcessPhaseList> phaseLists = new List<ProcessPhaseList>();

	public void AddStateProcesses(GameObject parentToScan)
	{
		if (!(parentToScan == null))
		{
			object[] componentsInChildren = parentToScan.GetComponentsInChildren(typeof(GluiProcessBase));
			Add(componentsInChildren);
		}
	}

	private void Add(object[] processes)
	{
		for (int i = 0; i < processes.Length; i++)
		{
			GluiProcessBase gluiProcessBase = (GluiProcessBase)processes[i];
			if (gluiProcessBase.AnyPhaseHandled())
			{
				Add(gluiProcessBase);
			}
		}
	}

	private void Add(GluiProcessBase thisProcess)
	{
		GluiStatePhase[] array = thisProcess.PhasesHandled();
		for (int i = 0; i < array.Length; i++)
		{
			ProcessPhaseList processPhaseList = Find(array[i]);
			if (processPhaseList == null)
			{
				processPhaseList = new ProcessPhaseList();
				processPhaseList.phase = array[i];
				phaseLists.Add(processPhaseList);
			}
			processPhaseList.Processes.Add(thisProcess);
		}
	}

	protected ProcessPhaseList Find(GluiStatePhase phase)
	{
		foreach (ProcessPhaseList phaseList in phaseLists)
		{
			if (phaseList.phase == phase)
			{
				return phaseList;
			}
		}
		return null;
	}

	public virtual void Clear()
	{
		phaseLists.Clear();
	}
}
