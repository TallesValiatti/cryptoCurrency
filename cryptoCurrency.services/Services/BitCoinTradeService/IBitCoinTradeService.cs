using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using cryptoCurrency.core.Enums;

namespace cryptoCurrency.services.Services.BitCoinTradeService
{
    public interface IBitCoinTradeService
    {
        void SetKey(string key);

        void SetCryptoCurrencyTypeEnum(EnumCryptoCurrency.EnumCryptoCurrencyType enumType);

        Task<IDictionary<string, decimal>> GetBalanceAsync();

        Task<IDictionary<string, Object>> GetLastOrderAsync();
    }
}
