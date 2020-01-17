using System;
using System.Threading.Tasks;
using cryptoCurrency.core.Enums;
using static cryptoCurrency.core.Enums.EnumBotState;

namespace cryptoCurrency.services.Services.DecisonMakerService
{
    public interface IDecisionMakerService
    {
        void SetCryptoCurrencyTypeEnum(EnumCryptoCurrency.EnumCryptoCurrencyType enumType);

        EnumBotStateType  DecideWhichStateToGo();

        bool DecideIfShouldSell();

        Task<bool> DecideIfShouldBuy();
    }
}
