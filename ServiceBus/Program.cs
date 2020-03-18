using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBus
{
    class Program
    {
        static string ConnectionString = "Endpoint=sb://test-bus-test.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=EkLD4HvmS+qS3AiVua9g10BVUK395bC0AWbWK2/RjMc=";
        static string QueueName = "queue";
        static IQueueClient client;
        const int messageCount = 10;

        static async Task Main(string[] args)
        {
            client = new QueueClient(ConnectionString, QueueName);
            var mes = CreateMessages();
            await SendMessages(mes);
            Console.ReadKey();
            await client.CloseAsync();
        }

        static async Task SendMessages(IEnumerable<string> messages)
        {
            try
            {
                foreach(string m in messages)
                {
                    var bytes = new Message(Encoding.UTF8.GetBytes(m));
                    bytes.MessageId = Guid.NewGuid().ToString();
                    bytes.SessionId = Guid.NewGuid().ToString();
                    Console.WriteLine($"Send {m}");
                    await client.SendAsync(bytes);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static IEnumerable<string> CreateMessages()
        {
            var message = new List<string>();
            for(int i = 0; i < messageCount; i++)
            {
                message.Add($"Message {i}");
            }

            return message;
        }

    }
}
