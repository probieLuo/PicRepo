using System.Text.Json.Serialization;

namespace PicRepo.Client.Models.ReqResp.Gitee;

/// <summary>
/// Gitee API 返回的文件内容对象
/// </summary>
public class FileContentResp
{
    /// <summary>
    /// 文件类型，例如 "file"
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// 编码方式，例如 "base64"
    /// </summary>
    [JsonPropertyName("encoding")]
    public string? Encoding { get; set; }

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    [JsonPropertyName("size")]
    public long? Size { get; set; }

    /// <summary>
    /// 文件名
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 文件路径
    /// </summary>
    [JsonPropertyName("path")]
    public string? Path { get; set; }

    /// <summary>
    /// 文件内容（Base64 编码）
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    /// <summary>
    /// 文件 SHA 值
    /// </summary>
    [JsonPropertyName("sha")]
    public string? Sha { get; set; }

    /// <summary>
    /// API 访问地址
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// 网页访问地址
    /// </summary>
    [JsonPropertyName("html_url")]
    public string? HtmlUrl { get; set; }

    /// <summary>
    /// 原始文件下载地址
    /// </summary>
    [JsonPropertyName("download_url")]
    public string? DownloadUrl { get; set; }

    /// <summary>
    /// 相关链接集合
    /// </summary>
    [JsonPropertyName("_links")]
    public ContentLinks? Links { get; set; }
}