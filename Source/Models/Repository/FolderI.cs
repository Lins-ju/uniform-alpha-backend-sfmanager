using Newtonsoft.Json;
using Utils.Source.Models;

namespace SFManager.Source.Models.Repository
{
    public class FolderI
    {
        [JsonProperty(PropertyName = "id")]
        public string FolderId { get; set; }

        public string UserId { get; set; }

        public string FolderName { get; set; }

        [JsonIgnore]
        public Status Status { get; set; }

        public FolderI() { }

        public FolderI(string folderId, string userId, string folderName)
        {
            UserId = userId;
            FolderId = folderId;
            FolderName = folderName;
        }
    }
}
