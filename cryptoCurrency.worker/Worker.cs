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
                    TradeKey = _config.GetValue<string>("data:TradeKey"),
                    ThreadTime = _config.GetValue<long>("data:ThreadTime"),
                    NotificationKey = _config.GetValue<string>("data:NotificationKey"),
                    EnumCryptoCurrencyType = _config.GetValue<string>("data:EnumCryptoCurrencyType"),
                    BuyValueOrder = _config.GetValue<decimal>("data:BuyValueOrder"),
                    SetPercentBuyOrderLimit = _config.GetValue<decimal>("data:SetPercentBuyOrderLimit")
                };

                //main tasks
                await Task.Run(() => _mainTask.Execute(objData)); 

                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay((int)objData.ThreadTime, stoppingToken);
            }
        }
    }
}
