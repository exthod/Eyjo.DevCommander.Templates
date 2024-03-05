using Eyjo.DevCommander.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace MyDevCommanderApp.Modules.MyModule
{
	public class MyModule : IDevCommanderModule
    {
        private readonly IShell _shell;

		/// <summary>Constructor taking an IShell instance.</summary>
		/// <param name="shell"></param>
		public MyModule(IShell shell)
		{
            _shell = shell;
		}

		/// <inheritdoc/>
		public string Name => "DevCommanderModuleName";

        /// <summary>The DevCommander service registration method.</summary>
        /// <param name="context">Context containing the common services on the IHost.</param>
        /// <param name="services">Specifies the contract for a collection of service descriptors.</param>
        public static void ServiceRegistration(HostBuilderContext context, IServiceCollection services)
        {
            // Register your types for the dependency injection composition root here.
            services
                .AddTransient<MyModuleView>()
                .AddTransient<MyModuleViewModel>();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

		/// <inheritdoc/>
		public void OnStart()
        {
            _shell.RegisterMainView<MyModuleView, MyModuleViewModel>("My Module View", "\\My Menu\\");
#if DEBUG
            _shell.DisplayMainView<MyModuleView, MyModuleViewModel>();
#endif
        }
	}
}