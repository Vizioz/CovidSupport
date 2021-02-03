
namespace CovidSupport.Api.Models
{
    public class ResourceListItem : IResourceItemBase
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Zip { get; set; }

        public string[] Region { get; set; }

        public string Category { get; set; }

        public string[] Options { get; set; }

        public double? Lat { get; set; }

        public double? Lon { get; set; }        
    }
}
