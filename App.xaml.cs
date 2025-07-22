using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows;
using TrainBookingAutomation.Services;
using TrainBookingAutomation.Windows;

namespace TrainBookingAutomation
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Logging
            services.AddLogging(builder => builder.AddDebug());

            // Services
            services.AddSingleton<ITrainDataService, TrainDataService>();
            services.AddSingleton<IBookingEngine, BookingEngine>();
            services.AddSingleton<IAIRecommendationService, AIRecommendationService>();
            services.AddSingleton<IPriceOptimizationService, PriceOptimizationService>();
            services.AddSingleton<INotificationService, NotificationService>();

            // Windows
            services.AddTransient<MainWindow>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
} 