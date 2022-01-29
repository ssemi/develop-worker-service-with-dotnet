using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Worker.Modules;

namespace Worker_Converter5
{
    public class WorkerConverter : IWorker
    {
        private readonly ILogger<WorkerConverter> _logger;
        public WorkerConverter(ILogger<WorkerConverter> logger)
        {
            _logger = logger;
        }
        public async Task RunAsync(string message)
        {
            await Task.Run(() => _logger.LogInformation(message + " Worker NET5 Converter"));
        }
    }
}
