using System;
using System.Windows.Forms;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.CCTrayLib
{
	/// <summary>
	/// Monitors a remote CruiseControl.NET instance, and raises events in
	/// responses to changes in the server's state.
	/// </summary>
	public class StatusMonitor : IDisposable
	{
		public event BuildOccurredEventHandler BuildOccurred;
		public event PolledEventHandler Polled;
		public event ErrorEventHandler Error;

		private Timer _pollTimer;
		private ProjectStatus _currentProjectStatus = new ProjectStatus (ProjectIntegratorState.Stopped, IntegrationStatus.Unknown, ProjectActivity.Sleeping, "unknown", "http://ccnet.thoughtworks.com", DateTime.MinValue, "unknown", DateTime.MaxValue);
		private Settings _settings;

		public StatusMonitor (Settings settings)
		{
			_settings = settings;
			_pollTimer = new Timer ();
			_pollTimer.Interval = 15000;
			_pollTimer.Tick += new EventHandler (this.pollTimer_Tick);
		}

		/// <summary>
		/// Gets the Url of the build results web page for the current project.
		/// </summary>
		public string WebUrl
		{
			get { return _currentProjectStatus.WebURL; }
		}

		public Settings Settings
		{
			get { return _settings; }
			set { _settings = value; }
		}

		public ProjectStatus ProjectStatus
		{
			get { return _currentProjectStatus; }
		}

		public void Dispose ()
		{
			StopPolling();
		}

		public void StartPolling ()
		{
			// poll immediately
			Poll ();

			// use timer to ensure periodic polling
			_pollTimer.Enabled = true;
			_pollTimer.Start ();
		}

		public void StopPolling ()
		{
			_pollTimer.Enabled = false;
			_pollTimer.Stop ();
		}

		void pollTimer_Tick (object sender, EventArgs e)
		{
			Poll ();

			// update interval, in case it has changed
			_pollTimer.Interval = Settings.PollingIntervalSeconds*1000;
		}

		public void Poll ()
		{
			// check for any change in status, and raise events accordingly
			try
			{
				// todo: fix me - we need to have the name of the project we're looking at here, so we can
				// report on the right one - jeremy
				ProjectStatus latestProjectStatus = GetSingleRemoteProjectStatus (); //GetRemoteProjectStatus()[0];

				OnPolled (new PolledEventArgs (latestProjectStatus));

				if (HasBuildOccurred (latestProjectStatus))
				{
					BuildTransition transition = GetBuildTransition (latestProjectStatus);
					OnBuildOccurred (new BuildOccurredEventArgs (latestProjectStatus, transition));
				}

				_currentProjectStatus = latestProjectStatus;
			}
			catch (Exception ex)
			{
				OnError (new ErrorEventArgs (ex));
			}
		}

		#region Build transition detection

		bool HasBuildOccurred (ProjectStatus newProjectStatus)
		{
			// If last build date is DateTime.MinValue (struct's default value),
			// then the remote status has not yet been recorded.
			if (_currentProjectStatus.LastBuildDate == DateTime.MinValue)
				return false;

			// compare dates
			return (newProjectStatus.LastBuildDate != _currentProjectStatus.LastBuildDate);
		}

		#endregion

		protected virtual void OnPolled (PolledEventArgs e)
		{
			if (Polled != null)
				Polled (this, e);
		}

		protected virtual void OnBuildOccurred (BuildOccurredEventArgs e)
		{
			if (BuildOccurred != null)
				BuildOccurred (this, e);
		}

		protected virtual void OnError (ErrorEventArgs e)
		{
			if (Error != null)
				Error (this, e);
		}

		ProjectStatus GetSingleRemoteProjectStatus ()
		{
			ProjectStatus[] projectStatusses = GetRemoteCruiseControlProxy ().GetProjectStatus ();
			foreach (ProjectStatus status in projectStatusses)
			{
				if (status.Name == this.Settings.ProjectName)
				{
					return status;
				}
			}
			// TODO - we need a better way of getting the 'default' project if one hasn't been defined
			return projectStatusses[0];
		}

		public ProjectStatus[] GetRemoteProjects ()
		{
			try
			{
				return GetRemoteCruiseControlProxy ().GetProjectStatus (); //.GetProjects();
			}
			catch
			{
				// Ignore the error
				return new ProjectStatus[0];
			}
		}

		private ICruiseManager GetRemoteCruiseControlProxy ()
		{
			return _settings.CruiseManager;
		}

		public void ForceBuild (string projectName)
		{
			GetRemoteCruiseControlProxy ().ForceBuild (projectName);
		}

		private BuildTransition GetBuildTransition (ProjectStatus projectStatus)
		{
			bool wasOk = _currentProjectStatus.BuildStatus == IntegrationStatus.Success;
			bool isOk = projectStatus.BuildStatus == IntegrationStatus.Success;

			if (wasOk && isOk)
				return BuildTransition.StillSuccessful;
			else if (!wasOk && !isOk)
				return BuildTransition.StillFailing;
			else if (wasOk && !isOk)
				return BuildTransition.Broken;
			else if (!wasOk && isOk)
				return BuildTransition.Fixed;

			throw new Exception ("The universe has gone crazy.");
		}

	}

}