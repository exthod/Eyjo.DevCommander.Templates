using Eyjo.DevCommander;
using Eyjo.DevCommander.Extensions;
using Eyjo.DevCommander.WpfLibrary;
using Eyjo.DevCommander.WpfLibrary.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Windows;

namespace MyDevCommanderApp
{
	/// <summary>The MyDevCommanderApp WPF Application.</summary>
	public partial class App : Application
	{
		private IHost _host;
		private IWindowsShellNotifications _windowShellModel;

		protected override void OnStartup(StartupEventArgs e)
		{
			ConfigurationBuilder builder = new();
			IConfigurationRoot _configurationRoot = builder.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.Build();

			_host = Host.CreateDefaultBuilder()
				.UseSerilog((context, loggerConfiguration) =>
				{
					string applicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
					loggerConfiguration
						.WriteTo.File(Path.Combine(applicationDataPath, "MyDevCommanderApp", "MyDevCommanderApp.log"), rollingInterval: RollingInterval.Day)
						.MinimumLevel.Debug()
						.WriteTo.Debug();
				})
				.ConfigureServices((context, services) =>
				{
					services.AddOptions();
				})
				.UseDevCommander((plugInHandler) =>
				{
					// Uncomment to load IDevCommanderModule plug-ins from the "PlugIns" folder.
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
					config.Title = "MyDevCommanderApp";
				});

				_windowShellModel = _host.Services.GetService<IWindowsShellNotifications>();
				_windowShellModel.IsEnabled = true;
			}
			catch (Exception ex)
			{
				string message = "Woops - failed to start MyDevCommanderApp..!";
				MessageBox.Show(ex.FormatExceptionMessage(message), "MyDevCommanderApp", MessageBoxButton.OK, MessageBoxImage.Error);
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
