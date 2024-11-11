using SFManager.Source.Models.Controller;

namespace SFManager.Source.Models.Service
{
    public class FileICResponse
    {
        public string FileId { get; set; }

        public string Content { get; set; }

        public string FileName { get; set; }

        public FileICResponse() { }

        public FileICResponse(FileIC fileIc)
        {
            var splitObjectKey = fileIc.ObjectKey.Split('/');
            var fileId = splitObjectKey[2];

            FileId = fileId;
            Content = fileIc.Content;
            FileName = fileIc.FileName;
        }

    }
}
