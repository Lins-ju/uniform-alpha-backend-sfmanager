using System.Security.Policy;
using Utils.Source.Models;

namespace SFManager.Source.Models.Controller
{
    public class IdResult
    {
        public string Id { get; set; }

        public Status Status { get; set; }

        public IdResult() { }

        public IdResult(string id, Status status)
        {
            Id = id;
            Status = status;
        }

        public IdResult(Status status)
        {
            Status = status;
        }
    }
}
