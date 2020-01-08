using System;
using Microsoft.Extensions.Logging;

namespace cryptoCurrency.services.Services.GenericServices
{
    public class GenericService : IGenericService
    {
        #region variables
        private readonly ILogger<GenericService> _logger;
        #endregion
        #region methods

        public GenericService(ILogger<GenericService> logger)
        {
            this._logger = logger;
        }
        public object getObjectFromDynamic(string props, dynamic obj)
        {
            _logger.LogInformation("Get object from props '"+props+"' - {time}", DateTimeOffset.Now);
            return obj.GetType().GetProperty(props).GetValue(obj, null);
        }
        #endregion
    }
}
