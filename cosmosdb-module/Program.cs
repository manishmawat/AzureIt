using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace cosmosdb_module
{
    class Program
    {
        private DocumentClient client;
        static void Main(string[] args)
        {
            try
            {
                Program p = new Program();
                p.CheckAndCreateCosmosDB().Wait();

                //create user document
                p.CreateUserDocument().Wait();

                //Read user from database
                p.ReadUserDocument("Users", "WebCustomers", p.GetUserObject()).Wait();

                //Replace user document from database
                var updatedUser = p.GetUserObject();
                updatedUser.FirstName = "ChangedName";
                p.ReplaceUserDocument("Users", "WebCustomers", updatedUser).Wait();

                //Delete the user document from database
                p.DeleteUserDocument("Users","WebCustomers",p.GetUserObject()).Wait();
            }
            catch (DocumentClientException de)
            {
                Exception be = de.GetBaseException();
                Console.WriteLine($"Status Code: {de.StatusCode}, Message: {de.Message}, BaseExceptionMessage: {be.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
            finally
            {
                Console.WriteLine("Press any key to exit.");
                Console.ReadLine();
            }

        }

        private User GetUserObject()
        {
            return new User
            {
                Id = "3",
                UserId = "manish.mawat@contoso.com",
                LastName = "K",
                FirstName = "Manish",
                Email = "manish.mawat@contoso.com",
                OrderHistory = new OrderHistory[]
                {
                    new OrderHistory{
                        OrderId="1000",
                        DateShipped="05/11/2018",
                        Total="52.49"
                    }
                },
                ShippingPreference = new ShippingPreference[]
                {
                    new ShippingPreference
                    {
                        Priority = 1,
                        AddressLine1 = "90 W 8th St",
                        City = "New York",
                        State = "NY",
                        ZipCode = "10001",
                        Country = "USA"
                    }
                }
            };
        }
        private async Task CreateUserDocument()
        {
            var user1 = GetUserObject();
            await this.CreateUserDocumentIfNotExist("Users", "WebCustomers", user1);
        }
        private async Task CheckAndCreateCosmosDB()
        {
            this.client = new DocumentClient(new Uri(ConfigurationManager.AppSettings["accountEndpoint"]),
                                                        ConfigurationManager.AppSettings["accountKey"]);
            await this.client.CreateDatabaseIfNotExistsAsync(new Database { Id = "Users" });
            await this.client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri("Users"),
                                                                new DocumentCollection { Id = "WebCustomers" });
            Console.WriteLine("Document and Collection validation completed");
        }

        private void WriteToConsoleAndPromptToContinue(string format, params object[] args)
        {
            Console.WriteLine(format, args);
            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();
        }
        private async Task CreateUserDocumentIfNotExist(string databaseName, string collectionName,
                                                                User userDocument)
        {
            // try
            // {
            //     var user = this.client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, userDocument.Id), new RequestOptions { PartitionKey = new PartitionKey(userDocument.UserId) });
            //     if (user != null)
            //     {
            //         this.WriteToConsoleAndPromptToContinue("User {0} is already exist in the database.", userDocument.Id);
            //     }
            //     else
            //     {
            //         await this.client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), userDocument);
            //     }
            // }
            // catch(Exception ex)
            // {
            //     throw ex;
            // }
            try
            {
                await this.client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, userDocument.Id), new RequestOptions { PartitionKey = new PartitionKey(userDocument.UserId) });
                this.WriteToConsoleAndPromptToContinue("User {0} already exists in the database", userDocument.Id);
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await this.client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), userDocument);
                    this.WriteToConsoleAndPromptToContinue("Created User {0}", userDocument.Id);
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task ReadUserDocument(string databaseName, string collectionName, User userDocument)
        {
            try
            {
                var user = await this.client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, userDocument.UserId),
                                                    new RequestOptions { PartitionKey = new PartitionKey(userDocument.UserId) });
                this.WriteToConsoleAndPromptToContinue("Record with name : {0} {1} fetched from the database",
                                                    new string[] { userDocument.FirstName, userDocument.LastName });
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    this.WriteToConsoleAndPromptToContinue("User {0} not read", userDocument.Id);
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task ReplaceUserDocument(string databaseName, string collectionName, User updatedUser)
        {
            try
            {
                await this.client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, updatedUser.Id), updatedUser, new RequestOptions { PartitionKey = new PartitionKey(updatedUser.UserId) });
                this.WriteToConsoleAndPromptToContinue("Replaced last name for {0}", updatedUser.LastName);
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    this.WriteToConsoleAndPromptToContinue("User {0} not found for replacement", updatedUser.Id);
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task DeleteUserDocument(string databaseName, string collectionName, User deletedUser)
        {
            try
            {
                await this.client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, deletedUser.Id), new RequestOptions { PartitionKey = new PartitionKey(deletedUser.UserId) });
                Console.WriteLine("Deleted user {0}", deletedUser.Id);
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    this.WriteToConsoleAndPromptToContinue("User {0} not found for deletion", deletedUser.Id);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
