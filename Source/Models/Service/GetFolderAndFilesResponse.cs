using SFManager.Source.Models.Controller;

namespace SFManager.Source.Models.Service
{
    public class GetFolderAndFilesResponse
    {
        public Dictionary<string, List<FileICResponse>> FolderAndFiles = new Dictionary<string, List<FileICResponse>>();

        public GetFolderAndFilesResponse() { }

        public GetFolderAndFilesResponse(Dictionary<string, List<FileIC>> folderAndFiles)
        {
            if (folderAndFiles.Count > 0)
            {
                foreach (var item in folderAndFiles)
                {
                    List<FileICResponse> fileICResponses = new List<FileICResponse>();

                    foreach(var fileIC in item.Value)
                    {
                        var convertToFileICResponse = new FileICResponse(fileIC);
                        fileICResponses.Add(convertToFileICResponse);
                    }

                    FolderAndFiles.Add(item.Key, fileICResponses);
                }
            }
            if (folderAndFiles.Count == 0)
            {
                foreach (var item in folderAndFiles)
                {
                    FolderAndFiles.Add(item.Key, new List<FileICResponse>());
                }
            }
        }
    }
}
