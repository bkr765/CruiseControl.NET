using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using ThoughtWorks.CruiseControl.CCTrayLib.Configuration;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.CCTrayLib.Presentation;
using ThoughtWorks.CruiseControl.CCTrayLib.ServerConnection;

namespace CCTrayMulti
{
	public class MainForm : Form
	{
		private MenuItem menuFile;
		private MenuItem menuFileExit;
		private ListView lvProjects;
		private ColumnHeader colProject;
		private ColumnHeader colProjectStatus;
		private ImageList iconList;
		private MainMenu mainMenu;
		private TrayIcon trayIcon;
		private IContainer components;


		public MainForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			ApplyTemporaryHackToGetSomethingUpAndRunning();

		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager( typeof (MainForm) );
			this.lvProjects = new System.Windows.Forms.ListView();
			this.colProject = new System.Windows.Forms.ColumnHeader();
			this.colProjectStatus = new System.Windows.Forms.ColumnHeader();
			this.iconList = new System.Windows.Forms.ImageList( this.components );
			this.mainMenu = new System.Windows.Forms.MainMenu();
			this.menuFile = new System.Windows.Forms.MenuItem();
			this.menuFileExit = new System.Windows.Forms.MenuItem();
			this.trayIcon = new TrayIcon();
			this.SuspendLayout();
			// 
			// lvProjects
			// 
			this.lvProjects.Columns.AddRange( new System.Windows.Forms.ColumnHeader[]
				{
					this.colProject,
					this.colProjectStatus
				} );
			this.lvProjects.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lvProjects.Location = new System.Drawing.Point( 0, 0 );
			this.lvProjects.Name = "lvProjects";
			this.lvProjects.Size = new System.Drawing.Size( 292, 266 );
			this.lvProjects.SmallImageList = this.iconList;
			this.lvProjects.TabIndex = 0;
			this.lvProjects.View = System.Windows.Forms.View.List;
			// 
			// colProject
			// 
			this.colProject.Text = "Project";
			this.colProject.Width = 160;
			// 
			// colProjectStatus
			// 
			this.colProjectStatus.Text = "Status";
			this.colProjectStatus.Width = 123;
			// 
			// iconList
			// 
			this.iconList.ImageSize = new System.Drawing.Size( 16, 16 );
			this.iconList.ImageStream = ((System.Windows.Forms.ImageListStreamer) (resources.GetObject( "iconList.ImageStream" )));
			this.iconList.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// mainMenu
			// 
			this.mainMenu.MenuItems.AddRange( new System.Windows.Forms.MenuItem[]
				{
					this.menuFile
				} );
			// 
			// menuFile
			// 
			this.menuFile.Index = 0;
			this.menuFile.MenuItems.AddRange( new System.Windows.Forms.MenuItem[]
				{
					this.menuFileExit
				} );
			this.menuFile.Text = "&File";
			// 
			// menuFileExit
			// 
			this.menuFileExit.Index = 0;
			this.menuFileExit.Text = "&Exit";
			this.menuFileExit.Click += new System.EventHandler( this.menuFileExit_Click );
			// 
			// trayIcon
			// 
			this.trayIcon.Text = "CruiseControl.NET\n(This tooltip information still to be implemented)";
			this.trayIcon.Visible = true;
			// 
			// MainForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size( 5, 13 );
			this.ClientSize = new System.Drawing.Size( 292, 266 );
			this.Controls.Add( this.lvProjects );
			this.Icon = ((System.Drawing.Icon) (resources.GetObject( "$this.Icon" )));
			this.Menu = this.mainMenu;
			this.Name = "MainForm";
			this.Text = "CruiseControl.NET";
			this.ResumeLayout( false );

		}

		#endregion

		private void menuFileExit_Click( object sender, EventArgs e )
		{
			Application.Exit();
		}


		private Poller poller;

		private void ApplyTemporaryHackToGetSomethingUpAndRunning()
		{
			RemoteCruiseManagerFactory remoteCruiseManagerFactory = new RemoteCruiseManagerFactory();
			ICruiseProjectManagerFactory factory = new CruiseProjectManagerFactory( remoteCruiseManagerFactory );

			CCTrayMultiConfiguration configuration = new CCTrayMultiConfiguration( factory, "settings.xml" );

			IProjectMonitor[] monitors = configuration.GetProjectStatusMonitors();

			CreateListViewAdaptors( monitors );

			IProjectMonitor aggregatedMonitor = new AggregatingProjectMonitor( monitors );

			ProjectStateIconAdaptor projectStateIconAdaptor = new ProjectStateIconAdaptor( aggregatedMonitor, new ResourceProjectStateIconProvider() );
			DataBindings.Add( "Icon", projectStateIconAdaptor, "Icon" );
			
			trayIcon.BindTo(projectStateIconAdaptor);
			trayIcon.BindTo(aggregatedMonitor);			
			trayIcon.Visible = true;

			poller = new Poller( 5000, aggregatedMonitor);
			poller.Start();

			Debug.WriteLine( "started -- thread is " + Thread.CurrentThread.GetHashCode() );
		}

		private void CreateListViewAdaptors( IProjectMonitor[] monitors )
		{
			foreach (IProjectMonitor monitor in monitors)
			{
				lvProjects.Items.Add( new ProjectStatusListViewItemAdaptor().Create( monitor ) );
			}
		}

	}
}