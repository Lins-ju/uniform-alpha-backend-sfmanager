using Utils.Source.Models;

namespace SFManager.Source.Models.Service
{
    public class StatusResponse
    {
        public string RequestStatus { get; set; }

        public StatusResponse(Status status)
        {
            RequestStatus = status.ToString();
        }
    }
}
