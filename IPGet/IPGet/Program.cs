using IPGet.Models;
using IPGet.Options;
using IPGet.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using System.Reflection;
using System.Text.RegularExpressions;

#region 配置 Serilog 日志
//安装 Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Console()
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Verbose)
        .WriteTo.Async(a => a.File("logs/verboses/verboses-.log", rollingInterval: RollingInterval.Day))
    )
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Debug)
        .WriteTo.Async(a => a.File("logs/debugs/debugs-.log", rollingInterval: RollingInterval.Day))
    )
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information)
        .WriteTo.Async(a => a.File("logs/infos/infos-.log", rollingInterval: RollingInterval.Day))
    )
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning)
        .WriteTo.Async(a => a.File("logs/warnings/warnings-.log", rollingInterval: RollingInterval.Day))
    )
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Error)
        .WriteTo.Async(a => a.File("logs/errors/errors-.log", rollingInterval: RollingInterval.Day))
    )
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Fatal)
        .WriteTo.Async(a => a.File("logs/fatal/fatal-.log", rollingInterval: RollingInterval.Month))
    )
    .CreateLogger();

#endregion

// 定义要输出的配置文件路径
string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
if (!File.Exists(outputPath))
{
    Log.Warning("appsettings.json 文件不存在，尝试生成。。。");
    // 从嵌入资源中读取内容
    using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("IPGet.appsettings.json"))
    {
        if (stream != null)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                string content = reader.ReadToEnd();

                // 将内容写入到执行目录的文件中
                File.WriteAllText(outputPath, content);
                Log.Information($"appsettings.json 已成功复制到: {outputPath}");
                Log.Information("请配置 appsettings.json 文件信息，然后重新运行本程序！");
                Log.Information("按任意键退出。。。");
                while (true)
                {
                    // 检查用户输入
                    if (Console.KeyAvailable) // 如果有键被按下
                    {
                        Environment.Exit(0); // 直接退出程序
                    }

                    await Task.Delay(500); // 等待一小段时间，避免 CPU 占用过高
                }
            }
        }
        else
        {
            Log.Error("无法找到 appsettings.json 嵌入资源文件。");
        }
    }
}

IPGetOptions IPGetOptions = new IPGetOptions();
SmtpOptions SmtpOptions = new SmtpOptions();
DNSOptions DNSOptions = new DNSOptions();
CloudflareOptions CloudflareOptions = new CloudflareOptions();

var confBuilder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: true)
    .Build();

confBuilder.GetSection("IPGetOptions").Bind(IPGetOptions);
confBuilder.GetSection("SmtpOptions").Bind(SmtpOptions);
confBuilder.GetSection("DNSOptions").Bind(DNSOptions);
confBuilder.GetSection("CloudflareOptions").Bind(CloudflareOptions);

EmailSender emailSender = new EmailSender(SmtpOptions);
CloudFlareServer cloudFlareServer = new CloudFlareServer(IPGetOptions, DNSOptions, CloudflareOptions);

bool finish = false;

//判断获取IP地址类型
if (DNSOptions.Type.Equals("A"))
{
    // 启动一个 Task 来执行异步函数
    var GetIPv4Task = Task.Run(async () => await GetIPv4AddressTask());
    await ListenForInput();
    await GetIPv4Task;
}
else
{
    // 启动一个 Task 来执行异步函数
    var GetIPv6Task = Task.Run(async () => await GetIPv6AddressTask());
    await ListenForInput();
    await GetIPv6Task;
}

//获取IPv4地址Task
async Task GetIPv4AddressTask()
{
    while (true)
    {
        finish = false;
        await GetIPv4Address();
        finish = true;
        Log.Warning("-----------------------程序执行完毕-----------------------");
        await Task.Delay(60000 * IPGetOptions.IntervalTime); // 异步的等待
    }
}

//获取IPv6地址Task
async Task GetIPv6AddressTask()
{
    while (true)
    {
        finish = false;
        await GetIPv6Address();
        finish = true;
        Log.Warning("-----------------------程序执行完毕-----------------------");
        await Task.Delay(60000 * IPGetOptions.IntervalTime); // 异步的等待
    }
}

//监听输入
async Task ListenForInput()
{
    Log.Information("-----------------------输入 'q' 退出程序-----------------------");
    bool running = true;

    // 使用循环来等待用户输入
    while (running)
    {
        // 检查用户输入
        if (Console.KeyAvailable) // 如果有键被按下
        {
            var key = Console.ReadKey(true); // 读取按键，不在控制台显示
            if (key.KeyChar == 'q' || key.KeyChar == 'Q') // 检查是否为'q'或'Q'
            {
                running = false; // 设置标志以退出循环
            }
        }

        await Task.Delay(500); // 等待一小段时间，避免 CPU 占用过高
    }

    Log.Warning("-----------------------已检测到退出信号，准备退出-----------------------");

    await Exit();
}

//退出程序
async Task Exit()
{
    while (true)
    {
        if (finish)
        {
            Log.Warning("-----------------------程序已退出-----------------------");
            Environment.Exit(0); // 直接退出程序
        }
        await Task.Delay(100); // 等待一小段时间，避免 CPU 占用过高
    }
}


