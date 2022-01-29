namespace Worker.Modules.Models
{
    public class AppSettings
    {
        public RabbitMQSetup RabbitMQ { get; set; }
        public bool RunOnce { get; set; }
    }

    public class RabbitMQSetup
    {
        public string Server { get; set; }
        public string UserId { get; set; }
        public string UserPw { get; set; }
        public int Port { get; set; }
        public string VirtualHost { get; set; }

        public string QueueName { get; set; }
        public string ConsumerTag { get; set; }
    }
}
