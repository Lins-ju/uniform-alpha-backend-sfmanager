namespace SFManager.Source.Models.Service
{
    public class CreateFolderRequest
    {
        public string UserId { get; set; }

        public string FolderName { get; set; }

        public CreateFolderRequest() { }

        public CreateFolderRequest(string userId, string folderName)
        {
            UserId = userId;
            FolderName = folderName;
        }

    }
}
