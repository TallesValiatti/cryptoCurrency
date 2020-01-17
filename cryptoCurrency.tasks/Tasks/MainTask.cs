using System;
using System.Threading.Tasks;
using cryptoCurrency.core.Exceptions;
using cryptoCurrency.core.Enums;
using cryptoCurrency.services.Services.BitCoinTradeService;
using cryptoCurrency.services.Services.GenericServices;
using cryptoCurrency.services.Services.NotifcationService;
using Microsoft.Extensions.Logging;
using static cryptoCurrency.core.Enums.EnumCryptoCurrency;
using cryptoCurrency.services.Services.DecisonMakerService;
using cryptoCurrency.tasks.Tasks.TaskAwaitToBuy;
using cryptoCurrency.services.Services.CryptoCurrencyService;

namespace cryptoCurrency.tasks.Tasks
{
    public class MainTask : IMainTask
    {
        #region variables
        private readonly ILogger<MainTask> _logger;
        private readonly IBitCoinTradeService _bitCoinTradeService;
        private readonly IGenericService _genericService;
        private readonly INotificationService _notificationService;
        private readonly IDecisionMakerService _decisionMakerService;
        private readonly ITaskAwaitToBuy _awaitToBuyTask;
        private readonly ICryptoCurrencyService _cryptoCurrencyService;
        #endregion

        #region methods

        public MainTask(
            ILogger<MainTask> logger,
            IBitCoinTradeService bitCoinTradeService,
            IGenericService genericService,
            INotificationService notificationService,
            IDecisionMakerService decisionMakerService,
            ITaskAwaitToBuy awaitToBuyTask,
            ICryptoCurrencyService  cryptoCurrencyService
            )
        {
            this._logger = logger;
            this._bitCoinTradeService = bitCoinTradeService;
            this._genericService = genericService;
            this._notificationService = notificationService;
            this._decisionMakerService = decisionMakerService;
            this._awaitToBuyTask = awaitToBuyTask;
            this._cryptoCurrencyService = cryptoCurrencyService;
        }

        public async Task ExecuteAsync(dynamic objData)
        {
            try
            {
                // ***************************************  Init services ***************************************
                //new Task
                _logger.LogInformation("******************** NEW TASK *********************");
                //Starting task
                _logger.LogInformation("Starting main task - {time}", DateTimeOffset.Now);

                // set the private Key of the service BITCOIN TRADE
                _bitCoinTradeService.SetKey((string)_genericService.getObjectFromDynamic("TradeKey", objData));

                // set the private key of notification service
                _notificationService.SetKey((string)_genericService.getObjectFromDynamic("NotificationKey", objData));

                //Retrive the crypto currency type enum
                EnumCryptoCurrencyType enumType;

                var canConvert = Enum.TryParse<EnumCryptoCurrencyType>((string)_genericService.getObjectFromDynamic("EnumCryptoCurrencyType", objData), out enumType);
                if (!canConvert)
                    throw new CoreException("crypto currency not defined on enum: " + (string)_genericService.getObjectFromDynamic("EnumCryptoCurrencyType", objData));

                // set the crypto Currency type
                _bitCoinTradeService.SetCryptoCurrencyTypeEnum(enumType);

                //set the crypto Currency type
                _cryptoCurrencyService.SetCryptoCurrencyType(enumType);

                // ***************************************  Decide which State to Go ******************************
                var state = _decisionMakerService.DecideWhichStateToGo();

                if (state == EnumBotState.EnumBotStateType.awaitToBuy)
                    _awaitToBuyTask.Execute();
                    


                // Ending task
                _logger.LogInformation("Ending main task - {time}", DateTimeOffset.Now);
            }
            catch (CoreException cex)
            {
                _logger.LogError("CoreException - " + cex.Message + " - {time}", DateTimeOffset.Now);
                _notificationService.ErrorNotification(cex.Message);
            }
            catch(Exception ex)
            {
                _logger.LogError("Exeception - " + ex.Message + " - {time}", DateTimeOffset.Now);
                _notificationService.ErrorNotification(ex.Message);
            }
        }

        #endregion
    }
}
