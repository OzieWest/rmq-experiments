    class Program
    {        
        private static void Main(string[] args)
        {
            var topics = new List<string>();
            var messageSender = new RabbitSender();

            topics.Add("msg 1");
            topics.Add("msg 2");
            topics.Add("msg 3");

            var message = string.Format("Message: {0}", messageCount);            

            var routingkey = messageSender.Send(message, topics);

            MessageBox.Show(string.Format("Sending Message - {0}, Routing Key - {1}", message, routingkey), "Message sent");

            messageCount++;
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

        public string Send(string message, List<string> topics)
        {
            //Настройка
            var properties = _model.CreateBasicProperties();
            properties.SetPersistent(true);

            //Сериализация
            byte[] messageBuffer = Encoding.Default.GetBytes(message);

            //Создание "routing key" из "topics"
            var routingKey = topics.Aggregate(string.Empty, (current, key) => current + (key.ToLower() + "."));
            if (routingKey.Length > 1)
                routingKey = routingKey.Remove(routingKey.Length - 1, 1);

            Console.WriteLine("Routing key from topics: {0}", routingKey);

            //Отправка
            _model.BasicPublish(ExchangeName, routingKey, properties, messageBuffer);

            return routingKey;
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
