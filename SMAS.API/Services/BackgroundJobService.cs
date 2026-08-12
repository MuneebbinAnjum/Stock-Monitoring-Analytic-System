using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace SMAS.API.Services
{
    public class BackgroundJobService : IHostedService, IDisposable
    {
        private readonly ILogger<BackgroundJobService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private Timer? _timer;

        public BackgroundJobService(ILogger<BackgroundJobService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Background job service starting.");
            _timer = new Timer(async _ => await ExecuteAsync(), null, TimeSpan.Zero, TimeSpan.FromMinutes(15));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Background job service stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private async Task ExecuteAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var alertService = scope.ServiceProvider.GetRequiredService<IAlertService>();
                var forecastService = scope.ServiceProvider.GetRequiredService<IForecastService>();

                await alertService.TriggerAlertsCheckAsync();
                await forecastService.UpdateTrendScoresAsync();
                _logger.LogInformation("Background job completed alert and forecast checks.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing background jobs.");
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}