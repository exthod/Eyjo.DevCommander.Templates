using Eyjo.DevCommander;
using Eyjo.DevCommander.Extensions;
using Eyjo.DevCommander.WpfLibrary;
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
			string applicationName = "DevCommanderModuleNameApp";
			string applicationDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"DevCommanderApps\\{applicationName}");

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

			try
			{
				_host.Start();
				_ = _host.StartDevCommander(config =>
				{
					config.Title = applicationName;
					config.StorageFolder = applicationDataPath;
				});
			}
			catch (Exception ex)
			{
				string message = $"Woops - failed to start {applicationName}..!";
				MessageBox.Show(ex.FormatExceptionMessage(message), applicationName, MessageBoxButton.OK, MessageBoxImage.Error);
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
