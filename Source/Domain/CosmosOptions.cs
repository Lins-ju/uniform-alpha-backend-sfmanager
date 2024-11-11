namespace SFManager.Source.Domain
{
    public class CosmosOptions
    {
        public const string Section = "AzureOptions";

        public string ConnectionString { get; set; }

        public string DatabaseName { get; set; }

        public string ContainerName { get; set; }

        public CosmosOptions() { }
    }
}