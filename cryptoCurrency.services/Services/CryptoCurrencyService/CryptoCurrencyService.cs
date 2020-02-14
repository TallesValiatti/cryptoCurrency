using System;
using System.Collections.Generic;
using cryptoCurrency.core.Enums;
using cryptoCurrency.core.Exceptions;
using Microsoft.Extensions.Logging;
using System.Web;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json.Linq;

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

        public  IEnumerable<Object> GetLast24HPricePerMin()
        {
            try
            {
                _logger.LogInformation("Get GetLast24HPrice - {time}", DateTimeOffset.Now);
                var prices = GetPrices(1);
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

        public IEnumerable<decimal> GetLast11HPricePerHour()
        {
            try
            {
                _logger.LogInformation("Get GetLast11HPricePer - {time}", DateTimeOffset.Now);
                var prices = GetPrices(1);

                var pricesNormalized = splitValues(24, prices.ToList());

                return pricesNormalized.Skip(13);
            }
            catch (HttpRequestException ex)
            {
                throw new CoreException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new CoreException(ex.Message);
            }
        }

        private IEnumerable<decimal> splitValues(int numberOfSplits, IList<Object> serie)
        {
            var parts = new List<decimal>();

            var n = (int)serie.Count() / numberOfSplits;

            for (int i = 0; i < n; i = i + n)
            {
                decimal value = 0;

                if (serie.Count() > i + numberOfSplits)
                {
                    value = Enumerable.Average(serie.Skip(i).Take(n).Select(p => ((decimal)((JArray)p)[0])));
                }
                else
                {
                    value = Enumerable.Average(serie.Skip(i + n).Select(p => ((decimal)((JArray)p)[0])));
                }
                
                parts.Add(value);
            }
            return parts;
        }

        private IEnumerable<Object> GetPrices(int days)
        {
            if(days > 89)
            {
                throw new Exception("Number of days must be lower than 90 days.");
            }

            GetLast24HPriceReturn objResult;
            var URL = "https://api.coingecko.com/api/v3/coins/"+__cryptoCurrentyTypeStr+"/market_chart?vs_currency=brl&days=" + days.ToString();
            HttpClient request = new HttpClient();
            
            var Task = request.GetStringAsync(URL);
            Task.Wait();
            string strResult = Task.Result;

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
            public IEnumerable<Object> Prices { get; set; }
        }

        #endregion
    }
}
