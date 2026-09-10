namespace PicRepo.Client.Models
{
    public class HisItem
    {
        public string FileName { get; set; }
        public ulong FileSize { get; set; }
        public string Url { get; set; }
        public DateTime UploadTime { get; set; }
    }
}