using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using cryptoCurrency.core.Enums;
using cryptoCurrency.services.Services.BitCoinTradeService;
using cryptoCurrency.services.Services.CryptoCurrencyService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
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

            //Get the amount
            var amount = _bitCoinTradeService.GetBalance();

            //Get balance of account
            var balance = _bitCoinTradeService.GetBalance();

            //Get the last order of crypto currency
            var lastOrder = _bitCoinTradeService.GetLastOrder();

            // ***************************************************   State -> awaitTosell  ***************************************************
            if (
                        amount[_enumCryptoCurrencyType.ToString()] != (decimal)0 && //Amount remain on bit coin trade
                        (
                            String.Compare(lastOrder["status"].ToString(), EnumOrderStatusType.canceled.ToString()) == 0 || // status of last order equal to canceled
                            String.Compare(lastOrder["status"].ToString(), EnumOrderStatusType.executed_completely.ToString()) == 0  || // status of last order equal to executed_completely
                            lastOrder.Count == 0  //No order done yet
                        )
                )
            {
                return EnumBotStateType.awaitToSell;
            }

            // ***************************************************   State -> try to  Sell  ***************************************************
            else if (
                    String.Compare(lastOrder["status"].ToString(), EnumOrderStatusType.waiting.ToString()) == 0 && // wait
                    String.Compare(lastOrder["type"].ToString(), "sell") == 0
                )
            {
                return EnumBotStateType.tryToSell;
            }

            // ***************************************************   State -> try to  buy  ***************************************************
            else if(
                    String.Compare(lastOrder["status"].ToString(), EnumOrderStatusType.waiting.ToString()) == 0 && // wait
                    String.Compare(lastOrder["type"].ToString(), "buy") == 0
                )
            {
                return EnumBotStateType.tryToBuy;
            }
            // ***************************************************   State -> awaitToBuy  ***************************************************
            else if (
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

            // ***************************************************   State -> No State  ***************************************************
            return EnumBotStateType.NoState;
        }

        public void SetCryptoCurrencyTypeEnum(EnumCryptoCurrency.EnumCryptoCurrencyType enumType)
        {
            _enumCryptoCurrencyType = enumType;
        }

        public bool DecideIfShouldSell()
        {
            throw new NotImplementedException();
        }

        public  bool DecideIfShouldBuy()
        {
            //todo:remove
            return true;

            var pricesLast24H = _cryptoCurrencyService.GetLast24HPricePerMin();

            //get the last 2h
            var pricesLast2h = pricesLast24H.Skip(pricesLast24H.Count() - 120);

            var firstGroupPriceMean = pricesLast2h.Take(60).Select(x => ((decimal)((JArray)x)[1])).Sum()/(decimal)60;
            var SecondGroupPriceMean = pricesLast2h.Skip(60).Select(x => ((decimal)((JArray)x)[1])).Sum()/(decimal)60;

            //2%
            var limitToBuy = firstGroupPriceMean * (decimal)1.02;

            return SecondGroupPriceMean >= limitToBuy ? true:false;
        }
        #endregion

        #region conversion Class
        
        #endregion
    }
}
