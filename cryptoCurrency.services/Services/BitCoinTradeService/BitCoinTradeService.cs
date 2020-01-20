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
using System.Text;
using System.Net.Http.Headers;

namespace cryptoCurrency.services.Services.BitCoinTradeService
{
    public class BitCoinTradeService : IBitCoinTradeService
    {
        #region variables
        public string _Key{ get; set; }
        private readonly ILogger<BitCoinTradeService> _logger;
        private EnumCryptoCurrency.EnumCryptoCurrencyType _enumCryptoCurrencyType;
        private decimal _OrderValue;
        private decimal _percentBuyOrderLimit;
        private decimal _percentSellOrderLimit;
        #endregion

        #region methods

        public BitCoinTradeService(ILogger<BitCoinTradeService> logger)
        {
            this._logger = logger;
        }

        public decimal GetBalanceOfCryptoCurrency()
        {
            return GetBalance()[_enumCryptoCurrencyType.ToString()];
        }
        public  IDictionary<string, decimal> GetBalance()
        {
            GetBalanceAsyncReturn objResult = null;

            try
            {
                _logger.LogInformation("Get balance - {time}", DateTimeOffset.Now);

                var URL = "https://api.bitcointrade.com.br/v2/wallets/balance";
                HttpClient request = new HttpClient();
                request.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("ApiToken",_Key);

                var Task = request.GetStringAsync(URL);
                Task.Wait();
                string strResult = Task.Result;

                objResult = JsonConvert.DeserializeObject<GetBalanceAsyncReturn>(strResult);
            }
            catch(HttpRequestException httpex)
            {
                throw new CoreException(httpex.Message);
            }

            return getBalanceConvertTodict(objResult);
        }

        private IDictionary<string, decimal> getBalanceConvertTodict(GetBalanceAsyncReturn obj)
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

        public IDictionary<string, Object> GetLastOrder()
        {
            GetLastOrderAsyncReturn objResult = null;

            try
            {
                _logger.LogInformation("Get last order - {time}", DateTimeOffset.Now);

                var URL = "https://api.bitcointrade.com.br/v2/market/user_orders/list?pair=" + _enumCryptoCurrencyType.ToString();
                HttpClient request = new HttpClient();
                request.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("ApiToken", _Key);

                var Task = request.GetStringAsync(URL);
                Task.Wait();
                string strResult = Task.Result;

                objResult = JsonConvert.DeserializeObject<GetLastOrderAsyncReturn>(strResult);
            }
            catch (HttpRequestException httpex)
            {
                throw new CoreException(httpex.Message);
            }

            return GetLastOrderConvertTodict(objResult);
        }

        private IDictionary<string, Object> GetLastOrderConvertTodict(GetLastOrderAsyncReturn obj)
        {
            var dict = new Dictionary<string, Object>();

            if (obj.Data.Orders.Length == 0)
                return dict;

            dict.Add("status", obj.Data.Orders[0].Status);
            dict.Add("id", obj.Data.Orders[0].Id);
            dict.Add("type", obj.Data.Orders[0].Type);
            dict.Add("UnitPrice", obj.Data.Orders[0].UnitPrice);
            dict.Add("TotalPrice", obj.Data.Orders[0].TotalPrice);
            dict.Add("RequestedAmount", obj.Data.Orders[0].RequestedAmount);
            dict.Add("CreateDate", obj.Data.Orders[0].CreateDate);
            dict.Add("UpdateDate", obj.Data.Orders[0].UpdateDate);

            return dict;
        }

        public IDictionary<string,Object> ExecuteBuyOrder(decimal unitPrice, decimal amount, decimal requestPrice)
        {
            IDictionary<string, Object> objReturn = new Dictionary<string, object>();

            try
            {
                _logger.LogInformation("Execute Buy Order - {time}", DateTimeOffset.Now);

                var objResult =  ExecutGeneriOrder("buy", (float)unitPrice, (float)amount, (float)requestPrice);
                objReturn.Add("id", objResult.Data.Id);
                objReturn.Add("code", objResult.Data.Code);
                objReturn.Add("type", objResult.Data.Type);
                objReturn.Add("unit_price", objResult.Data.UnitPrice);
                objReturn.Add("pair", objResult.Data.Pair);
                objReturn.Add("amount", objResult.Data.Amount);
            }
            catch (HttpRequestException httpex)
            {
                throw new CoreException(httpex.Message);
            }
            catch (Exception ex)
            {
                throw new CoreException(ex.Message);
            }

            return objReturn;
        }
        
        
        public IDictionary<string,Object> ExecuteSellOrder(decimal unitPrice, decimal amount, decimal requestPrice)
        {
            IDictionary<string, Object> objReturn = new Dictionary<string, object>();

            try
            {
                _logger.LogInformation("Execute sell Order - {time}", DateTimeOffset.Now);

                var objResult =  ExecutGeneriOrder("sell", (float)unitPrice, (float)amount, (float)requestPrice);
                objReturn.Add("id", objResult.Data.Id);
                objReturn.Add("code", objResult.Data.Code);
                objReturn.Add("type", objResult.Data.Type);
                objReturn.Add("unit_price", objResult.Data.UnitPrice);
                objReturn.Add("pair", objResult.Data.Pair);
                objReturn.Add("amount", objResult.Data.Amount);
            }
            catch (HttpRequestException httpex)
            {
                throw new CoreException(httpex.Message);
            }
            catch (Exception ex)
            {
                throw new CoreException(ex.Message);
            }

            return objReturn;
        }

