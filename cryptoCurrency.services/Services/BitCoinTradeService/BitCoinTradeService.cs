using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using cryptoCurrency.core.Exceptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using cryptoCurrency.core.Enums;

namespace cryptoCurrency.services.Services.BitCoinTradeService
{
    public class BitCoinTradeService : IBitCoinTradeService
    {
        #region variables
        public string _Key{ get; set; }
        private readonly ILogger<BitCoinTradeService> _logger;
        private EnumCryptoCurrency.EnumCryptoCurrencyType _enumCryptoCurrencyType;

        #endregion

        #region methods

        public BitCoinTradeService(ILogger<BitCoinTradeService> logger)
        {
            this._logger = logger;
        }

        public async Task<IDictionary<string, decimal>> GetBalanceAsync()
        {
            GetBalanceAsyncReturn objResult = null;

            try
            {
                _logger.LogInformation("Get balance - {time}", DateTimeOffset.Now);

                var URL = "https://api.bitcointrade.com.br/v2/wallets/balance";
                HttpClient request = new HttpClient();
                request.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("ApiToken",_Key);

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

            var strCurrencycode = string.Empty;
            foreach (var item in obj.Data)
            {
                if (string.Compare(item.CurrencyCode, "BTC") == 0)
                    strCurrencycode = EnumCryptoCurrency.EnumCryptoCurrencyType.BRLBTC.ToString();
                else if (string.Compare(item.CurrencyCode, "ETH") == 0)
                    strCurrencycode = EnumCryptoCurrency.EnumCryptoCurrencyType.BRLETH.ToString();
                else if (string.Compare(item.CurrencyCode, "LTC") == 0)
                    strCurrencycode = EnumCryptoCurrency.EnumCryptoCurrencyType.BRLLTC.ToString();
                else if (string.Compare(item.CurrencyCode, "BCH") == 0)
                    strCurrencycode = EnumCryptoCurrency.EnumCryptoCurrencyType.BRLBCH.ToString();
                else if (string.Compare(item.CurrencyCode, "XRP") == 0)
                    strCurrencycode = EnumCryptoCurrency.EnumCryptoCurrencyType.BRLXRP.ToString();
                else 
                    strCurrencycode = item.CurrencyCode;
                
                dict.Add(strCurrencycode, item.AvailableAmount);
            }
                

            return dict;
        }

        public void SetCryptoCurrencyTypeEnum(EnumCryptoCurrency.EnumCryptoCurrencyType enumType)
        {
            _enumCryptoCurrencyType = enumType;
        }

        public void SetKey(string Key)
        {
            if (string.IsNullOrEmpty(Key))
                throw new CoreException("Trade key empty or null");

            _logger.LogInformation("Trade key - {time}", DateTimeOffset.Now);
            this._Key = Key;
        }

        public async Task<IDictionary<string, Object>> GetLastOrderAsync()
        {
            GetLastOrderAsyncReturn objResult = null;

            try
            {
                _logger.LogInformation("Get last order - {time}", DateTimeOffset.Now);

                var URL = "https://api.bitcointrade.com.br/v2/market/user_orders/list?pair=" + _enumCryptoCurrencyType.ToString();
                HttpClient request = new HttpClient();
                request.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("ApiToken", _Key);

                string strResult = await request.GetStringAsync(URL);

                objResult = JsonConvert.DeserializeObject<GetLastOrderAsyncReturn>(strResult);
            }
            catch (HttpRequestException httpex)
            {
                throw new CoreException(httpex.Message);
            }

            return GetLastOrderAsyncConvertTodict(objResult);
        }

        private IDictionary<string, Object> GetLastOrderAsyncConvertTodict(GetLastOrderAsyncReturn obj)
        {
            var dict = new Dictionary<string, Object>();

            if (obj.Data.Orders.Length == 0)
                return dict;

            dict.Add("status", obj.Data.Orders[0].Status);
            dict.Add("type", obj.Data.Orders[0].Type);
            dict.Add("UnitPrice", obj.Data.Orders[0].UnitPrice);
            dict.Add("TotalPrice", obj.Data.Orders[0].TotalPrice);
            dict.Add("RequestedAmount", obj.Data.Orders[0].RequestedAmount);
            dict.Add("CreateDate", obj.Data.Orders[0].CreateDate);
            dict.Add("UpdateDate", obj.Data.Orders[0].UpdateDate);

            return dict;
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

        #region getLastOrderAsync
        public class GetLastOrderAsyncReturn
        {
            [JsonProperty("code")]
            public object Code { get; set; }

            [JsonProperty("message")]
            public object Message { get; set; }

            [JsonProperty("data")]
            public Data Data { get; set; }
        }

        public class Data
        {
            [JsonProperty("pagination")]
            public Pagination Pagination { get; set; }

            [JsonProperty("orders")]
            public Order[] Orders { get; set; }
        }

        public class Order
        {
            [JsonProperty("code")]
            public string Code { get; set; }

            [JsonProperty("create_date")]
            public DateTimeOffset CreateDate { get; set; }

            [JsonProperty("executed_amount")]
            public decimal ExecutedAmount { get; set; }

            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("pair_code")]
            public string PairCode { get; set; }

            [JsonProperty("remaining_amount")]
            public decimal RemainingAmount { get; set; }

            [JsonProperty("remaining_price")]
            public decimal RemainingPrice { get; set; }

            [JsonProperty("requested_amount")]
            public decimal RequestedAmount { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("subtype")]
            public string Subtype { get; set; }

            [JsonProperty("total_price")]
            public long TotalPrice { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("unit_price")]
            public decimal UnitPrice { get; set; }

            [JsonProperty("update_date")]
            public DateTimeOffset UpdateDate { get; set; }
        }

        public class Pagination
        {
            [JsonProperty("current_page")]
            public long CurrentPage { get; set; }

            [JsonProperty("page_size")]
            public long PageSize { get; set; }

            [JsonProperty("registers_count")]
            public long RegistersCount { get; set; }

            [JsonProperty("total_pages")]
            public long TotalPages { get; set; }
        }
        #endregion

        #endregion
    }
}
