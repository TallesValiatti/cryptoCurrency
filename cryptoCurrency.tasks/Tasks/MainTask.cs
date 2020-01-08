using System;
using System.Threading.Tasks;
using cryptoCurrency.core.Exceptions;
using cryptoCurrency.services.Services.BitCoinTradeService;
using cryptoCurrency.services.Services.GenericServices;
using Microsoft.Extensions.Logging;

namespace cryptoCurrency.tasks.Tasks
{
    public class MainTask : IMainTask
    {
        #region variables
        private readonly ILogger<MainTask> _logger;
        private readonly IBitCoinTradeService _bitCoinTradeService;
        private readonly IGenericService _genericService;
        #endregion

        #region methods

        public MainTask(
            ILogger<MainTask> logger,
            IBitCoinTradeService bitCoinTradeService,
            IGenericService genericService
            )
        {
            this._logger = logger;
            this._bitCoinTradeService = bitCoinTradeService;
            this._genericService = genericService;
        }

        public async Task ExecuteAsync(dynamic objData)
        {
            try
            {
                //new Task
                _logger.LogInformation("******************** NEW TASK *********************");
                //Starting task
                _logger.LogInformation("Starting main task - {time}", DateTimeOffset.Now);

                // set the private Key of the service BITCOIN TRADE
                _bitCoinTradeService.setKey((string)_genericService.getObjectFromDynamic("key", objData));

                //Get balance
                var balance =  await _bitCoinTradeService.getBalanceAsync();

                // Ending task
                _logger.LogInformation("Ending main task - {time}", DateTimeOffset.Now);
            }
            catch (CoreException cex)
            {
                _logger.LogError("CoreException - "+ cex.Message + " -{time}", DateTimeOffset.Now);
            }
            catch(Exception ex)
            {
                _logger.LogError("Exeception - " + ex.Message + " - {time}", DateTimeOffset.Now);
            }
        }

        #endregion
    }
}
