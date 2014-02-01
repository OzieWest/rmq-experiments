    class Program
    {
        private static void Main(string[] args)
        {
            var messageCount = 0;
            string routingKey;
            var sender = new RabbitSender();

            while (true)
            {
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Q)
                    break;

                if (key.Key == ConsoleKey.Enter)
                {
                    //routingKey нужен, чтобы сервер 1, 2 или ни один получил/неполучил сообщение
                    routingKey = new Random().Next(0, 4).ToString(CultureInfo.InvariantCulture);

                    var message = string.Format("Message: {0} - Routing Key: {1}", messageCount, routingKey);
                    Console.WriteLine("Sending - {0}", message);
                    
                    sender.Send(message, routingKey);
                    messageCount++;
                }
            }
            
            Console.ReadLine();
        }
    }

    public class RabbitSender : IDisposable
    {
        private const string HostName = "localhost";
        private const string UserName = "guest";
        private const string Password = "guest";
        private const string ExchangeName = "exchange";
        private const bool IsDurable = true;

        private const string VirtualHost = "";
        private int Port = 0;

        private ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _model;

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
        }

        public void Send(string message, string routingKey)
        {
            //Настройка
            var properties = _model.CreateBasicProperties();
            properties.SetPersistent(true);

            //Сериализация
            byte[] messageBuffer = Encoding.Default.GetBytes(message);

            //Отправка сообщения
            _model.BasicPublish(ExchangeName, routingKey, properties, messageBuffer);
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
