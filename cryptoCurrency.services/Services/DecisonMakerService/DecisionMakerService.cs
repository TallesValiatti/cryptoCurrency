using System;
using System.Linq;
using System.Threading.Tasks;
using cryptoCurrency.core.Enums;
using cryptoCurrency.services.Services.BitCoinTradeService;
using cryptoCurrency.services.Services.CryptoCurrencyService;
using Microsoft.Extensions.Logging;
using static cryptoCurrency.core.Enums.EnumBotState;
using static cryptoCurrency.core.Enums.EnumCryptoCurrency;
using static cryptoCurrency.core.Enums.EnumOrderStatus;

namespace cryptoCurrency.services.Services.DecisonMakerService
{
    public class DecisionMakerService : IDecisionMakerService
    {
        #region Variables
        private readonly IBitCoinTradeService _bitCoinTradeService;
        private EnumCryptoCurrencyType _enumCryptoCurrencyType;
        private readonly ILogger<DecisionMakerService> _logger;
        private readonly ICryptoCurrencyService _cryptoCurrencyService;
        #endregion

        #region methods

        public DecisionMakerService(
            IBitCoinTradeService bitCoinTradeService,
            ILogger<DecisionMakerService> logger,
            ICryptoCurrencyService cryptoCurrencyService)
        {
            _bitCoinTradeService = bitCoinTradeService;
            _logger = logger;
            _cryptoCurrencyService = cryptoCurrencyService; 
        }

        public EnumBotStateType DecideWhichStateToGo()
        {
            _logger.LogInformation("Decide Which State To Go - {time}", DateTimeOffset.Now);

            //Get balance of account
            var taskBalance = _bitCoinTradeService.GetBalanceAsync();

            //Get the last order of crypto currency
            var taskLastOrder = _bitCoinTradeService.GetLastOrderAsync();

            taskBalance.Wait();
            taskLastOrder.Wait();

            var balance = taskBalance.Result;
            var lastOrder = taskLastOrder.Result;

            // ***************************************************   State -> awaitToBuy  ***************************************************
            if (
                     lastOrder.Count == 0  ||  //No order done yet
                    (
                        balance[_enumCryptoCurrencyType.ToString()] == (decimal)0 && //No amount remain on bit coin trade
                        (
                            String.Compare(lastOrder["status"].ToString(), EnumOrderStatusType.canceled.ToString())== 0 || // status of last order equal to canceled
                            String.Compare(lastOrder["status"].ToString(), EnumOrderStatusType.executed_completely.ToString()) == 0  // status of last order equal to executed_completely
                        )
                    )  
                )
            {
                return EnumBotStateType.awaitToBuy;
            }
            else
            {

                //Verify if exists amount at the crypto currency
                //var amount = balance[_enumCryptoCurrencyType.ToString()];

                //Exist some order
                var b = 1;
            }

            return EnumBotStateType.awaitToBuy;
        }

        public void SetCryptoCurrencyTypeEnum(EnumCryptoCurrency.EnumCryptoCurrencyType enumType)
        {
            _enumCryptoCurrencyType = enumType;
        }

        public bool DecideIfShouldSell()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DecideIfShouldBuy()
        {
            var pricesLast48H = await _cryptoCurrencyService.GetLast48HPrice();

            //get the last 2h
            var pricesLast2h = pricesLast48H.Skip(pricesLast48H.Count() - 120);
            var count = pricesLast48H.Count();
            var dataUltimo = pricesLast2h.Last();



            return true;
        }
        #endregion
    }
}
