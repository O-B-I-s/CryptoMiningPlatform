namespace CryptoMining.API.Services
{
    public class ProfitCalculationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ProfitCalculationService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // Check every minute

        public ProfitCalculationService(
            IServiceProvider serviceProvider,
            ILogger<ProfitCalculationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Profit Calculation Service started. Checking every {Interval} minute(s)",
                _checkInterval.TotalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var miningService = scope.ServiceProvider.GetRequiredService<IMiningService>();

                    await miningService.CalculateProfitsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in profit calculation service");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Profit Calculation Service stopped");
        }
    }
}
