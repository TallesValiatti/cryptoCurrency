using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using cryptoCurrency.core.Enums;


namespace cryptoCurrency.services.Services.CryptoCurrencyService
{
    public interface ICryptoCurrencyService
    {
        void SetCryptoCurrencyType(EnumCryptoCurrency.EnumCryptoCurrencyType enumType);

        Task<IEnumerable<IEnumerable<long>>> GetLast48HPrice();
    }
}
