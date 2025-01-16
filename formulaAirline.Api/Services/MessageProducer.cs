using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;

namespace formulaAirline.Api.Services
{
    public class MessageProducer : IMessageProducer
    {   
        public void SendingMessages<T>(T message)
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "user",
                Password = "password",
                VirtualHost = "/"
            };

            var conn = factory.CreateConnection();
            using var channel = conn.CreateModel();
            channel.QueueDeclare(queue: "bookings", durable: true, exclusive: false, autoDelete: false, arguments: null);

            var jsonString = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(jsonString);

            
            channel.BasicPublish(exchange: "", routingKey: "bookings", basicProperties: null, body: body);
        }

       
    }
}
