namespace Worker.Modules.MessageBroker
{
    using Microsoft.Extensions.ObjectPool;
    using Newtonsoft.Json;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using System;
    using System.Text;
    using System.Threading.Tasks;

    public interface IRabbitManager
    {
        void Publish<T>(T message, string routeKey, string exchangeName = "", string exchangeType = "direct") where T : class;
        void Consume(string queueName, string consumerTag, Func<byte[], Task> action);
        void BasicGet(string queueName, Func<byte[], Task> action);
    }

    public class RabbitManager : IRabbitManager
    {
        private readonly DefaultObjectPool<IModel> _objectPool;

        public RabbitManager(IPooledObjectPolicy<IModel> objectPolicy)
        {
            _objectPool = new DefaultObjectPool<IModel>(objectPolicy, Environment.ProcessorCount * 2);
        }

        public void Publish<T>(T message, string routeKey, string exchangeName = "", string exchangeType = "direct")
            where T : class
        {
            if (message == null)
                return;

            var channel = _objectPool.Get();

            try
            {
                //channel.QueueDeclare(queue: routeKey, durable: true, exclusive: false, autoDelete: false, arguments: null);

                //channel.ExchangeDeclare(exchangeName, exchangeType, true, false, null);

                var sendBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                //var properties = channel.CreateBasicProperties();
                //properties.Persistent = true;

                channel.BasicPublish(exchangeName, routeKey, body: sendBytes);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _objectPool.Return(channel);
            }
        }

        public void Consume(string queueName, string consumerTag, Func<byte[], Task> action)
        {

            var channel = _objectPool.Get();
            try
            {
                channel.BasicQos(0, 1, false);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += async (ch, ea) =>
                {
                    var body = ea.Body;

                    if (body.Length > 0)
                        await action.Invoke(body.ToArray());

                    channel.BasicAck(ea.DeliveryTag, false);
                };

                channel.BasicConsume(consumer: consumer, queue: queueName, autoAck: false, consumerTag: consumerTag);
#if DEBUG                
                Console.WriteLine("Press [enter] to exit.");
                Console.ReadLine();
#else
                Console.WriteLine("Press [x] button to exit.");
                System.Diagnostics.Process.GetCurrentProcess().WaitForExit();
#endif
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Consume Error] {ex.Message}");
                throw ex;
            }
            finally
            {
                _objectPool.Return(channel);
            }
        }

        public void BasicGet(string queueName, Func<byte[], Task> action)
        {
            var channel = _objectPool.Get();
            try
            {
                var message = channel.BasicGet(queueName, true);
                var body = message.Body;

                if (body.Length > 0)
                    action.Invoke(body.ToArray());
#if DEBUG                
                Console.WriteLine("Press [enter] to exit.");
                Console.ReadLine();
#else
                Console.WriteLine("Press [x] button to exit.");
                System.Diagnostics.Process.GetCurrentProcess().WaitForExit();
#endif
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Basic get Error] {ex.Message}");
                throw; // 원본 exception을 전달한다
            }
            finally
            {
                _objectPool.Return(channel);
            }
        }
    }
}
