using System.Text.Json.Serialization;

namespace PicRepo.Client.Models.ReqResp.Gitee;

public class UploadFileReq
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = "PicRepo upload file";

    [JsonPropertyName("branch")]
    public string Branch { get; set; }

    public async Task SetContentAsync(string localFilePath)
    {
        if (string.IsNullOrWhiteSpace(localFilePath)) throw new ArgumentException("localFilePath required", nameof(localFilePath));
        var bytes = await System.IO.File.ReadAllBytesAsync(localFilePath);
        Content = Convert.ToBase64String(bytes);
    }
}
