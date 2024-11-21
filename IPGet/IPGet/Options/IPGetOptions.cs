using Microsoft.Extensions.Options;

namespace IPGet.Options
{
    public partial class IPGetOptions : IOptions<IPGetOptions>
    {
        IPGetOptions IOptions<IPGetOptions>.Value => this;

        /// <summary>
        /// 获取公网IPv4的API接口
        /// </summary>
        public string GetIPv4API { get; set; } = "https://ipinfo.io/ip";
        /// <summary>
        /// 获取公网IPv6的API接口
        /// </summary>
        public string GetIPv6API { get; set; } = "https://api6.ipify.org";

        /// <summary>
        /// 公网IP获取间隔时间（分钟）
        /// </summary>
        public int IntervalTime { get; set; } = 10;

        /// <summary>
        /// 最新的IPv4地址
        /// </summary>
        public string? LastIPv4Address { get; set; }
        /// <summary>
        /// 最新的IPv6地址
        /// </summary>
        public string? LastIPv6Address { get; set; }

        /// <summary>
        /// 是否发送邮件
        /// 需要配置 SmtpOptions
        /// </summary>
        public bool SendEmail { get; set; } = false;

        /// <summary>
        /// 是否同步配置DNS信息
        /// 需要配置 DNSOptions
        /// </summary>
        public bool UpdateDNS { get; set; } = false;
    }
}