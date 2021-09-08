using ChatApp.Data.DTOs;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatApp.Web.RabbitMQ
{
    public class AddMessageToQeueue
    {
        private ConnectionFactory connectionFactory;

        public AddMessageToQeueue()
        {
            connectionFactory = new ConnectionFactory{ Uri = new Uri("Insert CloudAmqp Credentials Here") };

        }
        public void SendMessageToQueue(MessageDto messageDto)
        {
            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "NewMessage",
                                     durable:false,
                                     exclusive:false,
                                     autoDelete:false,
                                     arguments:null
                    );
                var body = JsonSerializer.SerializeToUtf8Bytes(messageDto);

                channel.BasicPublish(exchange: "",
                                     routingKey: "NewMessage",
                                     basicProperties:null,
                                     body:body);
            }
        }
    }
}
