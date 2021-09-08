using ChatApp.Bussiness.Concrete;
using ChatApp.Data.Context;
using ChatApp.Data.DTOs;
using ChatApp.Data.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ChatApp.MessageConsumer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var serviceProvider = new ServiceCollection().AddDbContext<ChatAppWebContext>(options =>
                   options.UseNpgsql("Insert Connection String Here"), ServiceLifetime.Transient).AddTransient(x => new ChatManager(x.GetRequiredService<ChatAppWebContext>())).BuildServiceProvider();
            var _messageManager = serviceProvider.GetService<ChatManager>();
            var newContext = serviceProvider.GetService<ChatAppWebContext>();

            var factory = new ConnectionFactory
            {
                Uri = new Uri("Insert CloudAmqp Credentials Here")
            };

            using var connection = factory.CreateConnection();
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(
                                     queue: "NewMessage",
                                     exclusive: false,
                                     autoDelete: false,
                                     durable: false,
                                     arguments: null
                                     );

                var consumer = new EventingBasicConsumer(channel);



                consumer.Received += async (model, ea) =>
                 {
                     var body = ea.Body.ToArray();
                     var stringMessage = Encoding.UTF8.GetString(body);
                     MessageDto messageDto = JsonSerializer.Deserialize<MessageDto>(stringMessage);
                     await _messageManager.SendMessage(messageDto);
                     Console.WriteLine(messageDto.Content + " - " + "received");
                 };

                channel.BasicConsume(
                                     queue: "NewMessage",
                                     autoAck: true,
                                     consumer: consumer
                                     );

                while (true)
                {
                    Console.Read();
                }
            }

        }
    }
}
