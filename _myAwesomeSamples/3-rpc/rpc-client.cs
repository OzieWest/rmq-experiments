    class Program
    {
        private static void Main(string[] args)
        {
            var messageCount = 0;
            var sender = new RabbitSender();

            while (true)
            {
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Q)
                    break;

                if (key.Key != ConsoleKey.Enter) 
                    continue;

                var message = string.Format("Message: {0}", messageCount);
                Console.WriteLine("Sending - {0}", message);
                    
                var response = sender.Send(message, new TimeSpan(0, 0, 3, 0));

                Console.WriteLine("Response - {0}", response);
                messageCount++;
            }
            
            Console.ReadLine();
        }
    }

    public class RabbitSender : IDisposable
    {
        private const string HostName = "localhost";
        private const string UserName = "guest";
        private const string Password = "guest";
        private const string QueueName = "queue";
        private const bool IsDurable = false;

        private const string VirtualHost = "";
        private int Port = 0;

        private string _responseQueue;
        private ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _model;
        private QueueingBasicConsumer _consumer;

        public RabbitSender()
        {
            _connectionFactory = new ConnectionFactory
            {
                HostName = HostName,
                UserName = UserName,
                Password = Password
            };

            if (string.IsNullOrEmpty(VirtualHost) == false)
                _connectionFactory.VirtualHost = VirtualHost;

            if (Port > 0)
                _connectionFactory.Port = Port;

            _connection = _connectionFactory.CreateConnection();
            _model = _connection.CreateModel();

            //Create dynamic response queue
            _responseQueue = _model.QueueDeclare().QueueName;
            _consumer = new QueueingBasicConsumer(_model);
            _model.BasicConsume(_responseQueue, true, _consumer);
        }

        public string Send(string message, TimeSpan timeout)
        {
            var correlationToken = Guid.NewGuid().ToString();

            //Настройка
            var properties = _model.CreateBasicProperties();
            properties.ReplyTo = _responseQueue;
            properties.CorrelationId = correlationToken;

            //Сериализация
            byte[] messageBuffer = Encoding.Default.GetBytes(message);

            //Отправка
            var timeoutAt = DateTime.Now + timeout;
            _model.BasicPublish("", QueueName, properties, messageBuffer);

            //Ожидание ответа
            while (DateTime.Now <= timeoutAt)
            {
                var deliveryArgs = (BasicDeliverEventArgs)_consumer.Queue.Dequeue();
                if (deliveryArgs.BasicProperties != null && deliveryArgs.BasicProperties.CorrelationId == correlationToken)
                {
                    var response = Encoding.Default.GetString(deliveryArgs.Body);
                    return response;
                }
            }
         
            throw new TimeoutException(@"The response was not returned before the timeout");
        }

        public void Dispose()
        {
            if (_connection != null)
                _connection.Close();
            
            if (_model != null && _model.IsOpen)
                _model.Abort();

            _connectionFactory = null;

            GC.SuppressFinalize(this);
        }
    }
