using NLog;
using Octokit;
using PicRepo.Client.Data;
using PicRepo.Client.Helper;
using PicRepo.Client.Models;
using System.IO;
using System.Net.Http;
using System.Text;

namespace PicRepo.Client.Services
{
    internal class PicRepoService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly HttpClient _httpClient;

        public PicRepoService()
        {
            _httpClient = new();
        }

        public async Task<string> UploadFileGiteeAsync(string owner, string repo, byte[] bytes,
            string pathInRepo, string accessToken, string message = "PicRepo upload file", string branch = "master")
        {
            string base64Content = Convert.ToBase64String(bytes);

            string url = $"https://gitee.com/api/v5/repos/{owner}/{repo}/contents/{pathInRepo}";
            var data = new Models.ReqResp.Gitee.UploadFileReq()
            {
                AccessToken = accessToken,
                Message = message,
                Branch = branch,
                Content = base64Content
            };
            var json = System.Text.Json.JsonSerializer.Serialize(data);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(url, content);
            string responseBody = await response.Content.ReadAsStringAsync();
            var uploadFileResp = System.Text.Json.JsonSerializer.Deserialize<Models.ReqResp.Gitee.UploadFileResp>(responseBody);
            if (uploadFileResp == null)
                throw new Exception("文件上传失败，响应为空");
            if (uploadFileResp.Message == "文件名已存在" && response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                HttpResponseMessage putResponse = await _httpClient.PutAsync(url, content);
                string putResponseBody = await response.Content.ReadAsStringAsync();
                uploadFileResp = System.Text.Json.JsonSerializer.Deserialize<Models.ReqResp.Gitee.UploadFileResp>(putResponseBody);
            }
            return uploadFileResp.Content.DownloadUrl;
        }

        /// <summary>
        /// 将图片（字节数组）上传到指定仓库的指定路径。如果文件已存在则更新，否则创建新文件。
        /// 返回可直接访问的 raw.githubusercontent 链接（基于 branch）。
        /// </summary>
        public async Task<string> UploadFileGithubAsync(string owner, string repo, string accessToken, string productHeader, byte[] imageBytes, string pathInRepo, string commitMessage, string branch = "main")
        {
            if (imageBytes == null || imageBytes.Length == 0) throw new ArgumentException("imageBytes is empty", nameof(imageBytes));
            if (string.IsNullOrWhiteSpace(pathInRepo)) throw new ArgumentException("pathInRepo required", nameof(pathInRepo));
            if (string.IsNullOrWhiteSpace(commitMessage)) commitMessage = "Add image";
            if (string.IsNullOrWhiteSpace(branch)) branch = "main";

            // GitHub Contents API 要求 content 为 base64 编码的字符串
            var base64Content = Convert.ToBase64String(imageBytes);
            GitHubClient client = new GitHubClient(new ProductHeaderValue(productHeader))
            {
                Credentials = new Credentials(accessToken)
            };
            try
            {
                RepositoryContent? existing = null;
                try
                {
                    var contents = await client.Repository.Content.GetAllContentsByRef(owner, repo, pathInRepo, branch).ConfigureAwait(false);
                    if (contents != null && contents.Count > 0)
                        existing = contents[0];
                }
                catch (NotFoundException)
                {
                    // 文件不存在，继续走创建逻辑
                }

                if (existing != null)
                {
                    var updateRequest = new UpdateFileRequest(commitMessage, base64Content, existing.Sha, branch, convertContentToBase64: false);
                    var updateResult = await client.Repository.Content.UpdateFile(owner, repo, pathInRepo, updateRequest).ConfigureAwait(false);
                }
                else
                {
                    var createRequest = new CreateFileRequest(commitMessage, base64Content, branch, convertContentToBase64: false);
                    var createResult = await client.Repository.Content.CreateFile(owner, repo, pathInRepo, createRequest).ConfigureAwait(false);
                }

                var rawUrl = $"https://raw.githubusercontent.com/{owner}/{repo}/{branch}/{Uri.EscapeDataString(pathInRepo)}";
                return rawUrl;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"上传图片到 GitHub 失败: {ex.Message}", ex);
            }
        }

        public async Task<(string url, UploadHistory? hisItem)> UploadAsync(IPicRepoConfig config, string filePath, string message = "PicRepo upload file")
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("filePath required", nameof(filePath));
            byte[] bytes = await System.IO.File.ReadAllBytesAsync(filePath);
            switch (config.PicRepoType)
            {
                case PicRepoType.GitHub:
                    if (config is GitHubPicRepoConfig gitHubConfig)
                    {
                        var token = PicRepo.Client.Helper.EncryptionHelper.DecryptString(gitHubConfig.Token);
                        var url = await UploadFileGithubAsync(gitHubConfig.Owner, gitHubConfig.Repo, token, gitHubConfig.ProductHeader, bytes, Path.GetFileName(filePath), message, gitHubConfig.Branch);
                        UploadHistory? hisModel = null;
                        try
                        {
                            using var db = new AppDbContext();
                            hisModel = new Models.UploadHistory
                            {
                                FileName = System.IO.Path.GetFileName(filePath),
                                FullFileName = filePath,
                                Repo = gitHubConfig.Repo,
                                CommitMessage = message,
                                Branch = gitHubConfig.Branch,
                                UploadTime = DateTime.UtcNow.ToLocalTime(),
                                Url = url,
                                FileSize = (ulong)bytes.Length,
                            };
                            db.UploadHistorys.Add(hisModel);
                            await db.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            // 记录数据库保存失败的异常，但不影响上传结果
                            _logger.Warn(ex, "Failed to save upload history to database.");
                        }
                        return (url, hisModel);
                    }
                    break;

                case PicRepoType.Gitee:
                    if (config is GiteePicRepoConfig giteeConfig)
                    {
                        var token = PicRepo.Client.Helper.EncryptionHelper.DecryptString(giteeConfig.Token);
                        var url = await UploadFileGiteeAsync(giteeConfig.Owner, giteeConfig.Repo, bytes, Path.GetFileName(filePath), token, message, giteeConfig.Branch);
                        UploadHistory? hisModel = null;
                        try
                        {
                            using var db = new AppDbContext();
                            hisModel = new Models.UploadHistory
                            {
                                FileName = System.IO.Path.GetFileName(filePath),
                                FullFileName = filePath,
                                Repo = giteeConfig.Repo,
                                CommitMessage = message,
                                Branch = giteeConfig.Branch,
                                UploadTime = DateTime.UtcNow.ToLocalTime(),
                                Url = url,
                                FileSize = (ulong)bytes.Length,
                            };
                            db.UploadHistorys.Add(hisModel);
                            await db.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            // 记录数据库保存失败的异常，但不影响上传结果
                            _logger.Warn(ex, "Failed to save upload history to database.");
                        }
                        return (url, hisModel);
                    }
                    break;
            }
            return ("", null);
        }
    }
}