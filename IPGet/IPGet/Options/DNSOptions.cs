using Microsoft.Extensions.Options;

namespace IPGet.Options
{
    public partial class DNSOptions : IOptions<DNSOptions>
    {
        DNSOptions IOptions<DNSOptions>.Value => this;

        /// <summary>
        /// 操作类型
        /// </summary>
        public string Type { get; set; } = "A";
        /// <summary>
        /// 需要替换的域名
        /// </summary>
        public string RecordName { get; set; } = "ip.example.com";
        /// <summary>
        /// 需要替换的A记录内容
        /// </summary>
        public string RecordContent { get; set; } = "192.168.0.1";
        /// <summary>
        /// 缓存记录时间（秒），1 表示自动
        /// </summary>
        public int TTL { get; set; } = 1;
        /// <summary>
        /// 是否启用代理功能
        /// </summary>
        public bool Proxied { get; set; } = false;
        /// <summary>
        /// DNS提供商
        /// 需要配置对应的供应商密钥
        /// </summary>
        public string DNSProvider { get; set; } = "Cloudflare";
    }
}