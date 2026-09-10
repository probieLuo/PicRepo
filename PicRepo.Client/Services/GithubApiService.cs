using NLog;
using Octokit;
using PicRepo.Client.Data;
using PicRepo.Client.Models;
using System.ComponentModel;

namespace PicRepo.Client.Services
{
    internal class GithubApiService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly GitHubClient _client;
        private readonly string _owner;
        private readonly string _repo;

        public GithubApiService(string productHeader, string token, string owner, string repo)
        {
            if (string.IsNullOrWhiteSpace(productHeader)) throw new ArgumentException("productHeader required", nameof(productHeader));
            if (string.IsNullOrWhiteSpace(token)) throw new ArgumentException("token required", nameof(token));
            if (string.IsNullOrWhiteSpace(owner)) throw new ArgumentException("owner required", nameof(owner));
            if (string.IsNullOrWhiteSpace(repo)) throw new ArgumentException("repo required", nameof(repo));

            _owner = owner;
            _repo = repo;
            _client = new GitHubClient(new ProductHeaderValue(productHeader))
            {
                Credentials = new Credentials(token)
            };
        }

        /// <summary>
        /// 将图片（字节数组）上传到指定仓库的指定路径。如果文件已存在则更新，否则创建新文件。
        /// 返回可直接访问的 raw.githubusercontent 链接（基于 branch）。
        /// </summary>
        public async Task<string> UploadImageAsync(byte[] imageBytes, string pathInRepo, string commitMessage, string branch = "main")
        {
            if (imageBytes == null || imageBytes.Length == 0) throw new ArgumentException("imageBytes is empty", nameof(imageBytes));
            if (string.IsNullOrWhiteSpace(pathInRepo)) throw new ArgumentException("pathInRepo required", nameof(pathInRepo));
            if (string.IsNullOrWhiteSpace(commitMessage)) commitMessage = "Add image";
            if (string.IsNullOrWhiteSpace(branch)) branch = "main";

            // GitHub Contents API 要求 content 为 base64 编码的字符串
            var base64Content = Convert.ToBase64String(imageBytes);

            try
            {
                RepositoryContent? existing = null;
                try
                {
                    var contents = await _client.Repository.Content.GetAllContentsByRef(_owner, _repo, pathInRepo, branch).ConfigureAwait(false);
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
                    var updateResult = await _client.Repository.Content.UpdateFile(_owner, _repo, pathInRepo, updateRequest).ConfigureAwait(false);
                }
                else
                {
                    var createRequest = new CreateFileRequest(commitMessage, base64Content, branch, convertContentToBase64: false);
                    var createResult = await _client.Repository.Content.CreateFile(_owner, _repo, pathInRepo, createRequest).ConfigureAwait(false);
                }

                var rawUrl = $"https://raw.githubusercontent.com/{_owner}/{_repo}/{branch}/{Uri.EscapeDataString(pathInRepo)}";
                return rawUrl;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("上传图片到 GitHub 失败", ex);
            }
        }

        /// <summary>
        /// Convenience overload: 从本地文件路径读取并上传
        /// </summary>
        public async Task<(string url, UploadHistory? hisItem)> UploadImageFromFileAsync(string localFilePath, string pathInRepo, string commitMessage, string branch = "main")
        {
            if (string.IsNullOrWhiteSpace(localFilePath)) throw new ArgumentException("localFilePath required", nameof(localFilePath));
            var bytes = await System.IO.File.ReadAllBytesAsync(localFilePath).ConfigureAwait(false);
            var url = await UploadImageAsync(bytes, pathInRepo, commitMessage, branch).ConfigureAwait(false);
            UploadHistory? hisModel = null;
            try
            {
                using var db = new AppDbContext();
                hisModel = new Models.UploadHistory
                {
                    FileName = System.IO.Path.GetFileName(localFilePath),
                    FullFileName = localFilePath,
                    Repo = _repo,
                    CommitMessage = commitMessage,
                    Branch = branch,
                    UploadTime = DateTime.UtcNow.ToLocalTime(),
                    Url = url,
                    FileSize = (ulong)bytes.Length,
                };
                db.UploadHistorys.Add(hisModel);
                await db.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // 记录数据库保存失败的异常，但不影响上传结果
                _logger.Warn(ex, "Failed to save upload history to database.");
            }
            return (url, hisModel);
        }
    }
}