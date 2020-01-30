using FabelioScrape.Web.Infrastructure;
using FabelioScrape.Web.Models.Products;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FabelioScrape.Web
{
    public class UpdateProductBackgroundService : BackgroundService
    {
        private readonly ILogger<UpdateProductBackgroundService> _logger;
        private readonly IServiceProvider _services;
        private Timer _timer;
        private int _executionCount;

        public UpdateProductBackgroundService(ILogger<UpdateProductBackgroundService> logger, IServiceProvider services)
        {
            _logger = logger;
            _services = services;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fabelio.com Products Content Update Service running.");

            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fabelio.com Products Content Update Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return base.StopAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.FromSeconds(20), TimeSpan.FromMinutes(1));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            try
            {
                var count = Interlocked.Increment(ref _executionCount);

                _logger.LogInformation(
                    "Execute: {Count}", count);

                using var scope = _services.CreateScope();
                var clientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
                var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                var appBaseUrl = config.GetValue<string>("CurrentHost");
                var syncTime = string.Format("{0:s}{0:zzz}", DateTime.Now);
                var client = clientFactory.CreateClient();
                client.BaseAddress = new Uri(appBaseUrl);
                client.PostAsync("/products/sync?syncTime="+syncTime, new StringContent("")).Wait();
            }
            catch
            {

            }
        }

    }
}
