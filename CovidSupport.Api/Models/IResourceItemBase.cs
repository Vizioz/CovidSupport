namespace CovidSupport.Api.Models
{
    public interface IResourceItemBase
    {
        int Id { get; set; }

        string Name { get; set; }

        string Description { get; set; }

        string Address { get; set; }

        string City { get; set; }

        string State { get; set; }

        string Zip { get; set; }

        string[] Region { get; set; }

        string Category { get; set; }
    }
}
