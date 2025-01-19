using System.Threading;

namespace UnityThreading
{
	internal class TaskWorker : ThreadBase
	{
		public Dispatcher Dispatcher;

		public TaskDistributor TaskDistributor;

		public TaskWorker(TaskDistributor taskDistributor)
			: base(false)
		{
			TaskDistributor = taskDistributor;
			Dispatcher = new Dispatcher(false);
		}

		protected override void Do()
		{
			while (!exitEvent.WaitOne(0))
			{
				if (Dispatcher.ProcessNextTask())
				{
					continue;
				}
				TaskDistributor.FillTasks(Dispatcher);
				if (Dispatcher.TaskCount == 0)
				{
					if (WaitHandle.WaitAny(new WaitHandle[2] { exitEvent, TaskDistributor.NewDataWaitHandle }) == 0)
					{
						break;
					}
					TaskDistributor.FillTasks(Dispatcher);
				}
			}
		}

		public override void Dispose()
		{
			base.Dispose();
			if (Dispatcher != null)
			{
				Dispatcher.Dispose();
			}
			Dispatcher = null;
		}
	}
}
