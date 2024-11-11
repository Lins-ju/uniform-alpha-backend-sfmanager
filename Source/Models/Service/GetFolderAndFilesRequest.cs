namespace SFManager.Source.Models.Service
{
    public class GetFolderAndFilesRequest
    {
        public string UserId { get; set; }

        public GetFolderAndFilesRequest() { }

        public GetFolderAndFilesRequest(string userId)
        {
            UserId = userId;
        }
    }
}
