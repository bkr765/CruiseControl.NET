using System;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Presentation
{
	public class TestingProjectMonitor : IProjectMonitor
	{
		private ProjectStatus projectStatus;
		private ProjectState projectState = ProjectState.NotConnected;
		private string projectName;

		public TestingProjectMonitor( string projectName )
		{
			this.projectName = projectName;
		}

		public string ProjectName
		{
			get { return projectName; }
		}
		public ProjectStatus ProjectStatus
		{
			get { return projectStatus; }
			set { projectStatus = value; }
		}
		public ProjectState ProjectState
		{
			get { return projectState; }
			set { projectState = value; }
		}

		public void OnBuildOccurred( MonitorBuildOccurredEventArgs args )
		{
			if (BuildOccurred != null)
				BuildOccurred( this, args );
		}

		public void OnPolled( MonitorPolledEventArgs args )
		{
			if (Polled != null)
				Polled( this, args );
		}

		public event MonitorBuildOccurredEventHandler BuildOccurred;
		public event MonitorPolledEventHandler Polled;

		public void Poll()
		{
			OnPolled(new MonitorPolledEventArgs(this));
		}
	}
}