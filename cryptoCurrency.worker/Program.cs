using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cryptoCurrency.services.Services.BitCoinTradeService;
using cryptoCurrency.services.Services.GenericServices;
using cryptoCurrency.tasks.Tasks;
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
                    services.AddScoped<IMainTask, MainTask>();
                    services.AddScoped<IBitCoinTradeService, BitCoinTradeService>();
                    services.AddScoped<IGenericService, GenericService>();
                    services.AddHostedService<Worker>();
                });
    }
}
