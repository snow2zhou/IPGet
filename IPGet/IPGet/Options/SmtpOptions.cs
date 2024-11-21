using Microsoft.Extensions.Options;

namespace IPGet.Options
{
    public partial class SmtpOptions : IOptions<SmtpOptions>
    {
        SmtpOptions IOptions<SmtpOptions>.Value => this;

        /// <summary>
        /// Smtp服务器
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// 发送端口
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// 连接超时
        /// </summary>
        public int Timeout { get; set; }
        /// <summary>
        /// 是否启用ssl
        /// </summary>
        public bool EnableSsl { get; set; }
        /// <summary>
        /// 是否使用默认凭据
        /// </summary>
        public bool UseDefaultCredentials { get; set; }
        /// <summary>
        /// 发件邮箱
        /// </summary>
        public string SendEmail { get; set; }
        /// <summary>
        /// 授权码
        /// </summary>
        public string AuthorizationCode { get; set; }

        /// <summary>
        /// Email邮件发送名
        /// </summary>
        public string EmailSender { get; set; } = "IP Address Changed";

        /// <summary>
        /// 需要通知传达的邮箱
        /// </summary>
        public string[] Emails { get; set; } = Array.Empty<string>();
    }    
}
