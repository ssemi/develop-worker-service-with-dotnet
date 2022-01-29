using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;
using Worker.Modules;
using Worker.Modules.MessageBroker;
using Worker.Modules.Models;

namespace Worker
{
    public class Runner
    {
        private readonly ILogger<Runner> _logger;
        private readonly AppSettings _config;
        private readonly IRabbitManager _queueAdapter;
        private readonly IWorker _Worker;

        public Runner(ILogger<Runner> logger, IOptions<AppSettings> config, IRabbitManager queueAdapter, IWorker Worker)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config?.Value ?? throw new ArgumentNullException(nameof(AppSettings));
            _queueAdapter = queueAdapter;
            _Worker = Worker;
        }

        public void DoAction(string optionQueueName)
        {
            var queue_name = _config.RabbitMQ?.QueueName ?? string.Empty;
            var consumer_tag = _config.RabbitMQ?.ConsumerTag?? string.Empty;

            if (string.IsNullOrEmpty(optionQueueName) == false) queue_name = optionQueueName;
            if (string.IsNullOrEmpty(queue_name))
                throw new ArgumentNullException(nameof(queue_name));

            var consumer = $"[{Environment.MachineName}] {consumer_tag}";

            Console.WriteLine($"[Queue setting result] consumer :  {consumer}  queue_name : {queue_name}");

            if (_config.RunOnce)
                _queueAdapter.BasicGet(queue_name, body => DoConsumeProcess(body));
            else
                _queueAdapter.Consume(queue_name, consumer_tag, body => DoConsumeProcess(body));
        }

        private async Task DoConsumeProcess(byte[] body)
        {
            var message = Encoding.UTF8.GetString(body);

            if (string.IsNullOrEmpty(message))
                return;

            Console.WriteLine($"[{DateTime.UtcNow.AddHours(9).ToString("yyyy-MM-dd HH:mm:ss")}] - {message}");

            if (IsValidJson(message))
                await _Worker.RunAsync(message);
        }

        private static bool IsValidJson(string strInput)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    Console.WriteLine(jex.Message);
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

    }
}
