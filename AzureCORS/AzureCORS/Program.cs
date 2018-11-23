using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using System.Configuration;
using System.Threading.Tasks;

namespace AzureCORS
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var storageConnectionString = ConfigurationManager.AppSettings.Get("StorageConnectionString");
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            CreateCORSPolicy(cloudBlobClient).GetAwaiter().GetResult();
        }

        static async Task CreateCORSPolicy(CloudBlobClient cloudBlobClient)
        {
            ServiceProperties serviceProperties = new ServiceProperties();
            serviceProperties.Cors = new CorsProperties();

            CorsRule corsRule = new CorsRule();
            corsRule.AllowedHeaders = new List<string>() { "*" };
            corsRule.ExposedHeaders = new List<string>() { "*" };
            corsRule.AllowedMethods = CorsHttpMethods.Post;
            corsRule.AllowedOrigins = new List<string>() { "https://localhost:8080/Books" };
            corsRule.MaxAgeInSeconds = 3600;

            serviceProperties.Cors.CorsRules.Add(corsRule);
            //try
            //{
               await cloudBlobClient.SetServicePropertiesAsync(serviceProperties);
            //}
            //catch(Exception ex)
            //{
            //    Console.WriteLine($"Error encountered in setting up the policy: {ex}");
            //}
        }
    }
}
