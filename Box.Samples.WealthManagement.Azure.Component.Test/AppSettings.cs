using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Box.Samples.WealthManagement.Azure.Component.Test
{
    public class AppSettings
    {
        [JsonProperty("AadB2CSignupUrlReferrer")]
        public string AadB2CSignupUrlReferrer { get; set; }

        [JsonProperty("AadB2CGraph")]
        public AadB2CGraph AadB2CGraph { get; set; }

        [JsonProperty("AzureComputerVisionApiKey")]
        public string AzureComputerVisionApiKey { get; set; }

        [JsonProperty("CosmosDb")]
        public CosmosDb CosmosDb { get; set; }

        [JsonProperty("Box")]
        public Box Box { get; set; }

        [JsonProperty("Client")]
        public Client Client { get; set; }
    }

    public partial class AadB2CGraph
    {
        [JsonProperty("Tenant")]
        public string Tenant { get; set; }

        [JsonProperty("ClientId")]
        public Guid ClientId { get; set; }

        [JsonProperty("ClientSecret")]
        public string ClientSecret { get; set; }
    }

    public partial class Box
    {
        [JsonProperty("MetadataScope")]
        public string MetadataScope { get; set; }

        [JsonProperty("MetadataTemplate")]
        public string MetadataTemplate { get; set; }

        [JsonProperty("ConfigFileName")]
        public string ConfigFileName { get; set; }
    }

    public partial class Client
    {
        [JsonProperty("clientsFolderName")]
        public string ClientsFolderName { get; set; }

        [JsonProperty("rootFolder")]
        public string RootFolder { get; set; }

        [JsonProperty("folders")]
        public List<Folder> Folders { get; set; }

        [JsonProperty("groups")]
        public List<string> Groups { get; set; }
    }

    public partial class Folder
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("parent", NullValueHandling = NullValueHandling.Ignore)]
        public string Parent { get; set; }

        [JsonProperty("collobUser", NullValueHandling = NullValueHandling.Ignore)]
        public string CollobUser { get; set; }
    }

    public partial class CosmosDb
    {
        [JsonProperty("EndpointUrl")]
        public string EndpointUrl { get; set; }

        [JsonProperty("PrimaryKey")]
        public string PrimaryKey { get; set; }

        [JsonProperty("DatabaseId")]
        public string DatabaseId { get; set; }

        [JsonProperty("DatabaseCollection")]
        public string DatabaseCollection { get; set; }
    }

}
