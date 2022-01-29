using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Worker.Modules;

namespace Worker_5A
{
    public class WorkerA : IWorker
    {
        private readonly ILogger<WorkerA> _logger;
        public WorkerA(ILogger<WorkerA> logger)
        {
            _logger = logger;
        }
        public async Task RunAsync(string message)
        {
            await Task.Run(() => _logger.LogInformation(message + " Worker NET5 A"));
        }
    }
}
