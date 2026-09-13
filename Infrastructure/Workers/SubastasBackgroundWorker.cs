using Application.UseCases.Subastas.LiquidarSubastasVencidas;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Workers
{
    public class SubastasBackgroundWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SubastasBackgroundWorker> _logger;
        private readonly TimeSpan _intervalo = TimeSpan.FromSeconds(15);

        public SubastasBackgroundWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<SubastasBackgroundWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SubastasBackgroundWorker iniciado. Verificando subastas cada {Segundos} segundos.", _intervalo.TotalSeconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<LiquidarSubastasVencidasCommand>>();
                        await handler.HandleAsync(new LiquidarSubastasVencidasCommand(), stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error no controlado durante la ejecución del SubastasBackgroundWorker.");
                }

                await Task.Delay(_intervalo, stoppingToken);
            }

            _logger.LogInformation("SubastasBackgroundWorker detenido.");
        }
    }
}
