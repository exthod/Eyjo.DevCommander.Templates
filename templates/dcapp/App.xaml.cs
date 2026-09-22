using Eyjo.DevCommander;
using Eyjo.DevCommander.WpfLibrary;
using Eyjo.Toolkit.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Windows;

namespace DevCommanderModuleNameApp
{
	/// <summary>The DevCommanderModuleName WPF Application.</summary>
	public partial class App : Application
	{
		private IHost _host;
		private DevCommanderShellConfiguration _shellConfiguration = new();
		private readonly string _applicationDisplayName = "DevCommanderModuleNameApp";

		protected override void OnStartup(StartupEventArgs e)
		{
			// Remove potential invalid folder characters
			string applicationDataName = string.Concat(_applicationDisplayName.Split(Path.GetInvalidFileNameChars()));
			string applicationDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), applicationDataName);
			bool shutdown = false;

			LoadConfiguration();
			_host = SetupHost(applicationDataPath);
			if (_host is { })
			{
				// Host is set up at this point, but the DevCommander is not yet started. 
				ShutdownMode = ShutdownMode.OnMainWindowClose;

				// Global unhandled exception handler:
				Application.Current.DispatcherUnhandledException += (sender, args) =>
				{
					DisplayError("Woops - an unhandled exception occurred..!", args.Exception);
					args.Handled = true;
				};

				try
				{
					_host.Start();
					_ = _host.StartDevCommander(config =>
					{
						// Set up runtime configuration for DevCommander:
						config.Title = _applicationDisplayName;
						config.StorageFolder = applicationDataPath;
					});
				}
				catch (Exception ex)
				{
					DisplayError($"Woops - failed to start {_applicationDisplayName}..!", ex);
					shutdown = true;
				}
			}
			else
				shutdown = true;

			if (shutdown)
			{
				Application.Current.Shutdown();
				return;
			}

			base.OnStartup(e);
		}

		protected override void OnExit(ExitEventArgs e)
		{
			if (_host is { })
			{
				using (_host)
				{
					_host.StopAsync();
				}
			}

			base.OnExit(e);
		}

		private void LoadConfiguration()
		{
			try
			{
				// Set up configuration:
				ConfigurationBuilder builder = new();
				IConfigurationRoot configurationRoot = builder.SetBasePath(Directory.GetCurrentDirectory())
					.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
					.Build();

				// Load shell configuration 
				_shellConfiguration = configurationRoot.GetSection(nameof(DevCommanderShellConfiguration)).Get<DevCommanderShellConfiguration>();
			}
			catch (Exception ex)
			{
				DisplayError("Woops - failed to bind configuration. No configuration will be used..!", ex);
			}
		}

		private IHost SetupHost(string applicationDataPath)
		{
			IHost host = null;
			try
			{
				// Set up host and dependency injection
				host = Host.CreateDefaultBuilder()
				   .UseSerilog((context, loggerConfiguration) =>
				   {
					   loggerConfiguration
						   .WriteTo.File(Path.Combine(applicationDataPath, "Logs\\dc.log"), rollingInterval: RollingInterval.Day)
						   .MinimumLevel.Debug()
						   .WriteTo.Debug();
				   })
				   .ConfigureServices((context, services) =>
				   {
					   services.AddOptions();
				   })
				   .UseDevCommander((plugInHandler) =>
				   {
					   // Load plug-ins from potentially configured folders. A plug-in is an assembly that contains at least one class that
					   // implements the IDevCommanderModule interface. The assembly can be located in any folder and will be loaded at runtime.
					   // This allows for a flexible and extensible architecture, where new features can be added to the application without
					   // modifying the existing codebase, simply by adding new plug-in assemblies to the designated folders.
					   if (_shellConfiguration?.PlugInFolders != null)
					   {
						   foreach (string folder in _shellConfiguration.PlugInFolders)
						   {
							   string searchFolder = Path.Combine(AppContext.BaseDirectory, folder);
							   if (Directory.Exists(searchFolder)) plugInHandler.SearchFileOrFolder(searchFolder, SearchOption.AllDirectories);
						   }
					   }
				   })
				   .UseDevCommanderClassicTheme()
				   .Build();
			}
			catch (Exception ex)
			{
				string message = "Woops - failed to set up host and dependency injection..!";
				if (host is null) message = "Critical: Application cannot start! Host and dependency injection could not be set up.";
				DisplayError(message, ex);
			}
			return host;
		}

		private void DisplayError(string message, Exception ex)
		{
			if (ex?.InnerException is Exception innerException)
			{
				message = innerException.FormatExceptionMessage(message);
			}
			MessageBox.Show(ex.FormatExceptionMessage(message), _applicationDisplayName, MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}
}
