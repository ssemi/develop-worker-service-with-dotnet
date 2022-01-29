using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Worker.Modules;

namespace Worker_B
{
    public class WorkerB : IWorker
    {
        private readonly ILogger<WorkerB> _logger;
        public WorkerB(ILogger<WorkerB> logger)
        {
            _logger = logger;
        }
        public async Task RunAsync(string message)
        {
            await Task.Run(() => _logger.LogInformation(message + " Worker B"));
        }
    }
}
