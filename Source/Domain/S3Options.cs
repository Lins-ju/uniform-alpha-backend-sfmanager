namespace SFManager.Source.Domain
{
    public class S3Options
    {
        public const string Section = "S3Options";

        public string BucketName { get; set; }

        public string AWS_ACESS_KEY_ID { get; set; }

        public string AWS_SECRET_ACCESS_KEY { get; set; }

        public string AWS_REGION { get; set; }

        public string ServiceUrl { get; set; }

        public S3Options() { }
    }
}
