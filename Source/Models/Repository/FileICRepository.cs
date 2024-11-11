namespace SFManager.Source.Models.Repository
{


    public class FileICRepository
    {
        public string ObjectKey { get; set; }

        public string Content { get; set; }

        public string FileName { get; set; }

        public FileICRepository(string objectKey, string content, string fileName)
        {
            ObjectKey = objectKey;
            Content = content;
            FileName = fileName;
        }

        public FileICRepository() { }
    }
}
