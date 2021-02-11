namespace CSN.Uwp.Core
{
    using CSN.Common.Repositories.Implementations;
    using CSN.Common.Repositories.Interfaces;
    using CSN.Uwp.Views.Settings;
    using CSN.Uwp.Views.Dashboard;
    using CSN.Uwp.Views.Start;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using CSN.Uwp.Repositories.Implementations;

    class IoCInitializer
    {
        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        public static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Repositories
            services.AddSingleton<IDeviceConsumptionRepository, DeviceConsumptionRepository>();
            services.AddSingleton<ISettingsRepository, SettingsRepository>();   

            // Services
            services.AddSingleton(typeof(NavigationService));
            services.AddSingleton(typeof(ScheduledTaskService));

            // ViewModels
            services.AddSingleton(typeof(StartPageViewModel));
            services.AddSingleton(typeof(DashboardPageViewModel));
            services.AddSingleton(typeof(SettingsPageViewModel));

            return services.BuildServiceProvider();
        }

    }
}
