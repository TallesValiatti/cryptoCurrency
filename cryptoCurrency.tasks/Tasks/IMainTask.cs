using System;
using System.Threading.Tasks;

namespace cryptoCurrency.tasks.Tasks
{
    public interface IMainTask
    {
        Task ExecuteAsync(dynamic objData);
    }
}
