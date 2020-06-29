using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.WebApi;

namespace CovidSupport.Api.Controllers
{
    public class ImportController : UmbracoAuthorizedApiController
    {
        [HttpGet]
        public HttpResponseMessage ImportRegions()
        {
            var success = new List<string>();
            var errors = new List<string>();

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
                    .Where(x => x.ContentType.Alias == "region").ToList();

                foreach (var resource in resources)
                {
                    try
                    {
                        var message = this.ImportRegion(resource, availableRegions);
                        success.Add(message);
                    }
                    catch (Exception e)
                    {
                        errors.Add(resource.Id + " - ERROR - " + e.Message);
                    }
                }
            }

            var json = JsonConvert.SerializeObject(new { TotalSuccess = success.Count, success, TotalErrors = errors.Count, errors });
            var retVal = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };

            return retVal;
        }

        [HttpGet]
        public HttpResponseMessage ImportRegion(int id)
        {
            string message;

            var cs = this.Services.ContentService;
            var resource = cs.GetById(id);

            var types = new string[]
            {
                "familyMeal", "resourceFarm", "resourceFarmMarket", "resourceFoodBeverage", "freeMeal",
                "resourceGroceries", "resourcePetSupplies", "resourcePharmacy", "resourceRestaurant"
            };

            if (resource != null)
            {
                var websiteContent = cs.GetAncestors(resource).FirstOrDefault(x => x.ContentType.Alias == "website");
                var website = websiteContent != null ? this.Umbraco.Content(websiteContent.Id) : null;

                if (website != null)
                {
                    var availableRegions = cs.GetPagedDescendants(website.Id, 0, 10000, out long totalRecordsReg)
                        .Where(x => x.ContentType.Alias == "region");

                    message = this.ImportRegion(resource, availableRegions);
                }
                else
                {
                    message = id + " - ERROR - No website found";
                }
            }
            else
            {
                message = id + " - ERROR - Resource not found";
            }

            var retVal = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(message))
            };

            return retVal;
        }

        private string ImportRegion(IContent resource, IEnumerable<IContent> availableRegions)
        {
            var regionIds = new List<Udi>();
            var ids = new List<string>();

            var regions = resource.GetValue<string>("region")?.Split(',') ?? new string[] { };

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
                    ids.Add(selectedRegion.Name);
                }
            }

            resource.SetValue("regionPicker", string.Join(",", regionIds));

            if (resource.Published)
            {
                this.Services.ContentService.SaveAndPublish(resource);
                return resource.Id + " - SaveAndPublish - " + string.Join(",", ids);
            }
            else
            {
                this.Services.ContentService.Save(resource);
                return resource.Id + " - Save - " + string.Join(",", ids);
            }
        }

        private IEnumerable<IContent> GetAvailableRegions(IPublishedContent website)
        {
            return this.Services.ContentService.GetPagedDescendants(website.Id, 0, 10000, out long totalRecordsReg)
                .Where(x => x.ContentType.Alias == "region");
        }
    }
}
