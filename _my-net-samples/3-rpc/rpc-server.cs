    class Program
    {
        static void Main(string[] args)
        {
            var queueProcessor = new RabbitConsumer()
            {
                Enabled = true
            };

            queueProcessor.Start();

            Console.ReadLine();
        }
    }

    public class RabbitConsumer : IDisposable
    {
        private const string HostName = "localhost";
        private const string UserName = "guest";
        private const string Password = "guest";

        private const string QueueName = "queue"; //<-
        private const bool IsDurable = false; //<-

        private const string VirtualHost = "";
        private int Port = 0;

        public delegate void OnReceiveMessage(string message);

        public bool Enabled { get; set; }
    
        private ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _model;

        public RabbitConsumer()
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
            _model.BasicQos(0, 1, false);
        }

        public void Start()
        {
            var consumer = new QueueingBasicConsumer(_model);
            _model.BasicConsume(QueueName, false, consumer);

            while (Enabled)
            {
                //Получения сообщения
                var deliveryArgs = (BasicDeliverEventArgs)consumer.Queue.Dequeue();

                var message = Encoding.Default.GetString(deliveryArgs.Body);
                Console.WriteLine("Received message: {0}", message);
                var response = string.Format("Processed message - {0} : Response is good", message);

                //Отправка ответа
                var replyProperties = _model.CreateBasicProperties();
                replyProperties.CorrelationId = deliveryArgs.BasicProperties.CorrelationId;
                byte[] messageBuffer = Encoding.Default.GetBytes(response);
                _model.BasicPublish("", deliveryArgs.BasicProperties.ReplyTo, replyProperties, messageBuffer);

                //Подтверждение обработки
                _model.BasicAck(deliveryArgs.DeliveryTag, false);
            }
        }

        public void Dispose()
        {
            if (_model != null)
                _model.Dispose();
            if (_connection != null)
                _connection.Dispose();

            _connectionFactory = null;

            GC.SuppressFinalize(this);
        }
    }