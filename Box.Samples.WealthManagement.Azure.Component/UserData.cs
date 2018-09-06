using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Box.Samples.WealthManagement.Azure.Component
{
    public class UserData
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("boxUserId")]
        public string BoxUserId { get; set; }

        [JsonProperty("name")]
        public String Name { get; set; }

        [JsonProperty("_etag")]
        public String ETag { get; set; }

    }
}
