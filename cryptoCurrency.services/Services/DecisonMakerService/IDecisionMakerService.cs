﻿using System;
using System.Threading.Tasks;
using cryptoCurrency.core.Enums;
using static cryptoCurrency.core.Enums.EnumBotState;

namespace cryptoCurrency.services.Services.DecisonMakerService
{
    public interface IDecisionMakerService
    {
        void SetCryptoCurrencyTypeEnum(EnumCryptoCurrency.EnumCryptoCurrencyType enumType);

        void SetPercentToSell(decimal low, decimal high);

        void SetURLdecisonMaker (string URL);

        EnumBotStateType  DecideWhichStateToGo();

        bool predictDecisionMaker(string token);

        bool DecideIfShouldSell();

        bool DecideIfShouldBuy();
    }
}
