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
using cryptoCurrency.tasks.Tasks.TaskBuy;
using cryptoCurrency.tasks.Tasks.TaskSell;
using cryptoCurrency.tasks.Tasks.TaskAwaitToSell;

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
        private bool _notificateBotIsAlive;
        private readonly ITaskBuy _buyTask;
        private readonly ITaskSell _sellTask;
        private readonly ITaskAwaitToSell _awaiToSellTask;
        #endregion

        #region methods

        public MainTask(
            ILogger<MainTask> logger,
            IBitCoinTradeService bitCoinTradeService,
            IGenericService genericService,
            INotificationService notificationService,
            IDecisionMakerService decisionMakerService,
            ITaskAwaitToBuy awaitToBuyTask,
            ITaskBuy BuyTask,
            ITaskSell SellTask,
            ITaskAwaitToSell awaiToSellTask,
            ICryptoCurrencyService  cryptoCurrencyService
            )
        {
            _notificateBotIsAlive = true;
            this._logger = logger;
            this._bitCoinTradeService = bitCoinTradeService;
            this._genericService = genericService;
            this._notificationService = notificationService;
            this._decisionMakerService = decisionMakerService;
            this._awaitToBuyTask = awaitToBuyTask;
            this._buyTask = BuyTask;
            this._cryptoCurrencyService = cryptoCurrencyService;
            this._sellTask = SellTask;
            this._awaiToSellTask = awaiToSellTask;


        }

        public void Execute(dynamic objData)
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

                // set the Percent Buy Order Limit
                _bitCoinTradeService.SetPercentBuyOrderLimit((decimal)_genericService.getObjectFromDynamic("SetPercentBuyOrderLimit", objData));

                // set the Percent sell Order Limit
                _bitCoinTradeService.SetPercentSellOrderLimit((decimal)_genericService.getObjectFromDynamic("SetPercentSellOrderLimit", objData));

                // set the private key of notification service
                _notificationService.SetKey((string)_genericService.getObjectFromDynamic("NotificationKey", objData));

                //set the buy value order
                _bitCoinTradeService.SetOrderValue((decimal)_genericService.getObjectFromDynamic("BuyValueOrder", objData));

                //Notificate bot is alive
                if (_notificateBotIsAlive)
                {   
                    _notificationService.BotIsAliveNotification();
                    _notificateBotIsAlive = false;
                }

                //Retrive the crypto currency type enum
                EnumCryptoCurrencyType enumType;

                var canConvert = Enum.TryParse<EnumCryptoCurrencyType>((string)_genericService.getObjectFromDynamic("EnumCryptoCurrencyType", objData), out enumType);
                if (!canConvert)
                    throw new CoreException("crypto currency not defined on enum: " + (string)_genericService.getObjectFromDynamic("EnumCryptoCurrencyType", objData));

                // set the crypto Currency type
                _decisionMakerService.SetCryptoCurrencyTypeEnum(enumType);

                // set the crypto Currency URL decision maker
                _decisionMakerService.SetURLdecisonMaker((string)_genericService.getObjectFromDynamic("URLdecisonMaker", objData));

                // set the values to sell
                _decisionMakerService.SetPercentToSell((decimal)_genericService.getObjectFromDynamic("lowToSell", objData), (decimal)_genericService.getObjectFromDynamic("highToSell", objData));

                // set the crypto Currency type
                _bitCoinTradeService.SetCryptoCurrencyTypeEnum(enumType);

                //set the crypto Currency type
                _cryptoCurrencyService.SetCryptoCurrencyType(enumType);

                // ***************************************  Decide which State to Go ******************************
                var state = _decisionMakerService.DecideWhichStateToGo();

                if (state == EnumBotState.EnumBotStateType.awaitToBuy)
                    _awaitToBuyTask.Execute();
                else if (state == EnumBotState.EnumBotStateType.tryToBuy)
                    _buyTask.Execute();
                else if (state == EnumBotState.EnumBotStateType.awaitToSell)
                    _awaiToSellTask.Execute();
                else if (state == EnumBotState.EnumBotStateType.tryToSell)
                    _sellTask.Execute();

                // ************************************************ End TASK ***************************************
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
