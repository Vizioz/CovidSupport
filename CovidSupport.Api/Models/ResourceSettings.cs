using System.Collections;
using System.Collections.Generic;

namespace CovidSupport.Api.Models
{
    public class ResourceSettings
    {
        public ResourceSettings()
        {
            this.Regions = new List<string>();
            this.Categories = new List<ResourceCategory>();
        }

        public IEnumerable<string> Regions { get; set; }

        public IEnumerable<ResourceCategory> Categories { get; set; }
    }
}