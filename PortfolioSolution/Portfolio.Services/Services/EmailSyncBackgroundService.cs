using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Portfolio.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Services.Services
{
    public class EmailSyncBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailSyncBackgroundService> _logger;
        private readonly TimeSpan _syncInterval = TimeSpan.FromMinutes(5); // Sync every 5 minutes

        public EmailSyncBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<EmailSyncBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email Sync Background Service is starting");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var emailSyncService = scope.ServiceProvider.GetRequiredService<IEmailSyncService>();

                    await emailSyncService.SyncEmailsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during email sync");
                }

                await Task.Delay(_syncInterval, stoppingToken);
            }

            _logger.LogInformation("Email Sync Background Service is stopping");
        }
    }

}
