namespace CovidSupport.Api.Interfaces
{
    public interface IResourceItem
    {
        int Id { get; set; }

        string ProviderName { get; set; }

        string ServiceName { get; set; }

        string ShortDescription { get; set; }

        string[] ServiceRegions { get; set; }

        string StreetAddress { get; set; }

        string City { get; set; }

        string State { get; set; }

        string Zip { get; set; }
        
        string[] Tags { get; set; }

        double? Lat { get; set; }

        double? Lon { get; set; }
    }
}
