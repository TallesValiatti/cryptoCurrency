using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace cryptoCurrency.services.Services.BitCoinTradeService
{
    public interface IBitCoinTradeService
    {
        void setKey(string key);

        Task<IDictionary<string, decimal>> getBalanceAsync();
    }
}
