using System;
namespace cryptoCurrency.core.Enums
{
    public class EnumOrderStatus
    {
         public enum EnumOrderStatusType
        {
            executed_completely,
            executed_partially,
            waiting,
            canceled
        }
    }
}
