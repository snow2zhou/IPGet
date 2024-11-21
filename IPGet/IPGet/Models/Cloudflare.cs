using System.Text.Json.Serialization;
using static IPGet.Models.Cloudflare;

namespace IPGet.Models
{
    public partial class Cloudflare
    {
        public class DNSResult
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }
            [JsonPropertyName("name")]
            public string Name { get; set; }
            [JsonPropertyName("type")]
            public string Type { get; set; }
            [JsonPropertyName("content")]
            public string Content { get; set; }
            [JsonPropertyName("ttl")]
            public int Ttl { get; set; }
        }
        public class ResultInfo
        {
            [JsonPropertyName("page")]
            public int Page { get; set; }
            [JsonPropertyName("per_page")]
            public int PerPage { get; set; }
            [JsonPropertyName("count")]
            public int Count { get; set; }
            [JsonPropertyName("total_count")]
            public int TotalCount { get; set; }
            [JsonPropertyName("total_pages")]
            public int TotalPages { get; set; }
        }


        public class AddResult
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }
            [JsonPropertyName("zone_id")]
            public string ZoneId { get; set; }
            [JsonPropertyName("zone_name")]
            public string ZoneName { get; set; }
            [JsonPropertyName("name")]
            public string Name { get; set; }
            [JsonPropertyName("type")]
            public string Type { get; set; }
            [JsonPropertyName("content")]
            public string Content { get; set; }
            [JsonPropertyName("proxiable")]
            public bool Proxiable { get; set; }
            [JsonPropertyName("proxied")]
            public bool Proxied { get; set; }
            [JsonPropertyName("ttl")]
            public int Ttl { get; set; }
            [JsonPropertyName("settings")]
            public Settings Settings { get; set; }
            [JsonPropertyName("meta")]
            public Meta Meta { get; set; }
            [JsonPropertyName("comment")]
            public string Comment { get; set; }
            [JsonPropertyName("tags")]
            public List<Tags> Tags { get; set; }
            [JsonPropertyName("created_on")]
            public DateTime CreatedOn { get; set; }
            [JsonPropertyName("modified_on")]
            public DateTime Modified_on { get; set; }
        }

        public class Error
        {
            [JsonPropertyName("code")]
            public int Code { get; set; }
            [JsonPropertyName("message")]
            public string Message { get; set; }
        }
        public class Messages
        {
            [JsonPropertyName("code")]
            public int Code { get; set; }
            [JsonPropertyName("message")]
            public string Message { get; set; }
        }
        public class DNSRecord
        {
            [JsonPropertyName("comment")]
            public string Comment { get; set; }
            [JsonPropertyName("name")]
            public string Name { get; set; }
            [JsonPropertyName("proxied")]
            public bool Proxied { get; set; }
            [JsonPropertyName("ttl")]
            public int TTL { get; set; }
            [JsonPropertyName("content")]
            public string Content { get; set; }

            [JsonPropertyName("type")]
            public string Type { get; set; }
        }

        public class Settings
        {
        }
        public class Meta
        {
            [JsonPropertyName("auto_added")]
            public bool AutoAdded { get; set; }
            [JsonPropertyName("managed_by_apps")]
            public bool ManagedByApps { get; set; }
            [JsonPropertyName("managed_by_argo_tunnel")]
            public bool ManagedByArgoTunnel { get; set; }
        }
        public class Tags
        {
        }

        // Cloudflare API 返回的 DNS 记录响应类
        public class DnsResponse
        {
            [JsonPropertyName("result")]
            public List<DNSResult> Result { get; set; }
            [JsonPropertyName("success")]
            public bool Success { get; set; }
            [JsonPropertyName("errors")]
            public List<Error> Errors { get; set; }
            [JsonPropertyName("messages")]
            public List<Messages> Messages { get; set; }
            [JsonPropertyName("result_info")]
            public ResultInfo ResultInfo { get; set; }
        }
        public class DnsAddResponse
        {
            [JsonPropertyName("result")]
            public AddResult Result { get; set; }
            [JsonPropertyName("success")]
            public bool Success { get; set; }
            [JsonPropertyName("errors")]
            public List<Error> Errors { get; set; }
            [JsonPropertyName("messages")]
            public List<Messages> Messages { get; set; }
        }
        public class DnsDeleteResponse
        {
            [JsonPropertyName("result")]
            public DNSResult Result { get; set; }
            [JsonPropertyName("success")]
            public bool Success { get; set; }
            [JsonPropertyName("errors")]
            public List<Error> Errors { get; set; }
            [JsonPropertyName("messages")]
            public List<Messages> Messages { get; set; }
        }

        [JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Default)]
        [JsonSerializable(typeof(DNSRecord))]
        public partial class DNSRecordJsonSerializerContext : JsonSerializerContext
        {
        }

        [JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Default)]
        [JsonSerializable(typeof(DnsResponse))]
        public partial class DnsResponseJsonSerializerContext : JsonSerializerContext
        {
        }

        [JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Default)]
        [JsonSerializable(typeof(DnsAddResponse))]
        public partial class DnsAddResponseJsonSerializerContext : JsonSerializerContext
        {
        }

        [JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Default)]
        [JsonSerializable(typeof(DnsDeleteResponse))]
        public partial class DnsDeleteResponseJsonSerializerContext : JsonSerializerContext
        {
        }
    }
}