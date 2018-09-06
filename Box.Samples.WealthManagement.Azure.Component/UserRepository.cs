using System;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Box.Samples.WealthManagement.Azure.Component
{
    public class UserRepository
    {
        private readonly DocumentClient client;
        private readonly string databaseName;
        private readonly string collectionName;

        public UserRepository(DocumentClient client, string databaseName, string collectionName)
        {
            this.client = client;
            this.databaseName = databaseName;
            this.collectionName = collectionName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void CreateCollectionIfNotExists()
        {
            DocumentCollection collection = new DocumentCollection
            {
                Id = collectionName
            };
            collection.PartitionKey.Paths.Add("/id");

            this.client.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri(databaseName),
                collection,
                new RequestOptions { OfferThroughput = 10000 });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<UserData> GetAsync(String userId)
        {
            Uri documentUri = UriFactory.CreateDocumentUri(databaseName, collectionName, userId);

            return await client.ReadDocumentAsync<UserData>(
                documentUri,
                new RequestOptions { PartitionKey = new PartitionKey(userId) });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userData"></param>
        /// <returns></returns>
        public async Task AddAsync(UserData userData)
        {
            // Use the atomic Upsert for Insert or Replace
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(databaseName, collectionName);
            await client.UpsertDocumentAsync(collectionUri, userData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task RemoveAsync(String userId)
        {
            Uri documentUri = UriFactory.CreateDocumentUri(databaseName, collectionName, userId);

            await client.DeleteDocumentAsync(
                documentUri,
                new RequestOptions { PartitionKey = new PartitionKey(userId) });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updatedUserData"></param>
        /// <returns></returns>
        public async Task UpdateAsync(UserData updatedUserData)
        {
            var documentUri = UriFactory.CreateDocumentUri(databaseName, collectionName, updatedUserData.Id);

            UserData userData = await client.ReadDocumentAsync<UserData>(
                documentUri,
                new RequestOptions { PartitionKey = new PartitionKey(updatedUserData.Id ) });

            var condition = new AccessCondition { Condition = userData.ETag, Type = AccessConditionType.IfMatch };
            await client.ReplaceDocumentAsync(documentUri, updatedUserData, new RequestOptions { AccessCondition = condition });
        }
    }
}
