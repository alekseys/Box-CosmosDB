using System;
using System.Collections.Generic;
using System.Text;

namespace Box.Samples.WealthManagement.Setup
{
    public class AppSettings
    {
        public string RootFolderParentId { get; set; }

        public string RootFolder { get; set; }

        public List<Folder> Folders { get; set; }

        public string ConfigFilePath { get; set; }
    }

    public partial class Folder
    {
        public string Name { get; set; }

        public string Parent { get; set; }

        public string CollobGroup { get; set; }
    }
}
