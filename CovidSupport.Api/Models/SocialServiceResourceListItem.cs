﻿namespace CovidSupport.Api.Models
{
    public class SocialServiceResourceListItem : IResourceItemBase
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string ProviderAddLoc { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Zip { get; set; }

        public Region[] Region { get; set; }

        public string Category { get; set; }

        public string ClassificationType { get; set; }

        public string Description { get; set; }

        public OpenInfo OpenInfo { get; set; }

        public string[] Options { get; set; }

        public bool IsOpen { get; set; }

        public string Icon { get; set; }

        public double? Lat { get; set; }

        public double? Lng { get; set; }

        public string[] Tags { get; set; }        
    }
}
