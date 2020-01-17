using System;
using System.Collections.Generic;
using cryptoCurrency.core.Enums;
using cryptoCurrency.core.Exceptions;
using Microsoft.Extensions.Logging;
using System.Web;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace cryptoCurrency.services.Services.CryptoCurrencyService
{
    public class CryptoCurrencyService : ICryptoCurrencyService
    {
        #region Variables
        private EnumCryptoCurrency.EnumCryptoCurrencyType _cryptoCurrentyType;
        private readonly ILogger<CryptoCurrencyService> _logger;
        private string __cryptoCurrentyTypeStr;
        #endregion

        #region methods

        public CryptoCurrencyService(ILogger<CryptoCurrencyService> logger)
        {
            this._logger = logger;
        }

        public async Task<IEnumerable<IEnumerable<long>>> GetLast48HPrice()
        {
            try
            {
                _logger.LogInformation("Get GetLast24HPrice - {time}", DateTimeOffset.Now);
                var prices = await GetPrices(2);
                return prices;
            }
            catch(HttpRequestException ex)
            {
                throw new CoreException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new CoreException(ex.Message);
            }
        }
        
        private async Task<IEnumerable<IEnumerable<long>>> GetPrices(int days)
        {
            if(days > 89)
            {
                throw new Exception("Number of days must be lower than 90 days.");
            }

            GetLast24HPriceReturn objResult;
            var URL = "https://api.coingecko.com/api/v3/coins/"+__cryptoCurrentyTypeStr+"/market_chart?vs_currency=brl&days=" + days.ToString();
            HttpClient request = new HttpClient();
            
            string strResult = await request.GetStringAsync(URL);

            objResult = JsonConvert.DeserializeObject<GetLast24HPriceReturn>(strResult);
            return objResult.Prices;
        }

        public void SetCryptoCurrencyType(EnumCryptoCurrency.EnumCryptoCurrencyType enumType)
        {
            _logger.LogInformation("Setting the _cryptoCurrentyType - {time}", DateTimeOffset.Now);

            _cryptoCurrentyType = enumType;

            if (enumType == EnumCryptoCurrency.EnumCryptoCurrencyType.BRLBCH)
                __cryptoCurrentyTypeStr = "bitcoin-cash";
            else if (enumType == EnumCryptoCurrency.EnumCryptoCurrencyType.BRLBTC)
                __cryptoCurrentyTypeStr = "bitcoin";
            else if (enumType == EnumCryptoCurrency.EnumCryptoCurrencyType.BRLETH)
                __cryptoCurrentyTypeStr = "ethereum";
            else if (enumType == EnumCryptoCurrency.EnumCryptoCurrencyType.BRLLTC)
                __cryptoCurrentyTypeStr = "litecoin";
            else if (enumType == EnumCryptoCurrency.EnumCryptoCurrencyType.BRLXRP)
                __cryptoCurrentyTypeStr = "ripple";
        }
        #endregion

        #region return Classes


        private class GetLast24HPriceReturn
        {
            [JsonProperty("prices")]
            public IEnumerable<IEnumerable<long>> Prices { get; set; }
        }

        #endregion
    }
}
