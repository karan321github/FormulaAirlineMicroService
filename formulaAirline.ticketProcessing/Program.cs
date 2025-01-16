// See https://aka.ms/new-console-template for more information
using System.Data.Common;
using System.Text;
using System.Threading.Channels;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

Console.WriteLine("Hello, World!");
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
var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, EventArgs) =>
{
    var body = EventArgs.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);

    Console.WriteLine($"New ticket processing is initiated - {message}");
};

channel.BasicConsume("bookings", true, consumer);
Console.ReadKey();