using Box.Samples.WealthManagement.Azure.Component;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Box.V2.Models;
using System.Diagnostics;
using Microsoft.Azure.Documents.Client;

namespace Box.Samples.WealthManagement.Azure.ASP.MVC
{
    public class User
    {
        private AppSettings appSettings;
        private DocumentClient documentClient;

        public User(AppSettings appSettings, DocumentClient documentClient)
        {
            this.appSettings = appSettings;
            this.documentClient = documentClient;
        }

        public async Task<UserData> PostSignUpHandlerAsync(ClaimsIdentity identity)
        {
            UserData userData;
            var aadUserId = identity.Claims.FirstOrDefault(x => x.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
            var name = identity.Claims.FirstOrDefault(x => x.Type == "name")?.Value;

            // Create new user in Box by setting external id to AAD object id 
            var boxManager = new BoxManager(appSettings.Box.ConfigFilePath);
            var boxUser = await boxManager.CreateAppUser(name, aadUserId);

            //update custom attribute in AAD with box app user id
            var userUpdateJson = "{'extension_a7b648b02a8a4f048356172744148536_boxUserId': '" + boxUser.Id + "'}";
            var tenant = appSettings.AadB2CGraph.Tenant;
            var clientId = appSettings.AadB2CGraph.ClientId;
            var clientSecret = appSettings.AadB2CGraph.ClientSecret;

            var client = new AadB2CGraphClient(clientId, clientSecret, tenant);
            var formatted = JsonConvert.DeserializeObject(client.UpdateUser(aadUserId, userUpdateJson).Result);

            userData = new UserData
            {
                Id = aadUserId,
                Name = name,
                BoxUserId = boxUser.Id
            };

            //Save user in cosmos db
            await SaveUserAsync(userData);

            //Box folder setup
            BoxContentSetup(boxUser, boxManager);

            //Box membership setup
            BoxMembershipSetup(boxUser, boxManager);

            return userData;
        }

        private async Task SaveUserAsync(UserData userData)
        {
            //Add to Cosmos DB
            var databaseName = appSettings.CosmosDb.DatabaseId;
            var userCollection = appSettings.CosmosDb.UsersCollection;

            var userRepository = new UserRepository(documentClient, databaseName, userCollection);
            await userRepository.AddAsync(userData);
        }

        private void BoxContentSetup(BoxUser boxUser, BoxManager boxManager)
        {
            //get Clients folder id
            var clientsFolder = boxManager.GetFolderAsync(appSettings.Client.RootFolder).Result;

            //add folder with user name
            var clientFolder = boxManager.CreateFolderAsync(boxUser.Name, clientsFolder.Id).Result;
            var clientRootFolder = "Client Folder";

            //add my docs and other folders
            var foldersDictionary = new Dictionary<string, string>
            {
                //add root folder id in dictionary
                { clientRootFolder, clientFolder.Id }
            };

            foreach (var folder in appSettings.Client.Folders)
            {
                //process folders
                string parentFolderId;
                if (!foldersDictionary.TryGetValue(folder.Parent, out parentFolderId))
                {
                    Debug.WriteLine("Log Entry: {DateTime.Now}: Parent folder is not found.");

                    var parentFolder = boxManager.CreateFolderAsync(folder.Name, parentFolderId).Result;
                    parentFolderId = parentFolder.Id;
                    Debug.WriteLine($"Log Entry: {DateTime.Now}: New Parent folder id: {parentFolder.Id}, name: {parentFolder.Name}");

                    foldersDictionary.Add(parentFolder.Name, parentFolder.Id);
                    Debug.WriteLine($"Log Entry: {DateTime.Now}: New parent folder id: {parentFolder.Id}, name: {parentFolder.Name} added to the dictionary.");
                }

                //prepend client name to the folder that is shared to the broker so broker can identiy it by name
                var folderName = folder.Name;
                if (!string.IsNullOrEmpty(folder.CollobUser))
                {
                    folderName = $"{boxUser.Name} - {folderName}";
                }

                var boxFolder = boxManager.CreateFolderAsync(folderName, parentFolderId).Result;
                Debug.WriteLine($"Log Entry: {DateTime.Now}: Folder id: {boxFolder.Id}, name: {boxFolder.Name} created.");

                foldersDictionary.Add(boxFolder.Name, boxFolder.Id);
                Debug.WriteLine($"Log Entry: {DateTime.Now}: New folder id: {boxFolder.Id}, name: {boxFolder.Name} added to the dictionary.");

                //collob client to all top level folders under client folder
                if (folder.Parent.ToLower() == clientRootFolder.ToLower())
                {
                    //add user collob to my docs folder
                    var collob = boxManager.AddCollaboration(boxFolder.Id, boxUser.Id, BoxType.user).Result;
                    Debug.WriteLine($"Log Entry: {DateTime.Now}: New collaboration id: {collob.Id}, for folder: {collob.Item.Id}, user id: {collob.AccessibleBy.Id} added.");
                }

                //process user collaboration
                if (string.IsNullOrEmpty(folder.CollobUser))
                    continue;

                //get user (broker) to and add collob
                var broker = boxManager.GetAppUserAsync(folder.CollobUser).Result;
                var brokerCollob = boxManager.AddCollaboration(boxFolder.Id, broker.Id, BoxType.user).Result;
                Debug.WriteLine($"Log Entry: {DateTime.Now}: New collaboration id: {brokerCollob.Id}, for folder: {brokerCollob.Item.Id}, user id: {brokerCollob.AccessibleBy.Id} added.");
            }
        }

        private void BoxMembershipSetup(BoxUser boxUser, BoxManager boxManager)
        {
            //add user to all groups as a member
            var groupsDictionary = new Dictionary<string, string>();
            foreach (var group in appSettings.Client.Groups)
            {
                string groupId;
                if (!groupsDictionary.TryGetValue(group, out groupId))
                {
                    Debug.WriteLine("Log Entry: {DateTime.Now}: Group is not found in the dictionary.");
                    var boxGroup = boxManager.GetBoxGroupAsync(group).Result;
                    if (boxGroup == null)
                    {
                        Debug.WriteLine("Log Entry: {DateTime.Now}: Group is not found in Box.");
                        throw new InvalidOperationException($"Group {group} does not exist to add client");
                    }

                    groupId = boxGroup.Id;
                    groupsDictionary.Add(group, groupId);
                    Debug.WriteLine($"Log Entry: {DateTime.Now}: Retrived group id: {boxGroup.Id}, name: {boxGroup.Name} added to the dictionary.");
                }
                var boxMembership = boxManager.AddGroupMembership(groupId, boxUser.Id).Result;
                Debug.WriteLine($"Log Entry: {DateTime.Now}: New membership addes for group id: {groupId}, user: {boxUser.Id}.");
            }
        }
    }
}
