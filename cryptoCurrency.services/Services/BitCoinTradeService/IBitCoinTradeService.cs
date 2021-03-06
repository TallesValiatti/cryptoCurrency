﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using cryptoCurrency.core.Enums;

namespace cryptoCurrency.services.Services.BitCoinTradeService
{
    public interface IBitCoinTradeService
    {
        void SetKey(string key);
        void SetCryptoCurrencyTypeEnum(EnumCryptoCurrency.EnumCryptoCurrencyType enumType);
        IDictionary<string, decimal> GetBalance();
        decimal GetBalanceOfCryptoCurrency();
        IDictionary<string, Object> GetLastOrder();
        void SetOrderValue(decimal value);
        decimal GetOrderValue();
        IDictionary<string, Object> ExecuteBuyOrder(decimal unitPrice, decimal amount, decimal requestPrice);
        IDictionary<string, Object> ExecuteSellOrder(decimal unitPrice, decimal amount, decimal requestPrice);
        bool verifyIfBotHasMoney();
        IEnumerable<IDictionary<string, Object>> GetBookBuyOrders();
        IEnumerable<IDictionary<string, Object>> GetBookSellOrders();
        void SetPercentBuyOrderLimit(decimal percent);
        void SetPercentSellOrderLimit(decimal percent);
        bool canIncreaseOrderBuyPrice(decimal firstPrice, decimal currentPrice);
        bool canDecreaseOrderSellPrice(decimal firstPrice, decimal currentPrice);
        void CancelOrder(string orderId);
    }
}
