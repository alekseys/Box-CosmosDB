using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Threading.Tasks;

namespace Box.Samples.WealthManagement.Azure.Component
{
    public class FileRepository
    {
        private readonly DocumentClient client;
        private readonly string databaseName;
        private readonly string collectionName;

        public FileRepository(DocumentClient client, string databaseName, string collectionName)
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
        /// <param name="fileId"></param>
        /// <returns></returns>
        public async Task<FileData> GetAsync(string fileId)
        {
            Uri documentUri = UriFactory.CreateDocumentUri(databaseName, collectionName, fileId);

            return await client.ReadDocumentAsync<FileData>(
                documentUri,
                new RequestOptions { PartitionKey = new PartitionKey(fileId) });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileData"></param>
        /// <returns></returns>
        public async Task AddAsync(FileData fileData)
        {
            // Use the atomic Upsert for Insert or Replace
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(databaseName, collectionName);
            await client.UpsertDocumentAsync(collectionUri, fileData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public async Task RemoveAsync(String fileId)
        {
            Uri documentUri = UriFactory.CreateDocumentUri(databaseName, collectionName, fileId);

            await client.DeleteDocumentAsync(
                documentUri,
                new RequestOptions { PartitionKey = new PartitionKey(fileId) });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updatedFileData"></param>
        /// <returns></returns>
        public async Task UpdateAsync(FileData updatedFileData)
        {
            var documentUri = UriFactory.CreateDocumentUri(databaseName, collectionName, updatedFileData.Id);

            FileData fileData = await client.ReadDocumentAsync<FileData>(
                documentUri,
                new RequestOptions { PartitionKey = new PartitionKey(updatedFileData.Id) });

            var condition = new AccessCondition { Condition = fileData.ETag, Type = AccessConditionType.IfMatch };
            await client.ReplaceDocumentAsync(documentUri, updatedFileData, new RequestOptions { AccessCondition = condition });
        }
    }
}
