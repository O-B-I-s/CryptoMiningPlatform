namespace CryptoMining.API.Services
{
    public class ProfitCalculationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ProfitCalculationService> _logger;

        public ProfitCalculationService(
            IServiceProvider serviceProvider,
            ILogger<ProfitCalculationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var miningService = scope.ServiceProvider.GetRequiredService<IMiningService>();

                    await miningService.CalculateProfitsAsync();
                    _logger.LogInformation("Profit calculation completed at {Time}", DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error calculating profits");
                }

                // Run every hour (or adjust as needed)
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
