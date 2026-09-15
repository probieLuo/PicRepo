using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace PicRepo.Client.Models.ReqResp.Gitee
{
    internal class UpdateFileReq
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = "PicRepo upload file";

        [JsonPropertyName("branch")]
        public string Branch { get; set; }

        [JsonPropertyName("sha")]
        public string Sha { get; set; }
    }
}
