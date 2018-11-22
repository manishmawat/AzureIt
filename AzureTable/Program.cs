using System;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
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
            RetrieveMany(cloudStorageAccount).GetAwaiter().GetResult();
            Console.ReadLine();
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

        public static async Task InsertMany(CloudStorageAccount cloudStorageAccount)
        {
            var cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
            var cloudTableReference = cloudTableClient.GetTableReference("MyFirstTableStorage");
            var blog = new BlogEntity(2, "Manish_2", "My Title_2", "Description_2");
            TableOperation insert = TableOperation.Insert(blog);
            blog = new BlogEntity(3, "Manish_3", "My Title_3", "Description_3");
            TableOperation insert2 = TableOperation.Insert(blog);
            blog = new BlogEntity(4, "Manish_4", "My Title_4", "Description_4");
            TableOperation insert3 = TableOperation.Insert(blog);
            //{
            //    new BlogEntity(2, "Manish_2", "My Title_2", "Description_2"),
            //    new BlogEntity(3, "Manish_3", "My Title_3", "Description_3"),
            //    new BlogEntity(4, "Manish_4", "My Title_4", "Description_4"),
            //    new BlogEntity(5, "Manish_5", "My Title_5", "Description_5")
            //};
            TableBatchOperation tableOperations = new TableBatchOperation();
            tableOperations.Add(insert);
            tableOperations.Add(insert2);
            tableOperations.Add(insert3);
            await cloudTableReference.ExecuteBatchAsync(tableOperations);
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

        public static async Task RetrieveMany(CloudStorageAccount cloudStorageAccount)
        {
            var cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
            var cloudTableReference = cloudTableClient.GetTableReference("MyFirstTableStorage");
            TableQuery<BlogEntity> tableQuery = new TableQuery<BlogEntity>();
            var results = await cloudTableReference.ExecuteQuerySegmentedAsync<BlogEntity>(tableQuery, null);
            foreach (var result in results)
            {
                Console.WriteLine($"This blog is written by {result.Author} and has description as {result.Description}");
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
