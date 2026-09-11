using NLog;
using System.IO;
using System.Net.Http;
using System.Text;

namespace PicRepo.Client.Services
{
    internal class GiteeApiService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly HttpClient client = new();

        public async Task<string> UploadFileAsync(string owner, string repo, string path,
            string localFilePath, string accessToken, string message = "PicRepo upload file", string branch = "master")
        {
            byte[] fileBytes = File.ReadAllBytes(localFilePath);
            string base64Content = Convert.ToBase64String(fileBytes);

            string url = $"https://gitee.com/api/v5/repos/{owner}/{repo}/contents/{path}";
            /*
            var formData = new Dictionary<string, string>
            {
                { "access_token", accessToken },
                { "content", base64Content },
                { "message", message },
                { "branch", branch }
            };
            
            var content = new FormUrlEncodedContent(formData);

            HttpResponseMessage response = await client.PostAsync(url, content);
            */
            var data = new Models.ReqResp.Gitee.UploadFileReq()
            {
                AccessToken = accessToken,
                Message = message,
                Branch = branch,
            };
            await data.SetContentAsync(localFilePath);
            var json = System.Text.Json.JsonSerializer.Serialize(data);
            using var content = new StringContent(json, Encoding.UTF8,"application/json");
            HttpResponseMessage response = await client.PostAsync(url, content);
            string responseBody = await response.Content.ReadAsStringAsync();
            var uploadFileResp = System.Text.Json.JsonSerializer.Deserialize<Models.ReqResp.Gitee.UploadFileResp>(responseBody);
            if (uploadFileResp == null)
                throw new Exception("文件上传失败，响应为空");
            if (uploadFileResp.Message == "文件名已存在" && response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                HttpResponseMessage putResponse = await client.PutAsync(url, content);
                string putResponseBody = await response.Content.ReadAsStringAsync();
                uploadFileResp = System.Text.Json.JsonSerializer.Deserialize<Models.ReqResp.Gitee.UploadFileResp>(putResponseBody);
            }
            return uploadFileResp.Content.DownloadUrl;
        }
    }
}