using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cryptoCurrency.services.Services.BitCoinTradeService;
using cryptoCurrency.services.Services.CryptoCurrencyService;
using cryptoCurrency.services.Services.DecisonMakerService;
using cryptoCurrency.services.Services.GenericServices;
using cryptoCurrency.services.Services.NotifcationService;
using cryptoCurrency.tasks.Tasks;
using cryptoCurrency.tasks.Tasks.TaskAwaitToBuy;
using cryptoCurrency.tasks.Tasks.TaskSell;
using cryptoCurrency.tasks.Tasks.TaskAwaitToSell;
using cryptoCurrency.tasks.Tasks.TaskBuy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace cryptoCurrency.worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    //Services
                    services.AddSingleton<INotificationService, NotifcationService>();
                    services.AddSingleton<IBitCoinTradeService, BitCoinTradeService>();
                    services.AddSingleton<IGenericService, GenericService>();
                    services.AddSingleton<IDecisionMakerService, DecisionMakerService>(); 
                    services.AddSingleton<ICryptoCurrencyService, CryptoCurrencyService>();
                  
                    //tasks
                    services.AddSingleton<IMainTask, MainTask>();
                    services.AddSingleton<ITaskAwaitToBuy, TaskAwaitToBuy>();
                    services.AddSingleton<ITaskBuy, TaskBuy>();
                    services.AddSingleton<ITaskSell, TaskSell>();
                    services.AddSingleton<ITaskAwaitToSell, TaskAwaitToSell>();

                    //hosted Service
                    services.AddHostedService<Worker>();
                });
    }
}
