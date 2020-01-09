using System;
namespace cryptoCurrency.services.Services.NotifcationService
{
    public interface INotificationService
    {
        void SetKey(string Key);

        void ErrorNotification(string message);

        
    }
}
