using System.Text.Json.Serialization;
using Utils.Source.Models;

namespace SFManager.Source.Models.Controller
{

    // (File Information and Content)
    public class FileIC
    {
        [JsonPropertyName("objectKey")]
        public string ObjectKey { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("fileName")]
        public string FileName { get; set; }

        [JsonIgnore]
        public Status Status { get; set; }

        public FileIC(string objectKey, string content, string fileName)
        {
            ObjectKey = objectKey;
            Content = content;
            FileName = fileName;
        }

        public FileIC(string objectKey, string content, string fileName, Status status)
        {
            ObjectKey = objectKey;
            Content = content;
            FileName = fileName;
            Status = status;
        }

        public FileIC(Status status)
        {
            Status = status;
        }

        public FileIC() { }
    }
}
