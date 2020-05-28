using CovidSupport.Api.Interfaces;

namespace CovidSupport.Api.Models
{
    public class Resource : IResourceItem
    {
        public Resource()
        {
            this.Options = new string[] { };
        }

        public int Id { get; set; }

        public string ProviderName { get; set; }

        public string Description { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Zip { get; set; }

        public string Region { get; set; }

        public double? Lat { get; set; }

        public double? Lon { get; set; }

        public string[] Options { get; set; }

        public bool Free { get; set; }

        public string ProviderAddLoc { get; set; }

        public string Monday { get; set; }

        public string Tuesday { get; set; }

        public string Wednesday { get; set; }

        public string Thursday { get; set; }

        public string Friday { get; set; }

        public string Saturday { get; set; }

        public string Sunday { get; set; }

        public string SpMonday { get; set; }

        public string SpTuesday { get; set; }

        public string SpWednesday { get; set; }

        public string SpThursday { get; set; }

        public string SpFriday { get; set; }

        public string SpSaturday { get; set; }

        public string SpSunday { get; set; }

        public string Contact { get; set; }

        public string ContactSpanish { get; set; }

        public string Email { get; set; }

        public string WebLink { get; set; }

        public string Twitter { get; set; }

        public string Instagram { get; set; }

        public string Facebook { get; set; }

        public string Instructions { get; set; }

        public string Offers { get; set; }

        public string Notes { get; set; }
    }
}