        private ExecuteGenericOrderReturn ExecutGeneriOrder(string OrderType, float unitPrice, float amount, float requestPrice)
        {
            var URL = "https://api.bitcointrade.com.br/v2/market/create_order";
            HttpClient request = new HttpClient();
            request.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("ApiToken", _Key);
            request.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue("application/json"));

            var body = new
            {
                pair = _enumCryptoCurrencyType.ToString(),
                amount = amount,
                type = OrderType,
                subtype = "limited",
                unit_price = unitPrice,
                request_price = requestPrice
            };
            var jsonContent = JsonConvert.SerializeObject(body);
            var content = new StringContent(jsonContent);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var Task = request.PostAsync(URL, content);
            Task.Wait();
            var result = Task.Result;

            if(result.StatusCode != HttpStatusCode.OK)
                throw new Exception("Execute Generic Order error");

            var TaskRead = result.Content.ReadAsStringAsync();
            TaskRead.Wait();
            var strResponse = TaskRead.Result;

            return JsonConvert.DeserializeObject<ExecuteGenericOrderReturn>(strResponse);
        }

        public void CancelOrder(string orderId)
        {
            var body = new
            {
                id = orderId
            };

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("ApiToken", _Key);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            var jsonContent = JsonConvert.SerializeObject(body);
            var content = new StringContent(jsonContent);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpRequestMessage request = new HttpRequestMessage
            {
                Content = content,
                Method = HttpMethod.Delete,
                RequestUri = new Uri("https://api.bitcointrade.com.br/v2/market/user_orders/")
            };
             
            var Task = httpClient.SendAsync(request);
            Task.Wait();
            var result = Task.Result;

            _logger.LogInformation("Cancel Order - {time}", DateTimeOffset.Now);
            //var URL = "https://api.bitcointrade.com.br/v2/market/user_orders/";
            //HttpClient request = new HttpClient();
            //request.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("ApiToken", _Key);
            //request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //var body = new
            //{
            //    id  = orderId
            //};

            //var jsonContent = JsonConvert.SerializeObject(body);
            //var content = new StringContent(jsonContent);
            //content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            //var Task = request.DeleteAsync(URL,;
            //Task.Wait();
            //var result = Task.Result;

            if (result.StatusCode != HttpStatusCode.OK)
                throw new Exception("Cancel Order error");
        }
        public bool verifyIfBotHasMoney()
        {
            //verify if the bot has money to buy
            var amountRemain = GetBalance();
            var Remain = (decimal)amountRemain["BRL"];

            if (Remain <= _OrderValue)
            {
                _logger.LogInformation("Bot has no money to buy");
                return false;
            }
            return true;
        }

        public void SetOrderValue(decimal value)
        {
            if (value < (decimal)25)
                throw new CoreException("Value must be greater than R$25.00");
            _OrderValue = value;
        }
        public  IEnumerable<IDictionary<string, Object>> GetBookBuyOrders()
        {
            _logger.LogInformation("Get Book Buy Orders- {time}", DateTimeOffset.Now);
            List<Dictionary<string, Object>> objReturn = new List<Dictionary<string, Object>>();
            try
            {
                var objBookOrders = GetBookOrders();

                foreach (var item in objBookOrders.Data.Buying)
                {
                    var dict = new Dictionary<string, Object>();
                    dict.Add("id", (string)item.Id);
                    dict.Add("amount", (decimal)item.Amount);
                    dict.Add("UnitPrice", (decimal)item.UnitPrice);
                    objReturn.Add(dict);
                }
            }
            catch (HttpRequestException httpex)
            {
                throw new CoreException(httpex.Message);
            }
            catch (Exception ex)
            {
                throw new CoreException(ex.Message);
            }
            if (objReturn.Count == 0)
            {
                throw new CoreException("GetBookOrders does not get values");
            }
            return objReturn;        
        }

