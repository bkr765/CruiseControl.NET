using System;
using System.IO;

using NUnit.Framework;
using marathon.framework;

using ThoughtWorks.CruiseControl.Shared.Util;

namespace ThoughtWorks.CruiseControl.ControlPanel.Test
{
	[TestFixture]
	public class MainPanelTest 
	{
		private DirectoryInfo _tmpDir;
		public const string _configFileContents = @"<cruisecontrol>
  <project name='MyProject'>
    <sourcecontrol type='cvs'>
      <executable>C:\program files\tortoisecvs\cvs</executable>
      <workingDirectory>c:\dev\ccnet\projects\marathon.net</workingDirectory>
    </sourcecontrol>

    <build type='nant'>
      <executable>c:\dev\ccnet\projects\marathon.net\tools\nant\nant.exe</executable>
      <baseDirectory>c:\dev\ccnet\projects\marathon.net</baseDirectory>
      <buildFile>cruise.build</buildFile>
    </build>
  </project>
</cruisecontrol>";

		[SetUp]
		public void SetUp() 
		{
			_tmpDir = new DirectoryInfo("tmp");
			_tmpDir.Create();
		}

		[TearDown]
		public void TearDown()
		{
			_tmpDir.Delete(true);
		}

		[Test, Ignore("need to come back to the gui testing - jeremy")]
		public void OpeningExistingConfigFileShowsProjectName()
		{
			TestableMainPanel panel = new TestableMainPanel();
			panel.FileToOpen = TempFileUtil.CreateTempFile(_tmpDir.FullName, "ccnet.config", _configFileContents);
			using (MarathonThread thread = new MarathonThread(panel)) 
			{
				using (MForm form = new MForm("Cruise Control - Control Panel")) 
				{
					form.Check("projectName", "");

					form.Press("&File.&Open");

					// this is not working in marathon yet, so we hacked around it
//					using (MForm openFileDialog = new MForm("Open File")) 
//					{
//						openFileDialog.Enter("File Name:", _tmpDir + "/ccnet.config");
//						openFileDialog.Press("Open");
//					}

					form.Check("projectName", "MyProject");
					form.Check("sourceControlType", "cvs");
				}
			}
		}

		private class TestableMainPanel : MainPanel 
		{
			public string FileToOpen;

			protected override string ChooseFileToOpen()
			{
				return FileToOpen;
			}
		}
	}
}
