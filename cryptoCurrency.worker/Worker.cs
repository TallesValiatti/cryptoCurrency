using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using cryptoCurrency.tasks.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace cryptoCurrency.worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IMainTask _mainTask;
        private readonly IConfiguration _config;
        private ServiceProvider _serviceProvider;

        public Worker(ILogger<Worker> logger, IMainTask mainTask, IConfiguration config)
        {
            _logger = logger;
            _mainTask = mainTask;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //bitcointrade key

                var objData = new
                { 
                    key = _config.GetValue<string>("data:key"),
                    threadTime = _config.GetValue<long>("data:threadTime"),
                };

                //main tasks
                _mainTask.ExecuteAsync(objData);

                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay((int)objData.threadTime, stoppingToken);
            }
        }
    }
}
