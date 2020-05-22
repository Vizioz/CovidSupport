using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CovidSupport.Api.Models;
using Examine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Examine;
using Umbraco.Web;

namespace CovidSupport.Api.Controllers
{
    public class ResourceController : BaseApiController
    {
        [HttpGet]
        public HttpResponseMessage Settings()
        {
            if (this.Website == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadGateway, "Website not found.", this.FormatterConfiguration);
            }

            try
            {
                var settings = new ResourceSettings
                {
                    Categories = this.GetCategories(),
                    Regions = this.GetRegions()
                };

                return this.Request.CreateResponse(HttpStatusCode.Accepted, settings, this.FormatterConfiguration);
            }
            catch (Exception e)
            {
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message, this.FormatterConfiguration);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetAll()
        {
            try
            {
                var results = this.Searcher.CreateQuery("content").All().Execute();

                var items = results.Select(this.BuildResourceListItem);

                return this.Request.CreateResponse(HttpStatusCode.Accepted, items, this.FormatterConfiguration);
            }
            catch (Exception e)
            {
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message, this.FormatterConfiguration);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetByCategory(string id)
        {
            try
            {
                IEnumerable<ResourceListItem> items = new List<ResourceListItem>();
                var categories = this.GetCategories();
                var category = this.FindInCategoryTree(categories, id);

                if (category != null)
                {
                    var results = this.Index.GetSearcher().CreateQuery("content").Field("parentID", category.Id.ToString()).Execute();
                    items = results.Select(this.BuildResourceListItem);
                }
                
                return this.Request.CreateResponse(HttpStatusCode.Accepted, items, this.FormatterConfiguration);
            }
            catch (Exception e)
            {
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message, this.FormatterConfiguration);
            }
        }

        [HttpGet]
        public HttpResponseMessage Get(string id)
        {
            try
            {
                var result = this.Index.GetSearcher().CreateQuery("content").Id(id).Execute(1).FirstOrDefault();

                var item = this.BuildResourceListItem(result);

                return this.Request.CreateResponse(HttpStatusCode.Accepted, item, this.FormatterConfiguration);
            }
            catch (Exception e)
            {
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message, this.FormatterConfiguration);
            }
        }

        private IEnumerable<ResourceCategory> GetCategories()
        {
            var resourcesNode = this.Website.FirstChildOfType("communityResources");

            return resourcesNode != null
                ? resourcesNode.Children.Select(this.BuildCategory)
                : new List<ResourceCategory>();
        }

        private ResourceCategory FindInCategoryTree(IEnumerable<ResourceCategory> categories, string code)
        {
            ResourceCategory findCategory = null;

            foreach (var category in categories)
            {
                if (string.Equals(category.Code, code, StringComparison.InvariantCultureIgnoreCase))
                {
                    findCategory = category;
                }
                else if (category.SubCategories.Any())
                {
                    findCategory = this.FindInCategoryTree(category.SubCategories, code);
                }

                if (findCategory != null)
                {
                    break;
                }
            }

            return findCategory;
        }

        private IEnumerable<string> GetRegions()
        {
            var results = this.Searcher.CreateQuery("content").All().Execute();

            return results.Select(x => x.Values["region"]).Where(val => !string.IsNullOrEmpty(val)).Distinct();
        }

        private ResourceCategory BuildCategory(IPublishedContent content)
        {
            var category = new ResourceCategory
            {
                Id = content.Id,
                Name = content.Name,
                Code = content.Name
            };

            if (content.ContentType.Alias == "resourceCategory")
            {
                category.SubCategories = content.Children.Select(this.BuildCategory);
            }

            return category;
        }

        private ResourceListItem BuildResourceListItem(ISearchResult searchResult)
        {
            int.TryParse(searchResult.Id, out int id);
            var providerName = searchResult.GetValues("providerName").FirstOrDefault();
            var description = searchResult.GetValues("cuisine").FirstOrDefault();
            var address = searchResult.GetValues("address").FirstOrDefault();
            var city = searchResult.GetValues("city").FirstOrDefault();
            var stateList = searchResult.GetValues("state").FirstOrDefault();
            var state = stateList != null ? JsonConvert.DeserializeObject<string[]>(stateList) : new string[]{};
            var zip = searchResult.GetValues("zip").FirstOrDefault();
            var map = searchResult.GetValues("map").FirstOrDefault();
            var mapInfo = map != null ? JsonConvert.DeserializeObject<MapInfo>(map) : new MapInfo();

            var options = searchResult.Values.Where(x => x.Value == "1").Select(x => x.Key).ToArray();

            return new ResourceListItem
            {
                Id = id,
                ProviderName = providerName,
                Description = description,
                Address = address,
                City = city,
                State = state.Length > 0 ? state[0] : null,
                Zip = zip,
                Lat = mapInfo?.LatLng?.Length > 0 ? mapInfo.LatLng[0] : (double?)null,
                Lon = mapInfo?.LatLng?.Length > 1 ? mapInfo.LatLng[1] : (double?)null,
                Options = options
            };
        }
    }
}
