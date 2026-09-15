using PicRepo.Client.Helper;

namespace PicRepo.Client.Models
{
    public class HisItem
    {
        public PicRepoType PicRepoType { get; set; }
		public string Repo { get; set; }
		public string Branch { get; set; }
		public string FileName { get; set; }
        public ulong FileSize { get; set; }
        public string Url { get; set; }
        public DateTime UploadTime { get; set; }
    }
}