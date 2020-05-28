namespace CovidSupport.Api.Interfaces
{
    public interface IResourceItem
    {
        int Id { get; set; }

        string ProviderName { get; set; }

        string Description { get; set; }

        string Address { get; set; }

        string City { get; set; }

        string State { get; set; }

        string Zip { get; set; }

        string Region { get; set; }

        double? Lat { get; set; }

        double? Lon { get; set; }

        string[] Options { get; set; }
    }
}
