﻿using System.Collections.Generic;

namespace CovidSupport.Api.Models
{
    public class CategoryResources
    {
        public IEnumerable<IResourceItemBase> Markers { get; set; }

        public string HighlightFilters { get; set; }

        public bool ShowListFirst { get; set; }

        public bool ShowCounties { get; set; }

        public bool ShowTooltips { get; set; }
    }
}
