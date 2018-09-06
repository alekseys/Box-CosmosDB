using Box.V2;
using Box.V2.Config;
using Box.V2.JWTAuth;
using Box.V2.Models;
using Box.V2.Models.Request;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Box.Samples.WealthManagement.Azure.Component
{
    public class BoxManager
    {
        private readonly BoxClient adminClient;
        private readonly string configFilePath;

        public BoxManager(string configFilePath)
        {
            this.configFilePath = configFilePath;
            var boxJwt = new BoxJWTAuth(ConfigureBox());
            var adminToken = boxJwt.AdminToken(); //valid for 60 minutes so should be cached and re-used
            adminClient = boxJwt.AdminClient(adminToken);
        }

        public async Task<BoxUser> CreateAppUser(string name, string aadUserObjectId)
        {
            BoxUser boxUser;
            try
            {
                //must set IsPlatformAccessOnly=true for an App User
                var userRequest = new BoxUserRequest()
                {
                    Name = name,
                    IsPlatformAccessOnly = true,
                    ExternalAppUserId = aadUserObjectId
                };
                boxUser = await adminClient.UsersManager.CreateEnterpriseUserAsync(userRequest);
            }
            catch (Exception e)
            {
                throw;
            }

            return boxUser;
        }

        public async Task<BoxUser> GetAppUserAsync(string login)
        {
            BoxUser boxUser;
            try
            {
                var boxUsers = await adminClient.UsersManager.GetEnterpriseUsersAsync();
                var allBoxUsersList = boxUsers.Entries;
                // Get a specific user from allBoxUsersList
                 boxUser = allBoxUsersList.Find(u => u.Login == login);
            }
            catch (Exception e)
            {
                throw;
            }

            return boxUser;
        }

        public async Task<BoxItem> GetFolderAsync(string folderName)
        {
            BoxItem boxFolder;
            try
            {
                var root = await adminClient.FoldersManager.GetInformationAsync("0");
                var items = await adminClient.FoldersManager.GetFolderItemsAsync(root.ItemCollection.Entries[0].Id, 100);
                var folders = items.Entries.OfType<BoxFolder>().ToList();

                // Get a specific user from allBoxUsersList
                boxFolder = folders.Find(f => f.Name.ToLower() == folderName.ToLower());
            }
            catch (Exception e)
            {
                throw;
            }

            return boxFolder;
        }

        public async Task<BoxFolder> CreateFolderAsync(string name, string parentFolderId, string description = null)
        {
            BoxFolder boxFolder;
            try
            {
                //must set IsPlatformAccessOnly=true for an App User
                var folderRequest = new BoxFolderRequest()
                {
                    Name = name,
                    Description = description,
                    Type = BoxType.folder,
                    Parent = new BoxRequestEntity() { Id = parentFolderId }
                };
                boxFolder = await adminClient.FoldersManager.CreateAsync(folderRequest);

            }
            catch (Exception e)
            {
                throw;
            }
            return boxFolder;
        }

        public async Task<BoxGroupMembership> AddGroupMembership(string groupId, string userId)
        {
            BoxGroupMembership boxGroupMembership;
            try
            {
                var boxGroupMembershipRequest = new BoxGroupMembershipRequest()
                {
                    Group = new BoxGroupRequest()
                    {
                        Id = groupId
                    },
                    User = new BoxUserRequest()
                    {
                        Id = userId
                    },
                };

                boxGroupMembership = await adminClient.GroupsManager.AddMemberToGroupAsync(boxGroupMembershipRequest);
            }
            catch (Exception e)
            {
                throw;
            }
            return boxGroupMembership;
        }

        public async Task<BoxCollaboration> AddCollaboration(string folderId, string id, BoxType boxType)
        {
            BoxCollaboration boxCollaboration;
            try
            {
                var collabRequest = new BoxCollaborationRequest()
                {
                    Item = new BoxRequestEntity()
                    {
                        Id = folderId,
                        Type = BoxType.folder
                    },
                    AccessibleBy = new BoxCollaborationUserRequest()
                    {
                        Type = boxType,
                        Id = id
                    },
                    Role = BoxCollaborationRole.Editor.ToString(),
                    CanViewPath = false
                };

                boxCollaboration = await adminClient.CollaborationsManager.AddCollaborationAsync(collabRequest);
            }
            catch (Exception e)
            {
                throw;
            }
            return boxCollaboration;
        }

        public async Task<BoxGroup> GetBoxGroupAsync(string name)
        {
            BoxGroup boxGroup;

            try
            {
                var boxgroups = await adminClient.GroupsManager.GetAllGroupsAsync();
                List<BoxGroup> groups = boxgroups.Entries;

                boxGroup = groups.Find(u => u.Name.ToLower() == name.ToLower());
            }
            catch (Exception)
            {

                throw;
            }

            return boxGroup;
        }

        public async Task<BoxGroup> CreateBoxGroupAsync(string name)
        {
            BoxGroup boxGroup;

            try
            {
                var groupItem = new BoxGroupRequest
                {
                    Name = name
                };
                boxGroup = await adminClient.GroupsManager.CreateAsync(groupItem);
            }
            catch (Exception)
            {

                throw;
            }

            return boxGroup;
        }

        public string GetToken(string boxUserId)
        {
            var boxJwt = new BoxJWTAuth(ConfigureBox());
            return boxJwt.UserToken(boxUserId); //valid for 60 minutes so should be cached and re-used
        }

        private IBoxConfig ConfigureBox()
        {
            IBoxConfig boxConfig;
            using (var fs = new FileStream(configFilePath, FileMode.Open))
            {
                boxConfig = BoxConfig.CreateFromJsonFile(fs);
            }
            return boxConfig;
        }
    }
}
