namespace CovidSupport.Api.Models
{
    public interface IResourceItemBase
    {
        int Id { get; set; }

        string Name { get; set; }

        string ProviderAddLoc { get; set; }

        string Address { get; set; }

        string City { get; set; }

        string State { get; set; }

        string Zip { get; set; }

        Region[] Region { get; set; }

        string Category { get; set; }

        string ClassificationType { get; set; }

        string Description { get; set; }

        OpenInfo OpenInfo { get; set; }

        string[] Options { get; set; }

        bool IsOpen { get; set; }

        string Icon { get; set; }

        double? Lat { get; set; }

        double? Lng { get; set; }
    }
}
