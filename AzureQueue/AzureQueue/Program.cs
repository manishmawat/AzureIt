using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Configuration;
using System.Threading.Tasks;

namespace AzureQueue
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var StorageConnectionString = ConfigurationManager.AppSettings.Get("StorageConnectionString");
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(StorageConnectionString);
            CloudQueueClient cloudQueueClient = cloudStorageAccount.CreateCloudQueueClient();
            CloudQueue cloudQueue = cloudQueueClient.GetQueueReference("samplefirstqueue");
            CreateQueue(cloudQueue).GetAwaiter().GetResult();

            InsertMessage(cloudQueue).GetAwaiter().GetResult();
            PeekMessage(cloudQueue).GetAwaiter().GetResult();
            PopMessage(cloudQueue).GetAwaiter().GetResult();
            Console.ReadLine();
        }

        public async static Task CreateQueue(CloudQueue cloudQueue)
        {
            await cloudQueue.CreateIfNotExistsAsync();
        }

        public async static Task InsertMessage(CloudQueue cloudQueue)
        {
            CloudQueueMessage cloudQueueMessage = new CloudQueueMessage("This is an urgent message from Manish. - Second");
            await cloudQueue.AddMessageAsync(cloudQueueMessage);
        }

        public async static Task PopMessage(CloudQueue cloudQueue)
        {
            var result = await cloudQueue.GetMessageAsync();
            Console.WriteLine($"Message Poped from Storage Queue: {result.AsString}, has properties as DequeueCount: {result.DequeueCount}, ExpirationTime: {result.ExpirationTime}, Id: {result.Id}, InsertionTime: {result.InsertionTime}, NextVisibleTime: {result.NextVisibleTime}");
        }

        public async static Task PeekMessage(CloudQueue cloudQueue)
        {
            var result = await cloudQueue.PeekMessageAsync();
            Console.WriteLine($"Message Poped from Storage Queue: {result.AsString}, has properties as DequeueCount: {result.DequeueCount}, ExpirationTime: {result.ExpirationTime}, Id: {result.Id}, InsertionTime: {result.InsertionTime}, NextVisibleTime: {result.NextVisibleTime}");
        }
    }
}
