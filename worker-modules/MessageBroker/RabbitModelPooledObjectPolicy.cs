using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using Worker.Modules.Models;

namespace Worker.Modules.MessageBroker
{
    public class RabbitModelPooledObjectPolicy : IPooledObjectPolicy<IModel>
    {
        private readonly AppSettings _appSettings;
        private readonly IConnection _connection;

        public RabbitModelPooledObjectPolicy(IOptions<AppSettings> workerSettings)
        {
            _appSettings = workerSettings.Value;
            _connection = GetConnection($"[{Environment.MachineName}] {_appSettings.RabbitMQ.ConsumerTag}");
        }

        private IConnection GetConnection(string workerName)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _appSettings.RabbitMQ.Server,
                UserName = _appSettings.RabbitMQ.UserId,
                Password = _appSettings.RabbitMQ.UserPw,
                Port = _appSettings.RabbitMQ.Port,
                VirtualHost = "/",
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(60)
            };

            return factory.CreateConnection(workerName ?? "DotNet-Workers");
        }

        public IModel Create()
        {
            return _connection.CreateModel();
        }

        public bool Return(IModel obj)
        {
            if (obj.IsOpen)
            {
                return true;
            }
            else
            {
                obj?.Dispose();
                return false;
            }
        }
    }
}
