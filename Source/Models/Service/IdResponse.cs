using Utils.Source.Models;

namespace SFManager.Source.Models.Service
{
    public class IdResponse
    {
        public string Id { get; set; }

        public string requestStatus { get; set; }

        public IdResponse() { }

        public IdResponse(string id, Status status)
        {
            Id = id;
            requestStatus = status.ToString();
        }
    }
}
