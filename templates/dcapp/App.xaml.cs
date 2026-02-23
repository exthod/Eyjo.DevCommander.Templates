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

		protected override void OnStartup(StartupEventArgs e)
		{
			string applicationDisplayName = "DevCommanderModuleNameApp";

			// Remove potential invalid folder characters
			string applicationDataName = string.Concat(applicationDisplayName.Split(Path.GetInvalidFileNameChars()));
			string applicationDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"DevCommanderApps\\{applicationDataName}");

			ConfigurationBuilder builder = new();
			IConfigurationRoot _configurationRoot = builder.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.Build();

			_host = Host.CreateDefaultBuilder()
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
					// Uncomment to load IDevCommanderModule plug-ins from the "PlugIns" folder:
					//plugInHandler.SearchFileOrFolder("PlugIns", SearchOption.AllDirectories);
				})
				.UseDevCommanderClassicTheme()
				.Build();

			ShutdownMode = ShutdownMode.OnMainWindowClose;

			// Global unhandled exception handler:
			Application.Current.DispatcherUnhandledException += (sender, args) =>
			{
				string message = "Woops - an unhandled exception occurred..!";
				MessageBox.Show(args.Exception.FormatExceptionMessage(message), applicationDisplayName, MessageBoxButton.OK, MessageBoxImage.Error);
				args.Handled = true;
			};

			try
			{
				_host.Start();
				_ = _host.StartDevCommander(config =>
				{
					config.Title = applicationDisplayName;
					config.StorageFolder = applicationDataPath;
				});
			}
			catch (Exception ex)
			{
				string message = $"Woops - failed to start {applicationDisplayName}..!";
				MessageBox.Show(ex.FormatExceptionMessage(message), applicationDisplayName, MessageBoxButton.OK, MessageBoxImage.Error);
				Application.Current.Shutdown();
			}

			base.OnStartup(e);
		}

		protected override void OnExit(ExitEventArgs e)
		{
			using (_host)
			{
				_host.StopAsync();
			}

			base.OnExit(e);
		}
	}
}
