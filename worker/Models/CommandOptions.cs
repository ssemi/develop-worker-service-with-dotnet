using CommandLine;

namespace Worker.Models
{
    public class CommandOptions
    {
        [Option("env", HelpText = "environment")]
        public string Environment { get; set; }

        [Option("workertype", HelpText = "worker type")]
        public string WorkerType { get; set; }

        [Option("queue", HelpText = "queueName")]
        public string QueueName { get; set; }
    }
}
