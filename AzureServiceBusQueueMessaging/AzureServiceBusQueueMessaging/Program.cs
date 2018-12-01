using System;
using Microsoft.Azure.ServiceBus;
using AzureServiceBusQueueMessaging;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace AzureServiceBusQueueMessaging
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var conn = "Endpoint=sb://mytestservicebus11.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=rDU0P7qDvl29JKSsXiTNrw0+o0uvGtOllFb/06oth4I=";
            var queueName = "myservicebusqueue";

            IQueueClient queueClient = new QueueClient(conn, queueName);

            var message = new Message(Encoding.UTF8.GetBytes("Message for you from Toronto --11"));
            //queueClient.SendAsync(message);
            SendMessage(queueClient, message).GetAwaiter().GetResult();

            ReadMessage(queueClient).GetAwaiter().GetResult();

            Console.ReadLine();
        }

        static async Task SendMessage(IQueueClient queueClient, Message message)
        {
            await queueClient.SendAsync(message);
        }

        static async Task ReadMessage(IQueueClient queueClient)
        {
            queueClient.RegisterMessageHandler(ProcessMessage, new MessageHandlerOptions(ExceptionReceiverHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            });

            async Task ProcessMessage(Message message, CancellationToken token)
            {
                Console.WriteLine($"Received message: Sequence Number:{message.SystemProperties.SequenceNumber} Body: {Encoding.UTF8.GetString(message.Body)}");
                await queueClient.CompleteAsync(message.SystemProperties.LockToken);
            };

            async Task ExceptionReceiverHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
            {
                Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
                var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
                Console.WriteLine("Exception context for troubleshooting:");
                Console.WriteLine($"- Endpoint: {context.Endpoint}");
                Console.WriteLine($"- Entity Path: {context.EntityPath}");
                Console.WriteLine($"- Executing Action: {context.Action}");
                await Task.CompletedTask;
            }
        }
    }
}
