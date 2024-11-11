namespace SFManager.Source.Models.Service
{
    public class SaveFileRequest
    {
        public string UserId { get; set; }

        public string FolderId { get; set; }

        public string FileContent { get; set; }

        public string FileName { get; set; }

        public SaveFileRequest() { }

        public SaveFileRequest(string userId, string folderId, string fileContent, string fileName)
        {
            UserId = userId;
            FolderId = folderId;
            FileContent = fileContent;
            FileName = fileName;
        }
    }
}
