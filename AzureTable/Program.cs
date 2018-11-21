using System;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Azure;
using System.Configuration;
using System.Threading.Tasks;

namespace AzureTable
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var StorageConnectionString = ConfigurationManager.AppSettings.Get("StorageConnectionString");
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(StorageConnectionString);

            //create storage table
            CreateTable(cloudStorageAccount).GetAwaiter().GetResult();
            //InsertEntity(cloudStorageAccount).GetAwaiter().GetResult();
            RetrieveEntity(cloudStorageAccount, "blog", "1").GetAwaiter().GetResult();
        }

        public async static Task CreateTable(CloudStorageAccount cloudStorageAccount)
        {
            var cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
            var cloudTableReference = cloudTableClient.GetTableReference("MyFirstTableStorage");
            await cloudTableReference.CreateIfNotExistsAsync();
        }

        public static async Task InsertEntity(CloudStorageAccount cloudStorageAccount)
        {
            var cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
            var cloudTableReference = cloudTableClient.GetTableReference("MyFirstTableStorage");
            var blog = new BlogEntity(1, "Manish", "My Title", "Description");
            TableOperation insert = TableOperation.Insert(blog);
            await cloudTableReference.ExecuteAsync(insert);
        }

        public static async Task RetrieveEntity(CloudStorageAccount cloudStorageAccount, 
            string partitionKey, string rowKey)
        {
            var cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
            var cloudTableReference = cloudTableClient.GetTableReference("MyFirstTableStorage");
            TableOperation retrieve = TableOperation.Retrieve<BlogEntity>(partitionKey, rowKey);
            var result = await cloudTableReference.ExecuteAsync(retrieve);
            if(result.Result == null)
            {
                Console.WriteLine("Not able to find the results");
            }
            else
            {
                Console.WriteLine($"Blog found for author: {((BlogEntity)result.Result).Author}");
            }

        }
    }

    public class BlogEntity : TableEntity
    {
        public BlogEntity()
        {

        }
        public BlogEntity(int ID, string author, string title, string description)
        {
            this.UniqueID = ID;
            this.Author = author;
            this.Title = title;
            this.Description = description;
            this.PartitionKey = "blog";
            this.RowKey = ID.ToString();
        }

        public int UniqueID { get; set; }
        public string Author { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
