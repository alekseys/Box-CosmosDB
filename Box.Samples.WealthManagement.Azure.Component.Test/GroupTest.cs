using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Box.Samples.WealthManagement.Azure.Component.Test
{
    public class GroupTest
    {
        [Fact]
        public void CanCreateGroupAndCheckNameSuccess()
        {
            string configFilePath = @"C:\Projects\Box.Samples.WealthManagement.Azure.ASP.MVC\Box.Samples.WealthManagement.Azure.ASP.MVC\FileVaultBoxConfiguration.json";
            var boxManager = new BoxManager(configFilePath);
            var groupName = "Broker Clients Group";

            var group = boxManager.CreateBoxGroupAsync(groupName).Result;
            Assert.Equal(group.Name, groupName);
        }
    }
}
