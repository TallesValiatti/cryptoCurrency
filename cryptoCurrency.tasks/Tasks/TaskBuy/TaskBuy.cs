using cryptoCurrency.core.Exceptions;
using cryptoCurrency.services.Services.BitCoinTradeService;
using cryptoCurrency.services.Services.NotifcationService;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using static cryptoCurrency.core.Enums.EnumOrderStatus;

namespace cryptoCurrency.tasks.Tasks.TaskBuy
{
    public class TaskBuy : ITaskBuy
    {
        #region variables
        private readonly ILogger<TaskBuy> _logger;
        private readonly IBitCoinTradeService _bitCointTradeService;
        private readonly INotificationService _notificationService;
        #endregion

        #region methods

        public TaskBuy(ILogger<TaskBuy> logger, IBitCoinTradeService bitCoinTradeService, INotificationService notificationService)
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

            //temp variable order ID
            var OrderId = string.Empty;
            var OrderPrice = (decimal)0;

            //try buy case no buy order was made
            if (lastOrder.Count() == 0 || lastOrder["status"].ToString() != EnumOrderStatusType.waiting.ToString())
            {
                if (!_bitCointTradeService.verifyIfBotHasMoney())
                    return;

                _logger.LogInformation("Doing the first try to buy - {time}", DateTimeOffset.Now);

                //get the greatest book buy order
                var GreatestBookBuyOrder = _bitCointTradeService.GetBookBuyOrders().OrderByDescending(X => ((decimal)X["UnitPrice"])).FirstOrDefault();

                //preapare the first Order
                var orderUnitPrice = ((decimal)GreatestBookBuyOrder["UnitPrice"]) + (decimal)0.05;
                var requestPrice = _bitCointTradeService.GetOrderValue();
                var amount = Math.Round(requestPrice / orderUnitPrice, 8);
                var Order = _bitCointTradeService.ExecuteBuyOrder(orderUnitPrice, amount, requestPrice);

                //set the order ID
                OrderId = Order["id"].ToString();
                OrderPrice = Convert.ToDecimal(Order["unit_price"].ToString().Replace(",",".") ,CultureInfo.InvariantCulture);
                
            }
            else
            {
                OrderId = lastOrder["id"].ToString();
                OrderPrice = Convert.ToDecimal(lastOrder["UnitPrice"].ToString().Replace(",", "."), CultureInfo.InvariantCulture);
            }

            //_bitCointTradeService.CancelOrder(OrderId);
            
            //give some time to buy the order
            Thread.Sleep(5000);

            //Notify that will start to try to buy
            _notificationService.RegularNotification("Start to try to buy ...");

            IDictionary<string, object> lastOrderAfterSomeTime = null;
            //loop to buy the order
            while (true)
            {
                //get my last order and verify id the order buy was executed
                lastOrderAfterSomeTime = _bitCointTradeService.GetLastOrder();
                if (string.Compare(lastOrderAfterSomeTime["status"].ToString(), EnumOrderStatusType.executed_completely.ToString()) == 0 && string.Compare( lastOrderAfterSomeTime["type"].ToString(), "buy") == 0)
                    break;

                //get the greatest book buy order
                var GreatestBookBuyOrder = _bitCointTradeService.GetBookBuyOrders().OrderByDescending(X => ((decimal)X["UnitPrice"])).FirstOrDefault();

                //verify if is the same price
                if(string.Compare(lastOrderAfterSomeTime["UnitPrice"].ToString(), GreatestBookBuyOrder["UnitPrice"].ToString()) != 0)
                {
                    //verify if I can increase the price
                    if (_bitCointTradeService.canIncreaseOrderBuyPrice(OrderPrice, Convert.ToDecimal(GreatestBookBuyOrder["UnitPrice"].ToString().Replace(",", "."), CultureInfo.InvariantCulture)))
                    {
                        //get my last order and verify id the order buy was executed
                        lastOrderAfterSomeTime = _bitCointTradeService.GetLastOrder();
                        if (string.Compare(lastOrderAfterSomeTime["status"].ToString(), EnumOrderStatusType.executed_completely.ToString()) == 0 && string.Compare(lastOrderAfterSomeTime["type"].ToString(), "buy") == 0)
                            break;

                        //cancel the order
                        _bitCointTradeService.CancelOrder(lastOrderAfterSomeTime["id"].ToString());

                        //preapare another Order
                        var orderUnitPrice = ((decimal)GreatestBookBuyOrder["UnitPrice"]) + (decimal)0.01;
                        var requestPrice = _bitCointTradeService.GetOrderValue();
                        var amount = Math.Round(requestPrice / orderUnitPrice, 8);
                        var Order = _bitCointTradeService.ExecuteBuyOrder(orderUnitPrice, amount, requestPrice);
                    }
                    else
                    {
                        _logger.LogInformation("Cannot increase the value" + " - {time} ", DateTimeOffset.Now);
                        //cancel the order
                        _bitCointTradeService.CancelOrder(lastOrderAfterSomeTime["id"].ToString());
                        continue;
                    }
                }
                else
                {
                    //get the 2º greatest book buy order
                    var SecondGreatestBookBuyOrder = _bitCointTradeService.GetBookBuyOrders().OrderByDescending(X => ((decimal)X["UnitPrice"])).ToList()[1];

                    //if 2º is lower than mine - 1
                    if (((decimal)SecondGreatestBookBuyOrder["UnitPrice"]) < (((decimal)lastOrderAfterSomeTime["UnitPrice"]) - (decimal)0.01))
                    {
                        //cancel the order
                        _bitCointTradeService.CancelOrder(lastOrderAfterSomeTime["id"].ToString());

                        //preapare another Order
                        var orderUnitPrice = ((decimal)SecondGreatestBookBuyOrder["UnitPrice"]) + (decimal)0.01;
                        var requestPrice = _bitCointTradeService.GetOrderValue();
                        var amount = Math.Round(requestPrice / orderUnitPrice, 8);
                        var Order = _bitCointTradeService.ExecuteBuyOrder(orderUnitPrice, amount, requestPrice);
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
            if (string.Compare(lastOrderAfterSomeTime["status"].ToString(), EnumOrderStatusType.executed_completely.ToString()) == 0 && string.Compare(lastOrderAfterSomeTime["type"].ToString(), "buy") == 0)
            {
                var msg = "Bougth\nUnit Value: R$ " + lastOrderAfterSomeTime["UnitPrice"] + "\nRequested: " + lastOrderAfterSomeTime["RequestedAmount"] + "\nTotal value: " + lastOrderAfterSomeTime["TotalPrice"];
                _logger.LogInformation(msg + " - {time} ", DateTimeOffset.Now);
                _notificationService.RegularNotification(msg);
            }

        }

        #endregion


    }


}
