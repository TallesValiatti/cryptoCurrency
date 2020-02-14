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

        IEnumerable<Object> GetLast24HPricePerMin();

        IEnumerable<decimal> GetLast11HPricePerHour();
    }
}
