using System;
using Newtonsoft.Json;

namespace Box.Samples.WealthManagement.Azure.Component
{
    public class FileData
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("file_version")]
        public FileVersion FileVersion { get; set; }

        [JsonProperty("sequence_id")]
        public long SequenceId { get; set; }

        [JsonProperty("_etag")]
        public String ETag { get; set; }

        [JsonProperty("sha1")]
        public string Sha1 { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("path_collection")]
        public PathCollection PathCollection { get; set; }

        [JsonProperty("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonProperty("modified_at")]
        public DateTime? ModifiedAt { get; set; }

        [JsonProperty("trashed_at")]
        public DateTime? TrashedAt { get; set; }

        [JsonProperty("purged_at")]
        public DateTime? PurgedAt { get; set; }

        [JsonProperty("content_created_at")]
        public DateTime? ContentCreatedAt { get; set; }

        [JsonProperty("content_modified_at")]
        public DateTime? ContentModifiedAt { get; set; }

        [JsonProperty("created_by")]
        public FileUser CreatedBy { get; set; }

        [JsonProperty("modified_by")]
        public FileUser ModifiedBy { get; set; }

        [JsonProperty("owned_by")]
        public FileUser OwnedBy { get; set; }

        [JsonProperty("shared_link")]
        public object SharedLink { get; set; }

        [JsonProperty("parent")]
        public Parent Parent { get; set; }

        [JsonProperty("item_status")]
        public string ItemStatus { get; set; }
    }

    public class FileUser
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("login")]
        public string Login { get; set; }
    }

    public class FileVersion
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("sha1")]
        public string Sha1 { get; set; }
    }

    public class Parent
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("sequence_id")]
        public long? SequenceId { get; set; }

        [JsonProperty("etag")]
        public long? Etag { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class PathCollection
    {
        [JsonProperty("total_count")]
        public long TotalCount { get; set; }

        [JsonProperty("entries")]
        public Parent[] Entries { get; set; }
    }
}

