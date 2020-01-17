using System;
using cryptoCurrency.services.Services.DecisonMakerService;
using Microsoft.Extensions.Logging;

namespace cryptoCurrency.tasks.Tasks.TaskAwaitToBuy
{
    public class TaskAwaitToBuy:ITaskAwaitToBuy
    {
        #region variables
        private readonly ILogger<TaskAwaitToBuy> _logger;
        private readonly IDecisionMakerService _decisionMakerService;
        #endregion

        #region methods
        public TaskAwaitToBuy(ILogger<TaskAwaitToBuy> logger, IDecisionMakerService decisionMakerService)
        {
            this._logger = logger;
            this._decisionMakerService = decisionMakerService;
        }

        public void Execute()
        {
            _logger.LogInformation("Execute task await to buy - {time}", DateTimeOffset.Now);

            var TaskShouldBuy = _decisionMakerService.DecideIfShouldBuy();

            TaskShouldBuy.Wait();

            var ShouldBuy = TaskShouldBuy.Result;

            if (ShouldBuy)
            {
                _logger.LogInformation("Decide to buy - {time}", DateTimeOffset.Now);
            }
            else
            {
                _logger.LogInformation("Decide to NOT buy - {time}", DateTimeOffset.Now);
            }
        }

        #endregion
    }
}
