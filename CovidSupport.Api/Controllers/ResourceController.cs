using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CovidSupport.Api.Models;
using Examine;
using Newtonsoft.Json;
using Umbraco.Core.Models.PublishedContent;
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
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Website not found for the address " + this.WebsiteUrl);
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
            if (this.Website == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Website not found for the address " + this.WebsiteUrl);
            }

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
            if (this.Website == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Website not found for the address " + this.WebsiteUrl);
            }

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
                else
                {
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Category not found.");
                }
                
                return this.Request.CreateResponse(HttpStatusCode.Accepted, items, this.FormatterConfiguration);
            }
            catch (Exception e)
            {
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message, this.FormatterConfiguration);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetByRegion(string id)
        {
            if (this.Website == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Website not found for the address " + this.WebsiteUrl);
            }

            try
            {
                // TODO
                IEnumerable<ResourceListItem> items = new List<ResourceListItem>();
                var regions = this.GetRegions();
                
                throw new NotImplementedException("not implemented");
            }
            catch (Exception e)
            {
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message, this.FormatterConfiguration);
            }
        }

        [HttpGet]
        public HttpResponseMessage Get(string id)
        {
            if (this.Website == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Website not found for the address " + this.WebsiteUrl);
            }

            try
            {
                var result = this.Index.GetSearcher().CreateQuery("content").Id(id).Execute(1).FirstOrDefault();

                if (result != null)
                {
                    var item = this.BuildResourceItem(result);

                    return this.Request.CreateResponse(HttpStatusCode.Accepted, item, this.FormatterConfiguration);
                }
                else
                {
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Resource not found.");
                }
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
                else if (category.Subcategories.Any())
                {
                    findCategory = this.FindInCategoryTree(category.Subcategories, code);
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
            var regionsNode = this.Website.DescendantOfType("regions");

            return regionsNode != null
                ? regionsNode.Children.Select(x => x.Name)
                : new List<string>();
        }

        private ResourceCategory BuildCategory(IPublishedContent content)
        {
            var category = new ResourceCategory
            {
                Id = content.Id,
                Name = content.Name,
                Code = content.ContentType.Alias
            };

            if (content.ContentType.Alias == "resourceCategory")
            {
                category.Subcategories = content.Children.Select(this.BuildCategory);
            }

            return category;
        }

        private ResourceListItem BuildResourceListItem(ISearchResult searchResult)
        {
            int.TryParse(searchResult.Id, out int id);

            var providerName = searchResult.GetValues("providerName").FirstOrDefault();
            var serviceName = searchResult.GetValues("serviceName").FirstOrDefault();
            var shortDescription = searchResult.GetValues("shortDescription").FirstOrDefault();

            var regionsValue = searchResult.GetValues("serviceRegions").FirstOrDefault();
            var regionsKey = regionsValue != null ? JsonConvert.DeserializeObject<string[]>(regionsValue) : new string[] { };

            var streetAddress = searchResult.GetValues("streetAddress").FirstOrDefault();
            var city = searchResult.GetValues("city").FirstOrDefault();
            var stateValue = searchResult.GetValues("state").FirstOrDefault();
            var stateKeys = stateValue != null ? JsonConvert.DeserializeObject<string[]>(stateValue) : new string[]{};
            var zip = searchResult.GetValues("zip").FirstOrDefault();

            var tagsValue = searchResult.GetValues("tags").FirstOrDefault();
            var tagsKeys = tagsValue != null ? JsonConvert.DeserializeObject<string[]>(tagsValue) : new string[] { };

            var map = searchResult.GetValues("map").FirstOrDefault();
            var mapInfo = map != null ? JsonConvert.DeserializeObject<MapInfo>(map) : new MapInfo();

            return new ResourceListItem
            {
                Id = id,
                ProviderName = providerName,
                ServiceName = serviceName,
                ShortDescription = shortDescription,
                ServiceRegions = regionsKey,
                StreetAddress = streetAddress,
                City = city,
                State = stateKeys.Length > 0 ? stateKeys[0] : null,
                Zip = zip,
                Tags = tagsKeys,
                Lat = mapInfo?.LatLng?.Length > 0 ? mapInfo.LatLng[0] : (double?)null,
                Lon = mapInfo?.LatLng?.Length > 1 ? mapInfo.LatLng[1] : (double?)null
            };
        }

        private Resource BuildResourceItem(ISearchResult searchResult)
        {
            int.TryParse(searchResult.Id, out int id);

            var providerName = searchResult.GetValues("providerName").FirstOrDefault();
            var serviceName = searchResult.GetValues("serviceName").FirstOrDefault();
            var shortDescription = searchResult.GetValues("shortDescription").FirstOrDefault();

            var regionsValue = searchResult.GetValues("serviceRegions").FirstOrDefault();
            var regionsKey = regionsValue != null ? JsonConvert.DeserializeObject<string[]>(regionsValue) : new string[] { };

            var streetAddress = searchResult.GetValues("streetAddress").FirstOrDefault();
            var city = searchResult.GetValues("city").FirstOrDefault();
            var stateValue = searchResult.GetValues("state").FirstOrDefault();
            var stateKeys = stateValue != null ? JsonConvert.DeserializeObject<string[]>(stateValue) : new string[] { };
            var zip = searchResult.GetValues("zip").FirstOrDefault();

            var tagsValue = searchResult.GetValues("tags").FirstOrDefault();
            var tagsKeys = tagsValue != null ? JsonConvert.DeserializeObject<string[]>(tagsValue) : new string[] { };

            var map = searchResult.GetValues("map").FirstOrDefault();
            var mapInfo = map != null ? JsonConvert.DeserializeObject<MapInfo>(map) : new MapInfo();

            return new Resource
            {
                //Id = id,
                //ProviderName = providerName,
                //Description = description,
                //Address = address,
                //City = city,
                //State = state.Length > 0 ? state[0] : null,
                //Zip = zip,
                //Region = region,
                //Lat = mapInfo?.LatLng?.Length > 0 ? mapInfo.LatLng[0] : (double?)null,
                //Lon = mapInfo?.LatLng?.Length > 1 ? mapInfo.LatLng[1] : (double?)null,
                //Options = options,
                //ProviderAddLoc = providerAddLoc,
                //Free = free,
                //Monday = monday,
                //Tuesday = tuesday,
                //Wednesday = wednesday,
                //Thursday = thursday,
                //Friday = friday,
                //Saturday = saturday,
                //Sunday = sunday,
                //SpMonday = spMonday,
                //SpTuesday = spTuesday,
                //SpWednesday = spWednesday,
                //SpThursday = spThursday,
                //SpFriday = spFriday,
                //SpSaturday = spSaturday,
                //SpSunday = spSunday,
                //Contact = contact,
                //ContactSpanish = contactSpanish,
                //Email = email,
                //WebLink = webLink,
                //Twitter = twitter,
                //Instagram = instagram,
                //Facebook = facebook,
                //Instructions = instructions,
                //Offers = offers,
                //Notes = notes
            };
        }
    }
}
