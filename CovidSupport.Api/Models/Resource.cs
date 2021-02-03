using System.Collections.Generic;

namespace CovidSupport.Api.Models
{
    public class Resource : IResourceItem
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

        public List<OpeningTimes> OpenHours { get; set; }

        public string Contact { get; set; }

        public string Email { get; set; }

        public string WebLink { get; set; }

        public string Twitter { get; set; }

        public string Instagram { get; set; }

        public string Facebook { get; set; }

        public bool Free { get; set; }

        public string[] Options { get; set; }

        public double? Lat { get; set; }

        public double? Lon { get; set; }

        public string ProviderAddLoc { get; set; }

        public List<OpeningTimes> SpecialHours { get; set; }
        
        public string ContactSpanish { get; set; }

        public string Instructions { get; set; }

        public string Offers { get; set; }

        public string Notes { get; set; }
    }
}
