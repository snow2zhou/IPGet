using Microsoft.Extensions.Options;

namespace IPGet.Options
{
    public partial class CloudflareOptions : IOptions<CloudflareOptions>
    {
        CloudflareOptions IOptions<CloudflareOptions>.Value => this;

        /// <summary>
        /// 电子邮箱
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// API令牌
        /// </summary>
        public string APIToken { get; set; }
        /// <summary>
        /// 区域ID
        /// </summary>
        public string ZoneID { get; set; }
        /// <summary>
        /// 账户ID
        /// </summary>
        public string AccountID { get; set; }
    }
}
