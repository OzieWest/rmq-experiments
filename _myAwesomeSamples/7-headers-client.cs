    class Program
    {
        private static int messageCount;

        private static void Main(string[] args)
        {
            var headers = new Dictionary<string, string>();
            var messageSender = new RabbitSender();

            headers.Add("material 1", "msg 1");
            headers.Add("material 2", "msg 2");

            var message = string.Format("Message: {0}", messageCount);

            messageSender.Send(message, headers);

            MessageBox.Show(string.Format("Sending Message - {0}", message), "Message sent");

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

        public void Send(string message, Dictionary<string, string> headers)
        {
            //Настройка
            var properties = _model.CreateBasicProperties();
            properties.SetPersistent(true);
            properties.Headers = new ListDictionary();

            foreach (var header in headers)
            {
                Console.WriteLine("Header - Key: {0}, Value: {1}", header.Key, header.Value);
                properties.Headers.Add(header.Key, header.Value);
            }
            
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