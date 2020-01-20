using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using cryptoCurrency.core.Enums;
using cryptoCurrency.core.Exceptions;
using cryptoCurrency.services.Services.BitCoinTradeService;
using cryptoCurrency.services.Services.CryptoCurrencyService;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using static cryptoCurrency.core.Enums.EnumBotState;
using static cryptoCurrency.core.Enums.EnumCryptoCurrency;
using static cryptoCurrency.core.Enums.EnumOrderStatus;
using cryptoCurrency.core.Enums;

namespace cryptoCurrency.services.Services.DecisonMakerService
{
    public class DecisionMakerService : IDecisionMakerService
    {
        #region Variables
        private readonly IBitCoinTradeService _bitCoinTradeService;
        private EnumCryptoCurrencyType _enumCryptoCurrencyType;
        private readonly ILogger<DecisionMakerService> _logger;
        private readonly ICryptoCurrencyService _cryptoCurrencyService;
        private decimal _highPercentToSell;
        private decimal _lowPercentToSell;
        private IMemoryCache _cache;
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

        public void SetPercentToSell(decimal low, decimal high)
        {
            if (low >= (decimal)100 || low <= (decimal)0.0)
                throw new CoreException("Low value must be between 0 and 100");
            if (high >= (decimal)100 || high <= (decimal)0.0)
                throw new CoreException("Low value must be between 0 and 100");

            this._lowPercentToSell = low;
            this._highPercentToSell= high;

        }
        public bool DecideIfShouldSell()
        {
            //todo: remover
            return true;

            //get the prices of last 24h
            var pricesLast24H = _cryptoCurrencyService.GetLast24HPricePerMin();

            // get the last order value
            var lastOrder = _bitCoinTradeService.GetLastOrder();

            //verify if the last order is: status: executedcompletly and type:buy

            if (((string)lastOrder["type"]) != "buy")
                throw new CoreException("To try to sell, the type of last order must be 'buy'.");
            if (((string)lastOrder["status"]) != EnumOrderStatusType.executed_completely.ToString())
                throw new CoreException("To try to sell, the status of last order must be 'executed_completely'.");

            //unit price
            var price = (decimal)lastOrder["UnitPrice"];

            //Number of spots
            var X = 2;

            //get the last X values 
            var lastValues = pricesLast24H.ToList().OrderBy(x => ((decimal)((JArray)x)[0])).Skip(pricesLast24H.Count() - X);

            //sell if the last X values are greater then high sell price (price * highSellValue)
            if (lastValues.All(x => ((decimal)((JArray)x)[1]) >= price * (1 + _highPercentToSell / 100)))
                return true;
            
            //sell if the last X values are lower then low sell price (price * highSellValue)
            if (lastValues.All(x => ((decimal)((JArray)x)[1]) <= price * (1 - _lowPercentToSell/ 100)))
                return true;

            return false;
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