//获取IPv4地址
async Task GetIPv4Address()
{
    try
    {
        var httpClient = new HttpClient();
        // 请求公网 IPv4 地址
        string ipv4Address = await httpClient.GetStringAsync(IPGetOptions.GetIPv4API);
        DateTime lastDate = DateTime.Now;

        // 定义 IPv4 地址的正则表达式
        string pattern = @"\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b";
        // 使用正则表达式进行匹配
        Match match = Regex.Match(ipv4Address, pattern);
        if (match.Success)
        {
            ipv4Address = match.Value;

            if (string.IsNullOrEmpty(IPGetOptions.LastIPv4Address) || !IPGetOptions.LastIPv4Address.Equals(ipv4Address))
            {
                IPGetOptions.LastIPv4Address = ipv4Address;

                Log.Information($"当前公网 IPv4 地址：{ipv4Address}  |  获取时间：{lastDate.ToString("yyyy-MM-dd HH:mm:ss")}");

                if (IPGetOptions.SendEmail)
                {
                    foreach (string email in SmtpOptions.Emails)
                    {
                        await emailSender.SendEmailAsync(email, "公网IPv4变动", "检测时间：" + lastDate.ToString("yyyy-MM-dd HH:mm:ss") + "\n检测IP：" + ipv4Address);
                        Log.Fatal("公网IPv4地址变动邮件（" + email + "）发送成功！");
                    }
                }
                if (IPGetOptions.UpdateDNS)
                {
                    DNSOptions.RecordContent = IPGetOptions.LastIPv4Address;

                    if (DNSOptions.DNSProvider.Equals(DomainNameServiceProvider.Cloudflare))
                    {
                        await cloudFlareServer.UpdateDnsRecord();
                    }
                    else
                    {
                        Log.Error("公网IPv4地址DNS更新失败：未匹配到合适的DNS供应商-" + DNSOptions.DNSProvider);
                    }
                }
            }
            else
            {
                Log.Information("公网IPv4地址无变化：" + IPGetOptions.LastIPv4Address);
            }
        }
        else
        {
            Log.Error("公网IPv4地址获取失败：未匹配到IPv4地址：" + ipv4Address);
        }
    }
    catch (Exception ex)
    {
        Log.Error($"发生出错: {ex.Message}");
    }
}

//获取IPv6地址
async Task GetIPv6Address()
{
    try
    {
        var httpClient = new HttpClient();
        // 请求公网 IPv6 地址
        string ipv6Address = await httpClient.GetStringAsync(IPGetOptions.GetIPv6API);
        DateTime lastDate = DateTime.Now;
        // 定义 IPv6 地址的正则表达式
        // 定义IPv6地址的正则表达式
        string pattern = @"(\[?(?:[0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}\]?|" +
                         @"\[?(?:[0-9a-fA-F]{1,4}:){1,7}:]?|" +
                         @"\[?(?:[0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}\]?|" +
                         @"\[?(?:[0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}\]?|" +
                         @"\[?(?:[0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}\]?|" +
                         @"\[?(?:[0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}\]?|" +
                         @"\[?(?:[0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}\]?|" +
                         @"\[?[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})\]?|" +
                         @"::([0-9a-fA-F]{1,4}:){0,6}[0-9a-fA-F]{1,4}\]?|" +
                         @"([0-9a-fA-F]{1,4}:){1,7}:)";
        // 使用正则表达式进行匹配
        Match match = Regex.Match(ipv6Address, pattern);
        if (match.Success)
        {
            ipv6Address = match.Value;

            if (string.IsNullOrEmpty(IPGetOptions.LastIPv6Address) || !IPGetOptions.LastIPv6Address.Equals(ipv6Address))
            {
                IPGetOptions.LastIPv6Address = ipv6Address;

                Log.Information($"当前公网 IPv6 地址：{ipv6Address}  |  获取时间：{lastDate.ToString("yyyy-MM-dd HH:mm:ss")}");


                if (IPGetOptions.SendEmail)
                {
                    foreach (string email in SmtpOptions.Emails)
                    {
                        await emailSender.SendEmailAsync(email, "公网IPv6变动", "检测时间：" + lastDate.ToString("yyyy-MM-dd HH:mm:ss") + "\n检测IP：" + ipv6Address);
                        Log.Fatal("公网IPv6地址变动邮件（" + email + "）发送成功！");
                    }
                }

                if (IPGetOptions.UpdateDNS)
                {
                    DNSOptions.RecordContent = IPGetOptions.LastIPv6Address;

                    if (DNSOptions.DNSProvider.Equals(DomainNameServiceProvider.Cloudflare))
                    {
                        await cloudFlareServer.UpdateDnsRecord();
                    }
                    else
                    {
                        Log.Error("公网IPv4地址DNS更新失败：未匹配到合适的DNS供应商-" + DNSOptions.DNSProvider);
                    }
                }
            }
            else
            {
                Log.Information("公网IPv6地址无变化：" + IPGetOptions.LastIPv6Address);
            }
        }
        else
        {
            Log.Error("公网IPv6地址获取失败：未匹配到IPv6地址：" + ipv6Address);
        }
    }
    catch (Exception ex)
    {
        Log.Error($"发生出错: {ex.Message}");
    }
}
