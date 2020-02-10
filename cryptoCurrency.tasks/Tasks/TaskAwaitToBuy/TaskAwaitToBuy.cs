using System;
using cryptoCurrency.services.Services.DecisonMakerService;
using cryptoCurrency.tasks.Tasks.TaskBuy;
using Microsoft.Extensions.Logging;

namespace cryptoCurrency.tasks.Tasks.TaskAwaitToBuy
{
    public class TaskAwaitToBuy:ITaskAwaitToBuy
    {
        #region variables
        private readonly ILogger<TaskAwaitToBuy> _logger;
        private readonly IDecisionMakerService _decisionMakerService;
        private readonly ITaskBuy _taskBuy;
        #endregion

        #region methods
        public TaskAwaitToBuy(ILogger<TaskAwaitToBuy> logger, IDecisionMakerService decisionMakerService, ITaskBuy taskBuy)
        {
            this._logger = logger;
            this._decisionMakerService = decisionMakerService;
            this._taskBuy = taskBuy;
        }

        public void Execute()
        {
            _logger.LogInformation("Execute task await to buy - {time}", DateTimeOffset.Now);

            var ShouldBuy = _decisionMakerService.DecideIfShouldBuy();

            if (ShouldBuy)
            {
                _logger.LogInformation("Bot Decide to buy - {time}", DateTimeOffset.Now);
                _taskBuy.Execute();
            }
            else
            {
                _logger.LogInformation("Bot Decide to NOT buy - {time}", DateTimeOffset.Now);
            }
        }

        #endregion
    }
}
