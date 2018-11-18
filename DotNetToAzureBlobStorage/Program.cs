using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace DotNetToAzureBlobStorage
{
    class Program
    {
        private static string connectionString;
        static void Main(string[] args)
        {
            //connectionString = ConfigurationManager.AppSettings.Get("StorageConnectionString");
            //CloudStorageContainerreference:
            AzureBlobWork().GetAwaiter().GetResult();
        }

        private static async Task AzureBlobWork()
        {
            var connectionString = ConfigurationManager.AppSettings.Get("StorageConnectionString");
            if (CloudStorageAccount.TryParse(connectionString, out var storageAccount))
            {
                CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
                var cloudBlobContainer = cloudBlobClient.GetContainerReference("mydotcoreblobcontainer" + Guid.NewGuid().ToString());
                await cloudBlobContainer.CreateAsync();

                BlobContainerPermissions permissions = new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                };
                await cloudBlobContainer.SetPermissionsAsync(permissions);

                //Uploading blob to the Storage
                string localPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string localFile = "FileToUploadOnBlob" + Guid.NewGuid() + ".txt";
                string sourceFile = Path.Combine(localPath, localFile);
                await File.WriteAllTextAsync(sourceFile, "Hello Manish");

                Console.WriteLine($"Temp File = {sourceFile}");
                Console.WriteLine($"Uploading to Blog Storage as blob {localFile}");

                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(localFile);
                await cloudBlockBlob.UploadFromFileAsync(sourceFile);

                //List down all the blobs in the container on Storage.
                Console.WriteLine("List blobs in the container.");
                BlobContinuationToken blobContinuationToken = null;
                do
                {
                    var results = await cloudBlobContainer.ListBlobsSegmentedAsync(null, blobContinuationToken);
                    foreach (var item in results.Results)
                    {
                        Console.WriteLine(item.Uri);
                    }
                } while (blobContinuationToken != null);

                //Download the blob from Azure Storage
                //Download the file with same reference created earlier for the file.
                Console.WriteLine("Downloading file from blob Azure Storage");
                var destinationFile = sourceFile.Replace(".txt", "_Downloaded.txt");
                await cloudBlockBlob.DownloadToFileAsync(destinationFile, FileMode.Create);

                //Copy Blob files.
                CopyBlobAsync(cloudBlobContainer, localFile);
            }
            else
            {
                Console.WriteLine("Not able to connect to Azure Storage");
            }
            Console.WriteLine("Blob work completed");
        }

        static void CopyBlobAsync(CloudBlobContainer cloudBlobContainer, string fileName)
        {
            //put the blob name, which you want to copy.
            CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);
            CloudBlockBlob cloudBlockBlob_copied = cloudBlobContainer.GetBlockBlobReference($"{Path.GetFileNameWithoutExtension(fileName)}_copied.txt");
            cloudBlockBlob_copied.StartCopyAsync(cloudBlockBlob);
        }
    }
}
