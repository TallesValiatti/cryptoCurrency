using cryptoCurrency.services.Services.BitCoinTradeService;
using cryptoCurrency.services.Services.NotifcationService;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using cryptoCurrency.core.Enums;

using static cryptoCurrency.core.Enums.EnumOrderStatus;

namespace cryptoCurrency.tasks.Tasks.TaskSell
{
    public class TaskSell : ITaskSell
    {
        #region variables
        private readonly ILogger<TaskSell> _logger;
        private readonly IBitCoinTradeService _bitCointTradeService;
        private readonly INotificationService _notificationService;
        #endregion

        #region methods
        public TaskSell(ILogger<TaskSell> logger, IBitCoinTradeService bitCoinTradeService, INotificationService notificationService)
        {
            this._logger = logger;
            this._bitCointTradeService = bitCoinTradeService;
            this._notificationService = notificationService;

        }
        public void Execute()
        {
            var stepsToWait = 7;

            //Get last order
            var lastOrder = _bitCointTradeService.GetLastOrder();

            //get balance
            var balanceCryptoCurrency = _bitCointTradeService.GetBalanceOfCryptoCurrency();

            //temp variable order ID
            var OrderId = string.Empty;
            var OrderPrice = (decimal)0;

            //try sell case no sell order was made
            if (lastOrder["status"].ToString() != EnumOrderStatusType.waiting.ToString())
            {
                _logger.LogInformation("Doing the first try to sell - {time}", DateTimeOffset.Now);

                //get the greatest book sell order
                var GreatestBookBuyOrder = _bitCointTradeService.GetBookSellOrders().OrderBy(X => ((decimal)X["UnitPrice"])).FirstOrDefault();

                //preapare the first Order
                var orderUnitPrice = ((decimal)GreatestBookBuyOrder["UnitPrice"]) - (decimal)0.05;
                var amount = balanceCryptoCurrency;
                var requestPrice = orderUnitPrice * amount;
                var Order = _bitCointTradeService.ExecuteSellOrder(orderUnitPrice, amount, requestPrice);

                //set the order ID
                OrderId = Order["id"].ToString();
                OrderPrice = Convert.ToDecimal(Order["unit_price"].ToString().Replace(",", "."), CultureInfo.InvariantCulture);
            }
            else
            {
                OrderId = lastOrder["id"].ToString();
                OrderPrice = Convert.ToDecimal(lastOrder["UnitPrice"].ToString().Replace(",", "."), CultureInfo.InvariantCulture);
            }

            //_bitCointTradeService.CancelOrder(OrderId);
            //give some time to sell the order
            Thread.Sleep(5000);

            //Notify that will start to try to buy
            _notificationService.RegularNotification("Start to try to sell ...");

            IDictionary<string, object> lastOrderAfterSomeTime = null;
            //loop to sell the order
            while (true)
            {
                //get my last order and verify id the order buy was executed
                lastOrderAfterSomeTime = _bitCointTradeService.GetLastOrder();
                if (string.Compare(lastOrderAfterSomeTime["status"].ToString(), EnumOrderStatusType.executed_completely.ToString()) == 0 && string.Compare(lastOrderAfterSomeTime["type"].ToString(), "sell") == 0)
                    break;

                //get the greatest book sell order
                var GreatestBookSellOrder = _bitCointTradeService.GetBookSellOrders().OrderBy(X => ((decimal)X["UnitPrice"])).FirstOrDefault();

                //verify if is the same price
                if (string.Compare(lastOrderAfterSomeTime["UnitPrice"].ToString(), GreatestBookSellOrder["UnitPrice"].ToString()) != 0)
                {
                    //verify if I can increase the price
                    if (_bitCointTradeService.canDecreaseOrderSellPrice(OrderPrice, Convert.ToDecimal(GreatestBookSellOrder["UnitPrice"].ToString().Replace(",", "."), CultureInfo.InvariantCulture)))
                    {
                        //get my last order and verify id the order buy was executed
                        lastOrderAfterSomeTime = _bitCointTradeService.GetLastOrder();
                        if (string.Compare(lastOrderAfterSomeTime["status"].ToString(), EnumOrderStatusType.executed_completely.ToString()) == 0)
                            break;

                        //cancel the order
                        _bitCointTradeService.CancelOrder(lastOrderAfterSomeTime["id"].ToString());

                        //preapare another Order
                        var orderUnitPrice = Math.Round(((decimal)GreatestBookSellOrder["UnitPrice"]), 2)- (decimal)0.01;
                        var amount = balanceCryptoCurrency;
                        var requestPrice = Math.Round(orderUnitPrice * amount, 2);
                        var Order = _bitCointTradeService.ExecuteSellOrder(orderUnitPrice, amount, requestPrice);
                    }
                    else
                    {
                        _logger.LogInformation("Cannot decrease the value" + " - {time} ", DateTimeOffset.Now);
                        //cancel the order
                        _bitCointTradeService.CancelOrder(lastOrderAfterSomeTime["id"].ToString());
                        continue;
                    }
                }
                else
                {
                    //get the 2º greatest book sell order
                    var SecondGreatestBookSellOrder = _bitCointTradeService.GetBookSellOrders().OrderBy(X => ((decimal)X["UnitPrice"])).ToList()[1];

                    //if 2º is greater than mine + 1
                    if (((decimal)SecondGreatestBookSellOrder["UnitPrice"]) > (((decimal)lastOrderAfterSomeTime["UnitPrice"]) + (decimal)0.01))
                    {
                        //cancel the order
                        _bitCointTradeService.CancelOrder(lastOrderAfterSomeTime["id"].ToString());

                        //preapare another Order
                        var orderUnitPrice = Math.Round(((decimal)SecondGreatestBookSellOrder["UnitPrice"]), 2) - (decimal)0.01;
                        var amount = balanceCryptoCurrency;
                        var requestPrice = Math.Round(orderUnitPrice * amount, 2);
                        var Order = _bitCointTradeService.ExecuteSellOrder(orderUnitPrice, amount, requestPrice);
                    }
                }
               
                
                //wait to avoid http 429
                if (stepsToWait == 0)
                {
                    stepsToWait = 7;
                    Thread.Sleep(5000);
                }

                stepsToWait--;
            }

            //get my last order and verify id the order buy was executed
            lastOrderAfterSomeTime = _bitCointTradeService.GetLastOrder();
            if (string.Compare(lastOrderAfterSomeTime["status"].ToString(), EnumOrderStatusType.executed_completely.ToString()) == 0 && string.Compare(lastOrderAfterSomeTime["type"].ToString(), "sell") == 0)
            {
                var msg = "Sould\nUnit Value: R$ " + lastOrderAfterSomeTime["UnitPrice"] + "\nTotal Value: R$ " + lastOrderAfterSomeTime["TotalPrice"];
                _logger.LogInformation(msg + " - {time} ", DateTimeOffset.Now);
                _notificationService.RegularNotification(msg);
            }

        }

        #endregion
    }
}
