﻿using System.Collections.Generic;

namespace CovidSupport.Api.Models
{
    public class ResourceCategory
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }

        public IEnumerable<ResourceCategory> Subcategories { get; set; }
    }
}
