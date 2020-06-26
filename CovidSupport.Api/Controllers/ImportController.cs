using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Web.WebApi;

namespace CovidSupport.Api.Controllers
{
    public class ImportController : UmbracoAuthorizedApiController
    {
        [HttpGet]
        public HttpResponseMessage ImportRegions()
        {
            var cs = this.Services.ContentService;

            var websites = this.Umbraco.ContentAtRoot();
            var types = new string[]
            {
                "familyMeal", "resourceFarm", "resourceFarmMarket", "resourceFoodBeverage", "freeMeal",
                "resourceGroceries", "resourcePetSupplies", "resourcePharmacy", "resourceRestaurant"
            };

            foreach (var website in websites)
            {
                var resources = cs.GetPagedDescendants(website.Id, 0, 10000, out long totalRecords)
                    .Where(x => types.Contains(x.ContentType.Alias));

                var availableRegions = cs.GetPagedDescendants(website.Id, 0, 10000, out long totalRecordsReg)
                    .Where(x => x.ContentType.Alias == "region");

                foreach (var resource in resources)
                {
                    var regionIds = new List<Udi>();

                    var regions = resource.GetValue<string>("region").Split(',');

                    foreach (var region in regions)
                    {
                        var regionName = region.Replace("_", " ", StringComparison.InvariantCultureIgnoreCase)
                            .Replace("pearson", "person", StringComparison.InvariantCultureIgnoreCase).Trim();

                        if (!region.EndsWith("county", StringComparison.InvariantCultureIgnoreCase))
                        {
                            regionName += " county";
                        }

                        var selectedRegion = availableRegions.FirstOrDefault(x =>
                            string.Equals(x.Name, regionName, StringComparison.InvariantCultureIgnoreCase));

                        if (selectedRegion != null)
                        {
                            regionIds.Add(Udi.Create("document", selectedRegion.Key));
                        }
                    }

                    resource.SetValue("regionPicker", string.Join(",", regionIds));

                    if (resource.Published)
                    {
                        cs.SaveAndPublish(resource);
                    }
                    else
                    {
                        cs.Save(resource);
                    }
                }
            }

            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }
    }
}
