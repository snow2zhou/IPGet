自动获得你的公网 IPv4 或 IPv6 地址，并解析到对应的域名服务，或发送到指定的邮箱。

## 特性

- 支持Mac、Windows、Linux系统，支持ARM、x86架构
- 支持的域名服务商 `Cloudflare`
- 默认间隔10分钟同步一次

## 使用方法

- 从 [Releases](https://github.com/snow2zhou/IPGet/releases) 下载并解压 IPGet

- 运行程序
  - 初次运行，会生成 appsettings.json 配置文件。
  - 配置好 appsettings.json 文件后，即可正常使用。（若未配置，则仅获取公网IPv4地址）。
  
- 配置说明（appsettings.json）
  - 配置邮件发送功能
    - 设置 IPGetOptions-> SendEmail 为 true
    - 配置 SmtpOptions 参数
      - [qq Smtp 设置](https://cloud.tencent.com/developer/article/2177098)
      - 参考示例 
```IPGetOptions
  "IPGetOptions": {
    "GetIPv4API": "https://ipinfo.io/ip", //获取公网IPv4的API接口 
    "GetIPv6API": "https://api6.ipify.org", //获取公网IPv6的API接口
    "IntervalTime": 10, //公网IP获取间隔时间（分钟）
    "LastIPv4Address": "", //最新的IPv4地址
    "LastIPv6Address": "", //最新的IPv6地址
    "SendEmail": true, //是否发送邮件
    "UpdateDNS": false //是否同步配置DNS信息
  },
```

``` qq
  "SmtpOptions": {
    "Host": "smtp.qq.com", //smtp服务器地址
    "Port": 465, //发送端口，一般为25 465 587
    "Timeout": 30000, //连接超时（毫秒）
    "EnableSsl": true, //true or false -- 是否启用ssl
    "UseDefaultCredentials": true, //true or false -- 是否使用默认凭据
    "SendEmail": "123456789@qq.com", //发件邮箱
    "AuthorizationCode": "这里填QQ号码对应的授权码", //授权码
    "EmailSender": "IP地址变更", //Email邮件发送名
    "Emails": [ //需要通知传达的邮箱
        "123456789@qq.com"
    ]
  },
```
[网易163 Smtp 设置](https://help.mail.163.com/faqDetail.do?code=d7a5dc8471cd0c0e8b4b8f4f8e49998b374173cfe9171305fa1ce630d7f67ac25ef2e192b234ae4d)
```163
  "SmtpOptions": {
    "Host": "smtp.163.com", //smtp服务器地址
    "Port": 465, //发送端口，一般为25 465 587
    "Timeout": 30000, //连接超时（毫秒）
    "EnableSsl": true, //true or false -- 是否启用ssl
    "UseDefaultCredentials": true, //true or false -- 是否使用默认凭据
    "SendEmail": "123456789@163.com", //发件邮箱
    "AuthorizationCode": "这里填163账号对应的授权码", //授权码
    "EmailSender": "IP地址变更", //Email邮件发送名
    "Emails": [ //需要通知传达的邮箱
        "123456789@163.com",
        "987654321@163.com"
    ]
  },
```
   
   - 配置DNS解析
    - 设置 IPGetOptions-> UpdateDNS 为 true
    - 配置 DNSOptions 参数
    - 配置 CloudflareOptions 参数[获取API 令牌](https://dash.cloudflare.com/profile/api-tokens)
    - 参考示例

```IPGetOptions
  "IPGetOptions": {
    "GetIPv4API": "https://ipinfo.io/ip", //获取公网IPv4的API接口 
    "GetIPv6API": "https://api6.ipify.org", //获取公网IPv6的API接口
    "IntervalTime": 10, //公网IP获取间隔时间（分钟）
    "LastIPv4Address": "", //最新的IPv4地址
    "LastIPv6Address": "", //最新的IPv6地址
    "SendEmail": false, //是否发送邮件
    "UpdateDNS": true //是否同步配置DNS信息
  },
```

```DNSOptions
  "DNSOptions": {
    "Type": "A", //操作类型：A-IPv4，AAAA-IPv6
    "RecordName": "ip.example.com", //需要替换的域名
    "RecordContent": null, //需要替换的记录内容（自动获取，无需手填）
    "TTL": 1, //缓存记录时间（秒），1 表示自动
    "Proxied": false, //是否启用代理功能
    "DNSProvider": "Cloudflare" //DNS提供商
  },
```

```CloudflareOptions
  "CloudflareOptions": {
    "Email": "",
    "APIToken": "",
    "ZoneID": "",
    "AccountID": ""
  }
```

- 修改公网 IPv4 获取接口
  - 设置 IPGetOptions-> GetIPv4API 的参数
- 修改公网 IPv6 获取接口
  - 设置 IPGetOptions-> GetIPv6API 的参数
- 修改公网IP获取间隔时间（分钟）
  - 设置 IPGetOptions-> IntervalTime 的参数

## 可获取公网IP的公益API接口

##### 获取公网IPv4
https://ipinfo.io/ip
https://ipinfo.io/ip
https://api64.ipify.org
https://ipv4.icanhazip.com
https://api.myexternalip.com/json
https://ip-api.com

##### 获取公网IPv6
https://api6.ipify.org
https://api6.ipify.org
https://api64.ipify.org/?format=json
https://ipv6.icanhazip.com
https://cloudflare.com/cdn-cgi/trace
https://ifconfig.me/ip
https://ipecho.net/plain
