using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MyRabbitMQConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            //初始化連線資訊
            var factory = new ConnectionFactory()
            {                
                //設定連線 RabbitMQ username
                UserName = "ap",
                //設定 RabbitMQ password
                Password = "F0727RabbitMQ",
                //自動回復連線
                AutomaticRecoveryEnabled = true,
                //心跳檢測頻率
                RequestedHeartbeat = 10,
            };
            
            string exchange = "csw"; //user
            string queue = "chat";//category
            string routingKey = "ap";

            //連線多個 rabbitmq instance
            using (var connection = factory.CreateConnection(new string[2] { "localhost", "localhost" }))
            {
                //處理連線中斷
                connection.ConnectionShutdown += (o, e) =>
                {
                    //handle disconnect      
                    Console.WriteLine($"Fail:{0},{e}");
                };
                //開啟 channel
                using (var channel = connection.CreateModel())
                {
                    //宣告 queues
                    //宣告 exchanges，RabbitMQ提供了四種Exchange模式：fanout,direct,topic,header
                    channel.ExchangeDeclare(exchange, ExchangeType.Direct);
                    //宣告 queues
                    channel.QueueDeclare(queue, true, false, false, null);
                    //將 exchnage、queue 依 route rule 綁定
                    channel.QueueBind(queue, exchange, routingKey, null);

                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                    Console.WriteLine(" [*] Waiting for messages.");
                    //建立 consumer
                    var consumer = new EventingBasicConsumer(channel);
                    channel.BasicConsume(queue: queue, autoAck:false,                            
                            consumer: consumer);
                    //收到訊息時的處理方式
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        Console.WriteLine($" [x] Received {message} from {connection.ToString()}");
                        Console.WriteLine("Expiration=" + ea.BasicProperties.Expiration);
                        Console.WriteLine("Exchange=" + ea.Exchange);
                        Console.WriteLine("Routingkey="+ea.RoutingKey);
                        //手動 ack
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        Console.WriteLine("OK");
                    };
                    Console.WriteLine(" Press [enter] to exit.");
                    //持續等著接收訊息
                    while (true)
                    {
                    }
                }
            }
        }
    }
}
