using CovidSupport.Api.Interfaces;

namespace CovidSupport.Api.Models
{
    public class ResourceListItem : IResourceItem
    {
        public ResourceListItem()
        {
            this.ServiceRegions = new string[] { };
        }

        public int Id { get; set; }

        public string ProviderName { get; set; }

        public string ServiceName { get; set; }

        public string ShortDescription { get; set; }

        public string[] ServiceRegions { get; set; }

        public string StreetAddress { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Zip { get; set; }

        public string[] Tags { get; set; }

        public double? Lat { get; set; }

        public double? Lon { get; set; }
    }
}
