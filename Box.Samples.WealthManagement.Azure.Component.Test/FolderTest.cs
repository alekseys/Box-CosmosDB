using System;
using Xunit;
using Xunit.Abstractions;

namespace Box.Samples.WealthManagement.Azure.Component.Test
{
    public class FolderTest
    {
        private readonly ITestOutputHelper output;

        public FolderTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void CanCreateFolderAndCheckNameAndParentSuccess()
        {
            string configFilePath = @"C:\Projects\Box.Samples.WealthManagement.Azure.ASP.MVC\Box.Samples.WealthManagement.Azure.ASP.MVC\FileVaultBoxConfiguration.json";
            var boxManager = new BoxManager(configFilePath);
            var folderName = "Client";
            var folderDesc = "This is a Client Name folder";
            var parentFolderId = "0";

            var folder = boxManager.CreateFolderAsync(folderName, parentFolderId, folderDesc).Result;
            output.WriteLine($"Folder Id: {folder.Id}");

            Assert.Equal(folder.Name, folderName);
            Assert.Equal(folder.Parent.Id, parentFolderId);
        }
    }
}
