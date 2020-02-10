using cryptoCurrency.services.Services.DecisonMakerService;
using cryptoCurrency.tasks.Tasks.TaskSell;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace cryptoCurrency.tasks.Tasks.TaskAwaitToSell
{
    public class TaskAwaitToSell : ITaskAwaitToSell
    {
        #region variables
        private readonly IDecisionMakerService _decisionMakerService;
        private readonly ILogger<TaskAwaitToSell> _logger;
        private readonly ITaskSell _taskSell;
        #endregion

        #region methods

        public TaskAwaitToSell(IDecisionMakerService decisionMakerService, ILogger<TaskAwaitToSell> logger, ITaskSell taskSell)
        {
            this._decisionMakerService = decisionMakerService;
            this._logger = logger;
            this._taskSell = taskSell;
        }
        public void Execute()
        {
            _logger.LogInformation("Execute task await to sell - {time}", DateTimeOffset.Now);

            var ShoulSell = _decisionMakerService.DecideIfShouldSell();

            if (ShoulSell)
            {
                _logger.LogInformation("Bot Decide to Sell - {time}", DateTimeOffset.Now);
                _taskSell.Execute();
            }
            else
            {
                _logger.LogInformation("Bot Decide to NOT sell - {time}", DateTimeOffset.Now);
            }
        }
        #endregion

    }
}
