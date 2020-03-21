using RabbitMQ.Client;
using System;
using System.Text;

namespace MyRabbitMQProducer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var factory = new ConnectionFactory();
            //設定 RabbitMQ 位置
            //factory.HostName = "localhost";
            //設定連線 RabbitMQ username
            factory.UserName = "ap";
            //設定 RabbitMQ password
            factory.Password = "F0727RabbitMQ";

            using (var connection = factory.CreateConnection(new string[2] { "localhost", "localhost" }))
            {
                //開啟 channel
                using (var channel = connection.CreateModel())
                {
                    string exchange = "csw"; //user
                    string queue = "chat1";//category
                    string routingKey = "ap";
                    //宣告 exchanges，RabbitMQ提供了四種Exchange模式：fanout,direct,topic,header
                    channel.ExchangeDeclare(exchange, ExchangeType.Direct);
                    //宣告 queues
                    channel.QueueDeclare(queue, true, false, false, null);
                    //將 exchnage、queue 依 route rule 綁定
                    channel.QueueBind(queue, exchange, routingKey, null);
                    channel.BasicQos(0, 1, true);
                    string message = $"Hello World-{Guid.NewGuid()}";
                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(exchange, routingKey, new RabbitMQ.Client.Framing.BasicProperties { Persistent = true }, body);
                    Console.WriteLine($"Send Message：{message};{connection.ToString()}");
                }
            }

        }
    }
}
