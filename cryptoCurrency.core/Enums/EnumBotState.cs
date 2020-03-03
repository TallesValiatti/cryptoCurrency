using System;
namespace cryptoCurrency.core.Enums
{
    public class EnumBotState
    {
        public enum EnumBotStateType
        {
            awaitToBuy = 0,
            tryToBuy = 1,
            awaitToSell = 2,
            tryToSell = 3,
            NoState = 4
        }
    }
}

