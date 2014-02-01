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

                if (key.Key == ConsoleKey.Enter)
                {
                    var message = string.Format("Message: {0}", messageCount);

                    Console.WriteLine(string.Format("Sending - {0}", message));

                    sender.Send(message);
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
        private const string ExchangeName = "queue1";
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

        public void Send(string message)
        {
            //Настройка
            var properties = _model.CreateBasicProperties();
            properties.SetPersistent(true);

            //Сериализация
            byte[] messageBuffer = Encoding.Default.GetBytes(message);

            //Отправка
            _model.BasicPublish(ExchangeName, "", properties, messageBuffer);
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
