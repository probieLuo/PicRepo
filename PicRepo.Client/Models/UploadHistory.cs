namespace PicRepo.Client.Models
{
    public class UploadHistory : IEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string FileName { get; set; }
        public string FullFileName { get; set; }
        public ulong FileSize { get; set; }
        public string Url { get; set; }
        public string CommitMessage { get; set; }
        public string Repo { get; set; }
        public string Branch { get; set; }
        public DateTime UploadTime { get; set; } = DateTime.Now;
    }
}