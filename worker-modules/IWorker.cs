using System.Threading.Tasks;

namespace Worker.Modules
{
    public interface IWorker
    {
        Task RunAsync(string message);
    }
}
