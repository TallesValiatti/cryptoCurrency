using System;
using System.Threading.Tasks;

namespace cryptoCurrency.tasks.Tasks
{
    public interface IMainTask
    {
        void Execute(dynamic objData);
    }
}
