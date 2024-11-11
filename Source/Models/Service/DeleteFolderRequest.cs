namespace SFManager.Source.Models.Service
{
    public class DeleteFolderRequest
    {
        public string UserId { get; set; }

        public string FolderId { get; set; }

        public DeleteFolderRequest() { }

        public DeleteFolderRequest(string userId, string folderId)
        {
            UserId = userId;
            FolderId = folderId;
        }
    }
}
