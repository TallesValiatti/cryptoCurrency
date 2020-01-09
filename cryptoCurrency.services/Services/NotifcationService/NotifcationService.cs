using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using cryptoCurrency.core.Exceptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace cryptoCurrency.services.Services.NotifcationService
{
    public class NotifcationService : INotificationService
    {
        #region Props
        private string _key;
        private readonly ILogger<NotifcationService> _logger;
        #endregion

        #region Methods


        public NotifcationService(ILogger<NotifcationService> logger)
        {
            this._logger = logger;
        }

        public void ErrorNotification(string message)
        {

            if (string.IsNullOrEmpty(_key))
                return;

            _logger.LogInformation(" Send Error notification - {time}", DateTimeOffset.Now);

            //ToSlack
            ErrorNotificationOnSlack(message);
        }

        private void ErrorNotificationOnSlack(string message)
        {
            try
            {
                var urlWithAccessToken = "https://hooks.slack.com/services/" + _key;

                var client = new SlackClient(urlWithAccessToken);

                var msg = DateTime.Now.ToString("dd/MM/yyyy HH:mm") + " - ERROR\n" + message;

                client.PostMessage(text: msg);
            }
            catch(Exception ex)
            {
                //pass
                var a = 1;
            }
        }

        public void SetKey(string Key)
        {
            if(string.IsNullOrEmpty(Key))
                throw new CoreException("Notification key empty or null");

            _logger.LogInformation("Notification key - {time}", DateTimeOffset.Now);
            this._key = Key;
        }
        #endregion

        #region Slack Private classes
        private class SlackClient
        {
            private readonly Uri _uri;
            private readonly Encoding _encoding = new UTF8Encoding();

            public SlackClient(string urlWithAccessToken)
            {
                _uri = new Uri(urlWithAccessToken);
            }

            //Post a message using simple strings
            public void PostMessage(string text, string username = null, string channel = null)
            {
                Payload payload = new Payload()
                {
                    Channel = channel,
                    Username = username,
                    Text = text
                };

                PostMessage(payload);
            }

            //Post a message using a Payload object
            public void PostMessage(Payload payload)
            {
                string payloadJson = JsonConvert.SerializeObject(payload);

                using (WebClient client = new WebClient())
                {
                    NameValueCollection data = new NameValueCollection();
                    data["payload"] = payloadJson;

                    var response = client.UploadValues(_uri, "POST", data);

                    //The response text is usually "ok"
                    string responseText = _encoding.GetString(response);
                }
            }
        }

        private class Payload
        {
            [JsonProperty("channel")]
            public string Channel { get; set; }

            [JsonProperty("username")]
            public string Username { get; set; }

            [JsonProperty("text")]
            public string Text { get; set; }
        }
        #endregion

    }
}
