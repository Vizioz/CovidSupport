using System.Collections.Generic;

namespace CovidSupport.Api.Models
{
    public class ResourceSettings
    {
        public ResourceSettings()
        {
            this.Regions = new List<Region>();
            this.Categories = new List<ResourceCategory>();
        }

        public IEnumerable<Region> Regions { get; set; }

        public IEnumerable<ResourceCategory> Categories { get; set; }
    }
}