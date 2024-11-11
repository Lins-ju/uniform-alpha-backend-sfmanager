namespace SFManager.Source.Models.Service
{
    public class DeleteFileRequest
    {
        public string UserId { get; set; }

        public string FolderId { get; set; }

        public string FileId { get; set; }

        public DeleteFileRequest() { }

        public DeleteFileRequest(string userId, string folderId, string fileId)
        {
            UserId = userId;
            FolderId = folderId;
            FileId = fileId;
        }
    }
}
