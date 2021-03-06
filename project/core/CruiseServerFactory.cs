
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using ThoughtWorks.CruiseControl.Core.Config;
using ThoughtWorks.CruiseControl.Core.State;
using ThoughtWorks.CruiseControl.Core.Util;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.Core
{
    /// <summary>
    /// 	
    /// </summary>
	public class CruiseServerFactory : ICruiseServerFactory
	{
		private static readonly string RemotingConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
		private static bool WatchConfigFile
		{
			get
			{
				string value = ConfigurationManager.AppSettings["WatchConfigFile"];
				return value == null || StringUtil.EqualsIgnoreCase(value, Boolean.TrueString);
			}
		}

		private static ICruiseServer CreateLocal(string configFile)
		{
            IProjectStateManager stateManager = new XmlProjectStateManager();
            // Load the extensions configuration
            var configuration = ConfigurationManager.GetSection("cruiseServer") as ServerConfiguration;
            List<ExtensionConfiguration> extensionList = null;
            if (configuration != null) extensionList = configuration.Extensions;

		    PathUtils.ConfigFileLocation = Path.IsPathRooted(configFile)
		                                       ? configFile
		                                       : Path.Combine(Environment.CurrentDirectory, configFile);
			var server = new CruiseServer(
				NewConfigurationService(configFile),
				new ProjectIntegratorListFactory(),
				new NetReflectorProjectSerializer(),
                stateManager,
				new SystemIoFileSystem(), 
				new ExecutionEnvironment(),
                extensionList);

            // Initialise the IoC container.
            server.InitialiseServices();

            return server;
		}

		private static IConfigurationService NewConfigurationService(string configFile)
		{
			IConfigurationService service = new FileConfigurationService(
				new DefaultConfigurationFileLoader(),
				new DefaultConfigurationFileSaver(
					new NetReflectorProjectSerializer()), 
				new FileInfo(configFile));

			if (WatchConfigFile)
				service = new FileWatcherConfigurationService(service, new FileChangedWatcher(configFile));
			
			return new CachingConfigurationService(service);
		}

		private static ICruiseServer CreateRemote(string configFile)
		{
			return new RemoteCruiseServer(CreateLocal(configFile), RemotingConfigurationFile);
		}

        /// <summary>
        /// Creates the specified remote.	
        /// </summary>
        /// <param name="remote">The remote.</param>
        /// <param name="configFile">The config file.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public ICruiseServer Create(bool remote, string configFile)
		{
			return (remote) ? CreateRemote(configFile) : CreateLocal(configFile);
		}
	}
}
