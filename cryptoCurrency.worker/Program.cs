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
                    services.AddScoped<INotificationService, NotifcationService>();
                    services.AddScoped<IBitCoinTradeService, BitCoinTradeService>();
                    services.AddScoped<IGenericService, GenericService>();
                    services.AddScoped<IDecisionMakerService, DecisionMakerService>(); 
                    services.AddScoped<ICryptoCurrencyService, CryptoCurrencyService>();
                  
                    //tasks
                    services.AddScoped<IMainTask, MainTask>();
                    services.AddScoped<ITaskAwaitToBuy, TaskAwaitToBuy>();

                    services.AddHostedService<Worker>();
                });
    }
}
