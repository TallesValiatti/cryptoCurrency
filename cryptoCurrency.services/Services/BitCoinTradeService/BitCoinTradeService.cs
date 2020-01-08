using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using cryptoCurrency.core.Exceptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace cryptoCurrency.services.Services.BitCoinTradeService
{
    public class BitCoinTradeService : IBitCoinTradeService
    {
        #region variables
        public string _Key{ get; set; }
        private readonly ILogger<BitCoinTradeService> _logger;
        
        #endregion

        #region methods

        public BitCoinTradeService(ILogger<BitCoinTradeService> logger)
        {
            this._logger = logger;
        }

        public async Task<IDictionary<string, decimal>> getBalanceAsync()
        {
            GetBalanceAsyncReturn objResult = null;

            try
            {
                _logger.LogInformation("Get balance - {time}", DateTimeOffset.Now);

                var URL = "https://api.bitcointrade.com.br/v2/wallets/balance";
                HttpClient request = new HttpClient();
                request.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("ApiToken1",_Key);

                string strResult = await request.GetStringAsync(URL);

                objResult = JsonConvert.DeserializeObject<GetBalanceAsyncReturn>(strResult);
            }
            catch(HttpRequestException httpex)
            {
                throw new CoreException(httpex.Message);
            }

            return getBalanceAsyncConvertTodict(objResult);
        }

        private IDictionary<string, decimal> getBalanceAsyncConvertTodict(GetBalanceAsyncReturn obj)
        {
            var dict = new Dictionary<string, decimal>();

            foreach (var item in obj.Data)
                dict.Add(item.CurrencyCode, item.AvailableAmount);

            return dict;
        }

        public void setKey(string key)
        {
            _logger.LogInformation("Setting key - {time}", DateTimeOffset.Now);
            this._Key = key;
        }
        #endregion

        #region Returned classes

        #region getBalanceAsync

        private class GetBalanceAsyncReturn
        {
            [JsonProperty("code")]
            public object Code { get; set; }

            [JsonProperty("message")]
            public object Message { get; set; }

            [JsonProperty("data")]
            public Datum[] Data { get; set; }
        }

        private class Datum
        {
            [JsonProperty("available_amount")]
            public decimal AvailableAmount { get; set; }

            [JsonProperty("currency_code")]
            public string CurrencyCode { get; set; }

            [JsonProperty("locked_amount")]
            public long LockedAmount { get; set; }
        }
        #endregion

        #endregion
    }
}
