namespace cryptoCurrency.services.Services.GenericServices
{
    public interface IGenericService
    {
        object getObjectFromDynamic(string props, dynamic obj);
    }
}