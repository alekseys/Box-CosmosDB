using Box.Samples.WealthManagement.Azure.Component;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Box.Samples.WealthManagement.Setup
{
    class Program
    {
        static void Main(string[] args)
        {

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var appSettings = new AppSettings();
            builder.Build().Bind(appSettings);
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")) + ".txt";
            using (var writer = new StreamWriter(filePath))
            {
                writer.AutoFlush = true;
                Console.SetOut(writer);
                Process(appSettings);
            }
            //Console.ReadLine();
        }

        private static void Process(AppSettings appSettings)
        {

            var boxManager = new BoxManager(appSettings.ConfigFilePath);

            //Create root folder
            var rootFolder = boxManager.CreateFolderAsync(appSettings.RootFolder, appSettings.RootFolderParentId).Result;
            Console.WriteLine($"Log Entry: {DateTime.Now}: root folder id: {rootFolder.Id}, name: {rootFolder.Name}");

            var groupsDictionary = new Dictionary<string, string>();
            var foldersDictionary = new Dictionary<string, string>
            {
                //add root folder id in dictionary
                { rootFolder.Name, rootFolder.Id }
            };

            foreach (var folder in appSettings.Folders)
            {
                //process folders
                string parentFolderId;
                if (!foldersDictionary.TryGetValue(folder.Parent, out parentFolderId))
                {
                    Console.WriteLine("Log Entry: {DateTime.Now}: Parent folder is not found.");

                    var parentFolder = boxManager.CreateFolderAsync(folder.Name, parentFolderId).Result;
                    parentFolderId = parentFolder.Id;
                    Console.WriteLine($"Log Entry: {DateTime.Now}: New Parent folder id: {parentFolder.Id}, name: {parentFolder.Name}");

                    foldersDictionary.Add(parentFolder.Name, parentFolder.Id);
                    Console.WriteLine($"Log Entry: {DateTime.Now}: New parent folder id: {parentFolder.Id}, name: {parentFolder.Name} added to the dictionary.");
                }

                var boxFolder = boxManager.CreateFolderAsync(folder.Name, parentFolderId).Result;
                Console.WriteLine($"Log Entry: {DateTime.Now}: Folder id: {boxFolder.Id}, name: {boxFolder.Name} created.");

                foldersDictionary.Add(boxFolder.Name, boxFolder.Id);
                Console.WriteLine($"Log Entry: {DateTime.Now}: New folder id: {boxFolder.Id}, name: {boxFolder.Name} added to the dictionary.");

                //process groups and add collaboration
                if (string.IsNullOrEmpty(folder.CollobGroup))
                    continue;

                string groupId;
                if (!groupsDictionary.TryGetValue(folder.CollobGroup, out groupId))
                {
                    Console.WriteLine("Log Entry: {DateTime.Now}: Group is not found.");
                    var group = boxManager.GetBoxGroupAsync(folder.CollobGroup).Result;
                    if(group == null)
                    {
                        group = boxManager.CreateBoxGroupAsync(folder.CollobGroup).Result;
                    }
                    
                    groupId = group.Id;
                    Console.WriteLine($"Log Entry: {DateTime.Now}: New group id: {group.Id}, name: {group.Name} created.");

                    groupsDictionary.Add(folder.CollobGroup, groupId);
                    Console.WriteLine($"Log Entry: {DateTime.Now}: New group id: {group.Id}, name: {group.Name} added to the dictionary.");
                }

                var collob = boxManager.AddCollaboration(boxFolder.Id, groupId, BoxType.group).Result;
                Console.WriteLine($"Log Entry: {DateTime.Now}: New collaboration id: {collob.Id}, for folder: {collob.Item.Id}, group id: {collob.AccessibleBy.Id} added.");
            }
        }
    }
}
