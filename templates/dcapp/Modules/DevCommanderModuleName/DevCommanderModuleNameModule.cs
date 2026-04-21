using Eyjo.DevCommander.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace DevCommanderModuleNameApp.Modules.DevCommanderModuleName
{
	public class DevCommanderModuleNameModule(ILogger<DevCommanderModuleNameModule> logger, IShell shell) : IDevCommanderModule
	{
		private readonly ILogger<DevCommanderModuleNameModule> _logger = logger;
		private readonly IShell _shell = shell;

		/// <inheritdoc/>
		public string Name => "DevCommanderModuleName";

		/// <summary>The DevCommander service registration method.</summary>
		/// <param name="context">Context containing the common services on the IHost.</param>
		/// <param name="services">Specifies the contract for a collection of service descriptors.</param>
		public static void ServiceRegistration(HostBuilderContext context, IServiceCollection services)
		{
			// Register your types for the dependency injection composition root here.
			services
				.AddTransient<DevCommanderModuleNameView>()
				.AddTransient<DevCommanderModuleNameViewModel>();
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		/// <inheritdoc/>
		public void OnStart()
		{
			_logger.LogDebug("Hello from {Name}", Name);
			_shell.RegisterMainView<DevCommanderModuleNameView, DevCommanderModuleNameViewModel>("DevCommanderModuleName View", "\\DevCommanderModuleName Menu\\");

			// TODO: Remove this DEBUG-builds-only clause to launch the view upon start in release mode too.
#if DEBUG
			_shell.DisplayMainView<DevCommanderModuleNameView, DevCommanderModuleNameViewModel>();
#endif
		}
	}
}