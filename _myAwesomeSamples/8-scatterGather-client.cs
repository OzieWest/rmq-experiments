    class Program
    {
        private static void Main(string[] args)
        {
            var messageCount = 0;
            var sender = new RabbitSender();

            Console.WriteLine("Введите число 1-6 и нажмите Enter, чтобы отправить сообщение");
            int messageNumber = 0;
            while (true)
            {
                var key = Console.ReadKey();
                
                ProcessKeyStroke(ref messageNumber, ref key);                

                if (key.Key == ConsoleKey.Enter)
                {
                    if (messageNumber == 0)
                    {
                        Console.WriteLine("Введите число 1-6 и нажмите Enter, чтобы отправить сообщение");
                    }
                    else
                    {
                        var message = string.Format("Message ID: {0}", messageNumber);
                        Console.WriteLine("Sending - {0}", message);

                        var responses = sender.Send(message, messageNumber.ToString(CultureInfo.InvariantCulture), new TimeSpan(0, 1, 0, 3), 2);

                        Console.WriteLine();
                        Console.WriteLine("{0} replies recieved", responses.Count);
                        Console.WriteLine();
                        Console.WriteLine("Listing responses");

                        foreach (var response in responses)
                            Console.WriteLine("Response - {0}", response);

                        Console.WriteLine();
                        Console.WriteLine();
                        Console.WriteLine("Введите число 1-6 и нажмите Enter, чтобы отправить сообщение");
                        messageCount++;
                    }
                }
            }
            
            Console.ReadLine();
        }

        private static void ProcessKeyStroke(ref int messageNumber, ref ConsoleKeyInfo key)
        {            
            switch (key.Key)
            {
                case ConsoleKey.D1:
                    messageNumber = 1;
                    break;
                case ConsoleKey.D2:
                    messageNumber = 2;
                    break;
                case ConsoleKey.D3:
                    messageNumber = 3;
                    break;
                case ConsoleKey.D4:
                    messageNumber = 4;
                    break;
                case ConsoleKey.D5:
                    messageNumber = 5;
                    break;
                case ConsoleKey.D6:
                    messageNumber = 6;
                    break;
                case ConsoleKey.Enter:
                    break;
                default:
                    messageNumber = 0;
                    break;
            }            
        }
    }

    //=========================================================================================

    public class RabbitSender : IDisposable
    {
        private const string HostName = "localhost";
        private const string UserName = "guest";
        private const string Password = "guest";
        private const bool IsDurable = false;
        private const string ExchangeName = "exchange";

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

            _responseQueue = _model.QueueDeclare().QueueName;
            _consumer = new QueueingBasicConsumer(_model);
            _model.BasicConsume(_responseQueue, true, _consumer);
        }

        public List<string> Send(string message, string routingKey, TimeSpan timeout, int minResponses)
        {
            var responses = new List<string>();
            var correlationToken = Guid.NewGuid().ToString();

            //Настройка
            var properties = _model.CreateBasicProperties();
            properties.ReplyTo = _responseQueue;
            properties.CorrelationId = correlationToken;

            //Сериализация
            byte[] messageBuffer = Encoding.Default.GetBytes(message);

            //Отправка
            var timeoutAt = DateTime.Now + timeout;
            _model.BasicPublish(ExchangeName, routingKey, properties, messageBuffer);

            //Ожидание ответа
            while (DateTime.Now <= timeoutAt)
            {
                object result = null;
                _consumer.Queue.Dequeue(10, out result);
                if (result == null)
                {
                    //В настоящее время нет сообщений в очереди, и если мы уже имеем необходимый минимум то возвращаем его
                    if (responses.Count >= minResponses)
                        return responses;

                    Console.WriteLine("Waiting for responses");
                    Thread.Sleep(new TimeSpan(0, 0, 0, 0, 200));
                    continue;
                }

                var deliveryArgs = (BasicDeliverEventArgs)result;
                if (deliveryArgs.BasicProperties == null ||
                    deliveryArgs.BasicProperties.CorrelationId != correlationToken) continue;

                var response = Encoding.Default.GetString(deliveryArgs.Body);
                Console.WriteLine("Sender got response: {0}", response);
                responses.Add(response);
                
            }
            return responses;
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
