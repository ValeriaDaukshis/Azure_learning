using Microsoft.Azure.ServiceBus;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceBusReceiver
{
    class Program
    {
        static string ConnectionString = "";
        static string QueueName = "";
        static IQueueClient client;

        static void Main(string[] args)
        {
            ClientCrieter().GetAwaiter().GetResult();
        }

        static async Task ClientCrieter()
        {
            client = new QueueClient(ConnectionString, QueueName);
            GetMessages();
            Console.ReadKey();
            await client.CloseAsync();
        }

        static void GetMessages()
        {
            var messageOptions = new SessionHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentSessions = 1,
                AutoComplete = false
            };

            client.RegisterSessionHandler(ProcessMessagesAsync, messageOptions);
        }

        static async Task ProcessMessagesAsync(IMessageSession session, Message message, CancellationToken token)
        {
            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Text:{Encoding.UTF8.GetString(message.Body)}");

            await client.CompleteAsync(message.SystemProperties.LockToken);
        }

        static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }
    }
}
