    class Program
    {
        private const string HostName = "localhost";
        private const string UserName = "guest";
        private const string Password = "guest";

        private static void Main(string[] args)
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = HostName,
                UserName = UserName,
                Password = Password
            };

            createQueue(connectionFactory);
        }

        private static void createQueue(ConnectionFactory factory)
        {
            using(var connection = factory.CreateConnection())
            {
                var model = connection.CreateModel();

                model.QueueDeclare("MyQueue", true, false, false, null);
                model.ExchangeDeclare("MyExchange", ExchangeType.Topic);
                model.QueueBind("MyQueue", "MyExchange", "cars");    
            }
        }

        private static void sendMessage(ConnectionFactory factory, string ExchangeName, string QueueName)
        {
            using(var connection = factory.CreateConnection())
            {
                var model = connection.CreateModel();

                //Настройка
                var properties = model.CreateBasicProperties();
                properties.SetPersistent(false);

                //Сериализация
                byte[] messageBuffer = Encoding.Default.GetBytes("this is my message");

                //Отправка
                model.BasicPublish(ExchangeName, QueueName, properties, messageBuffer);
            }
        }
    }