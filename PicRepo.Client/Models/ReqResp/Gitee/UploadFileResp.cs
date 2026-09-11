using System.Text.Json.Serialization;

namespace PicRepo.Client.Models.ReqResp.Gitee
{
    /// <summary>
    /// Gitee 新建/更新文件接口的响应模型
    /// </summary>
    public class UploadFileResp
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("content")]
        public Content? Content { get; set; }

        [JsonPropertyName("commit")]
        public Commit? Commit { get; set; }
    }

    /// <summary>
    /// 文件内容信息
    /// </summary>
    public class Content
    {
        /// <summary>
        /// 文件名
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// 文件路径
        /// </summary>
        [JsonPropertyName("path")]
        public string Path { get; set; }

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        [JsonPropertyName("size")]
        public long Size { get; set; }

        /// <summary>
        /// 文件内容的 SHA 值
        /// </summary>
        [JsonPropertyName("sha")]
        public string Sha { get; set; }

        /// <summary>
        /// 文件类型
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        /// API 访问地址
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; }

        /// <summary>
        /// Gitee 网页地址
        /// </summary>
        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; }

        /// <summary>
        /// 原始文件下载地址
        /// </summary>
        [JsonPropertyName("download_url")]
        public string DownloadUrl { get; set; }

        /// <summary>
        /// 相关链接
        /// </summary>
        [JsonPropertyName("_links")]
        public ContentLinks Links { get; set; }
    }

    /// <summary>
    /// 文件内容相关链接
    /// </summary>
    public class ContentLinks
    {
        /// <summary>
        /// API 自身链接
        /// </summary>
        [JsonPropertyName("self")]
        public string Self { get; set; }

        /// <summary>
        /// HTML 页面链接
        /// </summary>
        [JsonPropertyName("html")]
        public string Html { get; set; }
    }

    /// <summary>
    /// 提交信息
    /// </summary>
    public class Commit
    {
        /// <summary>
        /// 提交的 SHA 值
        /// </summary>
        [JsonPropertyName("sha")]
        public string Sha { get; set; }

        /// <summary>
        /// 作者信息
        /// </summary>
        [JsonPropertyName("author")]
        public CommitUser Author { get; set; }

        /// <summary>
        /// 提交者信息
        /// </summary>
        [JsonPropertyName("committer")]
        public CommitUser Committer { get; set; }

        /// <summary>
        /// 提交信息
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; }

        /// <summary>
        /// 树对象
        /// </summary>
        [JsonPropertyName("tree")]
        public Tree Tree { get; set; }

        /// <summary>
        /// 父提交列表
        /// </summary>
        [JsonPropertyName("parents")]
        public List<Parent> Parents { get; set; }
    }

    /// <summary>
    /// 提交作者/提交者信息
    /// </summary>
    public class CommitUser
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        /// <summary>
        /// 提交时间，ISO 8601 格式，如 "2026-09-11T06:28:58+00:00"
        /// 建议用 DateTimeOffset 保留时区信息
        /// </summary>
        [JsonPropertyName("date")]
        public DateTimeOffset Date { get; set; }
    }

    /// <summary>
    /// Git 树对象
    /// </summary>
    public class Tree
    {
        [JsonPropertyName("sha")]
        public string Sha { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }

    /// <summary>
    /// 父提交信息
    /// </summary>
    public class Parent
    {
        [JsonPropertyName("sha")]
        public string Sha { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}