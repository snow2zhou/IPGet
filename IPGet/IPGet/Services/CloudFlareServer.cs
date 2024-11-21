using IPGet.Models;
using IPGet.Options;
using Microsoft.Extensions.Options;
using Serilog;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IPGet.Services
{
    public class CloudFlareServer
    {
        private readonly IPGetOptions _ipGetOptions;
        private readonly DNSOptions _dnsOptions;
        private readonly CloudflareOptions _cloudflareOptions;
        public CloudFlareServer(IOptions<IPGetOptions> ipGetOptions, IOptions<DNSOptions> dnsOptions, IOptions<CloudflareOptions> cloudflareOptions)
        {
            _ipGetOptions = ipGetOptions.Value;
            _dnsOptions = dnsOptions.Value;
            _cloudflareOptions = cloudflareOptions.Value;
        }

        /// <summary>
        /// 更新DNS记录
        /// </summary>
        /// <returns></returns>
        public async Task UpdateDnsRecord()
        {
            try
            {
                // 获取 A 记录
                var existingRecord = await GetDnsRecord();
                if (existingRecord != null)
                {
                    Log.Information($"记录存在: 当前内容为 {existingRecord.Content}");
                    // 如果 A 记录存在，则修改记录
                    //await UpdateDnsRecordForCloudFlare(existingRecord.Id);

                    if (_dnsOptions.Type.Equals("A") && existingRecord.Content.Equals(_ipGetOptions.LastIPv4Address) || _dnsOptions.Type.Equals("AAAA") && existingRecord.Content.Equals(_ipGetOptions.LastIPv6Address))
                    {
                        Log.Information("记录内容无变化！");
                    }
                    else
                    {
                        Log.Information($"尝试删除{_dnsOptions.Type}记录。。。");
                        await DeleteDnsRecord(existingRecord.Id);
                        Log.Information($"暂停10秒，然后添加{_dnsOptions.Type}记录。。。");
                        System.Threading.Thread.Sleep(10000);
                        await AddDnsRecord();
                    }
                }
                else
                {
                    // 如果 A 记录不存在，则添加新的 A 记录
                    await AddDnsRecord();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"更新 DNS 记录出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取DNG记录
        /// </summary>
        /// <returns></returns>
        public async Task<Cloudflare.DNSResult> GetDnsRecord()
        {
            try
            {
                // 创建HttpClient实例
                using (var httpClient = new HttpClient())
                {
                    var url = $"https://api.cloudflare.com/client/v4/zones/{_cloudflareOptions.ZoneID}/dns_records?type={_dnsOptions.Type}&name={_dnsOptions.RecordName}";
                    var request = new HttpRequestMessage();
                    request.Method = HttpMethod.Get;
                    request.RequestUri = new Uri(url);
                    request.Headers.Add("X-Auth-Email", _cloudflareOptions.Email);

                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _cloudflareOptions.APIToken);

                    using (var response = await httpClient.SendAsync(request))
                    {
                        if (response.EnsureSuccessStatusCode().IsSuccessStatusCode)
                        {
                            var jsonResponse = await response.Content.ReadAsStringAsync();
                            Log.Debug(jsonResponse);
                            var cloudflareDnsResponseOptions = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, TypeInfoResolver = Cloudflare.DnsResponseJsonSerializerContext.Default };
                            var dnsRecords = JsonSerializer.Deserialize<Cloudflare.DnsResponse>(jsonResponse, cloudflareDnsResponseOptions);
                            // 返回第一个找到的 A 记录，若存在
                            return dnsRecords.Success && dnsRecords.Result.Count > 0 ? dnsRecords.Result[0] : null;
                        }

                        Log.Error($"获取 {_dnsOptions.Type} 记录失败！");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"获取 {_dnsOptions.Type} 记录失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 删除指定的DNS记录
        ///  https://developers.cloudflare.com/api/operations/dns-records-for-a-zone-delete-dns-record
        /// </summary>
        /// <param name="recordId">记录编号</param>
        /// <returns></returns>
        public async Task DeleteDnsRecord(string recordId)
        {
            try
            {
                // 创建HttpClient实例
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _cloudflareOptions.APIToken);
                    //httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var url = $"https://api.cloudflare.com/client/v4/zones/{_cloudflareOptions.ZoneID}/dns_records/{recordId}";

                    var request = new HttpRequestMessage();
                    request.Method = HttpMethod.Delete;
                    request.RequestUri = new Uri(url);
                    request.Headers.Add("X-Auth-Email", _cloudflareOptions.Email);

                    using (var response = await httpClient.SendAsync(request))
                    {
                        if (response.EnsureSuccessStatusCode().IsSuccessStatusCode)
                        {
                            var jsonResponse = await response.Content.ReadAsStringAsync();
                            Log.Debug(jsonResponse);
                            var cloudflareDnsDeleteResponseOptions = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, TypeInfoResolver = Cloudflare.DnsDeleteResponseJsonSerializerContext.Default };
                            var result = JsonSerializer.Deserialize<Cloudflare.DnsDeleteResponse>(jsonResponse, cloudflareDnsDeleteResponseOptions);
                            if (result.Success)
                            {
                                Log.Information($"成功删除 {_dnsOptions.Type} 记录: {result.Result.Id}");
                            }
                            else
                            {
                                Log.Error($"删除 {_dnsOptions.Type} 记录失败: {string.Join(", ", result.Errors.ToString())}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"删除 {_dnsOptions.Type} 记录出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 添加DNS记录
        /// </summary>
        /// <returns></returns>
        public async Task AddDnsRecord()
        {
            try
            {
                // 创建HttpClient实例
                using (HttpClient httpClient = new HttpClient())
                {
                    Cloudflare.DNSRecord dnsRecord = new Cloudflare.DNSRecord
                    {
                        Type = _dnsOptions.Type,
                        Name = _dnsOptions.RecordName,
                        Content = _dnsOptions.RecordContent,
                        TTL = _dnsOptions.TTL, // 设定 TTL，1 表示自动
                        Proxied = _dnsOptions.Proxied // 如果使用 Cloudflare 的代理，设定为 true
                    };

                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _cloudflareOptions.APIToken);

                    var dnsOptions = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, TypeInfoResolver = Cloudflare.DNSRecordJsonSerializerContext.Default };
                    var jsonContent = JsonSerializer.Serialize(dnsRecord, dnsOptions);
                    var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    var url = $"https://api.cloudflare.com/client/v4/zones/{_cloudflareOptions.ZoneID}/dns_records";

                    using (var response = await httpClient.PostAsync(url, httpContent))
                    {
                        if (response.EnsureSuccessStatusCode().IsSuccessStatusCode)
                        {
                            var jsonResponse = await response.Content.ReadAsStringAsync();
                            Log.Debug(jsonResponse);
                            var cloudflareDnsAddResponseOptions = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, TypeInfoResolver = Cloudflare.DnsAddResponseJsonSerializerContext.Default };
                            var result = JsonSerializer.Deserialize<Cloudflare.DnsAddResponse>(jsonResponse, cloudflareDnsAddResponseOptions);
                            if (result.Success)
                            {
                                Log.Information($"成功添加 {_dnsOptions.Type} 记录: {result.Result.Name} - {result.Result.Content} (ID: {result.Result.Id})");
                            }
                            else
                            {
                                Log.Error($"添加 {_dnsOptions.Type} 记录失败: {string.Join(", ", result.Errors.ToString())}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"添加 {_dnsOptions.Type} 记录出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新DNS记录
        /// 执行失败
        /// https://developers.cloudflare.com/api/operations/dns-records-for-a-zone-patch-dns-record
        /// </summary>
        /// <param name="recordId"></param>
        /// <returns></returns>
        public async Task UpdateDnsRecord(string recordId)
        {
            try
            {
                // 创建HttpClient实例
                using (var httpClient = new HttpClient())
                {
                    Cloudflare.DNSRecord dnsRecord = new Cloudflare.DNSRecord
                    {
                        Type = _dnsOptions.Type,
                        Name = _dnsOptions.RecordName,
                        Content = _dnsOptions.RecordContent,
                        TTL = _dnsOptions.TTL, // 设定 TTL，1 表示自动
                        Proxied = _dnsOptions.Proxied // 如果使用 Cloudflare 的代理，设定为 true
                    };

                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _cloudflareOptions.APIToken);

                    var dnsOptions = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, TypeInfoResolver = Cloudflare.DNSRecordJsonSerializerContext.Default };
                    var jsonContent = JsonSerializer.Serialize(dnsRecord, dnsOptions);
                    var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");


                    var url = $"https://api.cloudflare.com/client/v4/zones/{_cloudflareOptions.ZoneID}/dns_records/{recordId}";

                    var request = new HttpRequestMessage();
                    request.Method = HttpMethod.Patch;
                    request.RequestUri = new Uri(url);
                    request.Headers.Add("X-Auth-Email", _cloudflareOptions.Email);
                    request.Content = httpContent;

                    //        var request2 = new HttpRequestMessage
                    //        {
                    //            Method = HttpMethod.Put,
                    //            RequestUri = new Uri(url),
                    //            Headers =
                    //{
                    //    { "X-Auth-Email", CloudflareOptions.Email },
                    //},
                    //            Content = new StringContent("{\n  \"comment\": \"Domain verification record\",\n  \"name\": \"" + DNSOptions.RecordName + "\",\n  \"proxied\": false,\n  \"settings\": {},\n  \"tags\": [],\n  \"ttl\": 3600,\n  \"content\": \"" + DNSOptions.RecordContent + "\",\n  \"type\": \"" + DNSOptions.Type + "\"\n}")
                    //            {
                    //                Headers =
                    //    {
                    //        ContentType = new MediaTypeHeaderValue("application/json")
                    //    }
                    //            }
                    //        };

                    using (var response = await httpClient.SendAsync(request))
                    {
                        if (response.EnsureSuccessStatusCode().IsSuccessStatusCode)
                        {
                            var jsonResponse = await response.Content.ReadAsStringAsync();
                            Log.Debug(jsonResponse);
                            var cloudflareDnsResponseOptions = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, TypeInfoResolver = Cloudflare.DnsResponseJsonSerializerContext.Default };
                            var result = JsonSerializer.Deserialize<Cloudflare.DnsResponse>(jsonResponse, cloudflareDnsResponseOptions);
                            if (result.Success)
                            {
                                Log.Information($"成功更新 {_dnsOptions.Type} 记录: {result.Result[0].Name} (ID: {result.Result[0].Id})");
                            }
                            else
                            {
                                Log.Error($"更新 {_dnsOptions.Type} 记录失败: {string.Join(", ", result.Errors)}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"更新 {_dnsOptions.Type} 记录出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取所有DNS记录
        /// </summary>
        /// <returns></returns>
        public async Task<Cloudflare.DnsResponse> GetAllDnsRecord()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var url = $"https://api.cloudflare.com/client/v4/zones/{_cloudflareOptions.ZoneID}/dns_records";
                    var request = new HttpRequestMessage();
                    request.Method = HttpMethod.Get;
                    request.RequestUri = new Uri(url);
                    request.Headers.Add("X-Auth-Email", _cloudflareOptions.Email);

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _cloudflareOptions.APIToken);

                    //        var requests = new HttpRequestMessage
                    //        {
                    //            Method = HttpMethod.Get,
                    //            RequestUri = new Uri($"https://api.cloudflare.com/client/v4/zones/{CloudflareOptions.ZoneID}/dns_records"),
                    //            Headers =
                    //{
                    //    { "X-Auth-Email", CloudflareOptions.Email },
                    //},
                    //        };

                    using (var response = await client.SendAsync(request))
                    {
                        if (response.EnsureSuccessStatusCode().IsSuccessStatusCode)
                        {
                            var jsonResponse = await response.Content.ReadAsStringAsync();
                            Log.Debug(jsonResponse);
                            var cloudflareDnsResponseOptions = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, TypeInfoResolver = Cloudflare.DnsResponseJsonSerializerContext.Default };
                            var dnsRecords = JsonSerializer.Deserialize<Cloudflare.DnsResponse>(jsonResponse, cloudflareDnsResponseOptions);
                            return dnsRecords;
                        }

                        Log.Error($"获取 {_dnsOptions.Type} 记录失败！");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"获取 {_dnsOptions.Type} 记录失败: {ex.Message}");
                return null;
            }
        }
    }
}
