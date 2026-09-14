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
}