        public IEnumerable<IDictionary<string, Object>> GetBookSellOrders()
        {
            _logger.LogInformation("Get Book sell Orders- {time}", DateTimeOffset.Now);
            List<Dictionary<string, Object>> objReturn = new List<Dictionary<string, Object>>();
            try
            {
                var objBookOrders = GetBookOrders();

                foreach (var item in objBookOrders.Data.Selling)
                {
                    var dict = new Dictionary<string, Object>();
                    dict.Add("id", (string)item.Id);
                    dict.Add("amount", (decimal)item.Amount);
                    dict.Add("UnitPrice", (decimal)item.UnitPrice);
                    objReturn.Add(dict);
                }
            }
            catch (HttpRequestException httpex)
            {
                throw new CoreException(httpex.Message);
            }
            catch (Exception ex)
            {
                throw new CoreException(ex.Message);
            }
            return objReturn;
        }

        private GetBookOrdersReturn GetBookOrders()
        {
            GetBookOrdersReturn objReturn = new GetBookOrdersReturn();

            var URL = "https://api.bitcointrade.com.br/v2/market?pair=" + _enumCryptoCurrencyType.ToString();
            HttpClient request = new HttpClient();
            request.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("ApiToken", _Key);

            var Task = request.GetStringAsync(URL);
            Task.Wait();
            var strResult = Task.Result;

            return JsonConvert.DeserializeObject<GetBookOrdersReturn>(strResult);
          
        }

        public bool canIncreaseOrderBuyPrice(decimal firstPrice, decimal currentPrice)
        {
            return currentPrice >= ((decimal)1 + _percentBuyOrderLimit/(decimal)100) * firstPrice ? false : true;
        
        }  
        public bool canDecreaseOrderSellPrice(decimal firstPrice, decimal currentPrice)
        {
            return currentPrice >= ((decimal)1 + _percentSellOrderLimit / (decimal)100) * firstPrice ? false : true;
        }

        public void SetPercentBuyOrderLimit(decimal percent)
        {
            this._percentBuyOrderLimit = percent;
        } 
        
        public void SetPercentSellOrderLimit(decimal percent)
        {
            this._percentSellOrderLimit = percent;
        }

        public decimal GetOrderValue() { return _OrderValue; }

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
            public GetLastOrderAsyncReturnData Data { get; set; }
        }

        public class GetLastOrderAsyncReturnData
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

        #region GetBookOrdersReturn
        public partial class GetBookOrdersReturn
        {
            [JsonProperty("code")]
            public object Code { get; set; }

            [JsonProperty("message")]
            public object Message { get; set; }

            [JsonProperty("data")]
            public Data Data { get; set; }
        }

        public partial class Data
        {
            [JsonProperty("buying")]
            public List<Ing> Buying { get; set; }

            [JsonProperty("executed")]
            public List<Executed> Executed { get; set; }

            [JsonProperty("selling")]
            public List<Ing> Selling { get; set; }
        }

        public partial class Ing
        {
            [JsonProperty("amount")]
            public double Amount { get; set; }

            [JsonProperty("code")]
            public string Code { get; set; }

            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("stop_limit_price")]
            public object StopLimitPrice { get; set; }

            [JsonProperty("unit_price")]
            public double UnitPrice { get; set; }

            [JsonProperty("user_code")]
            public string UserCode { get; set; }
        }

        public partial class Executed
        {
            [JsonProperty("active_order_code")]
            public string ActiveOrderCode { get; set; }

            [JsonProperty("active_order_user_code")]
            public string ActiveOrderUserCode { get; set; }

            [JsonProperty("amount")]
            public double Amount { get; set; }

            [JsonProperty("create_date")]
            public DateTimeOffset CreateDate { get; set; }
                      
            [JsonProperty("passive_order_code")]
            public string PassiveOrderCode { get; set; }

            [JsonProperty("passive_order_user_code")]
            public string PassiveOrderUserCode { get; set; }

           
            [JsonProperty("unit_price")]
            public double UnitPrice { get; set; }
        }
        #endregion

        #region ExecuteOrderReturn
        public partial class ExecuteGenericOrderReturn
        {
            [JsonProperty("message")]
            public object Message { get; set; }

            [JsonProperty("data")]
            public ExecuteOrderReturnData Data { get; set; }
        }

        public partial class ExecuteOrderReturnData
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("code")]
            public string Code { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("unit_price")]
            public long UnitPrice { get; set; }

            [JsonProperty("user_code")]
            public string UserCode { get; set; }

            [JsonProperty("pair")]
            public string Pair { get; set; }

            [JsonProperty("amount")]
            public double Amount { get; set; }
        }

        #endregion
        #endregion
    }
}
