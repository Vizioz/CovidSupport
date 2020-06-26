﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CovidSupport.Api.Models;
using Examine;
using Examine.LuceneEngine.Search;
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
            try
            {
                var settings = new ResourceSettings
                {
                    Categories = this.GetCategories(),
                    Regions = this.GetRegions()
                };

                return this.Request.CreateResponse(HttpStatusCode.Accepted, settings, this.FormatterConfiguration);
            }
            catch (ApiNotFoundException e)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, e.Message);
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
            catch (ApiNotFoundException e)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, e.Message);
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
                else
                {
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Category not found.");
                }
                
                return this.Request.CreateResponse(HttpStatusCode.Accepted, items, this.FormatterConfiguration);
            }
            catch (ApiNotFoundException e)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, e.Message);
            }
            catch (Exception e)
            {
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message, this.FormatterConfiguration);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetByRegion(string id)
        {
            try
            {
                IEnumerable<ResourceListItem> items;

                var regionNode = this.Website.DescendantOfType("regions").FirstChild(x =>
                    string.Equals(x.Name, id, StringComparison.InvariantCultureIgnoreCase));

                if (regionNode != null)
                {
                    var searcher = this.Index.GetSearcher();

                    var query = (LuceneSearchQueryBase)searcher.CreateQuery("content");
                    query.QueryParser.AllowLeadingWildcard = true;

                    var val = "*" + regionNode.Key.ToString().Replace("-", string.Empty);
                    var results = query.Field("serviceRegions", val.MultipleCharacterWildcard()).Execute();

                    items = results.Select(this.BuildResourceListItem);
                }
                else
                {
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Region not found.");
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, items, this.FormatterConfiguration);
            }
            catch (ApiNotFoundException e)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, e.Message);
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
            catch (ApiNotFoundException e)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, e.Message);
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

        private IEnumerable<Region> GetRegions()
        {
            var regionsNode = this.Website.DescendantOfType("regions");

            return regionsNode != null
                ? regionsNode.Children.Select(x => new Region {Id = x.Id, Name = x.Name})
                : new List<Region>();
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
            var description = searchResult.GetValues("cuisine").FirstOrDefault();
            var address = searchResult.GetValues("address").FirstOrDefault();
            var city = searchResult.GetValues("city").FirstOrDefault();
            var stateList = searchResult.GetValues("state").FirstOrDefault();
            var state = stateList != null ? JsonConvert.DeserializeObject<string[]>(stateList) : new string[]{};
            var zip = searchResult.GetValues("zip").FirstOrDefault();
            var region = searchResult.GetValues("region").FirstOrDefault();
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
                Region = region,
                Lat = mapInfo?.LatLng?.Length > 0 ? mapInfo.LatLng[0] : (double?)null,
                Lon = mapInfo?.LatLng?.Length > 1 ? mapInfo.LatLng[1] : (double?)null,
                Options = options
            };
        }

        private Resource BuildResourceItem(ISearchResult searchResult)
        {
            int.TryParse(searchResult.Id, out int id);
            var providerName = searchResult.GetValues("providerName").FirstOrDefault();
            var description = searchResult.GetValues("cuisine").FirstOrDefault();
            var address = searchResult.GetValues("address").FirstOrDefault();
            var city = searchResult.GetValues("city").FirstOrDefault();
            var stateList = searchResult.GetValues("state").FirstOrDefault();
            var state = stateList != null ? JsonConvert.DeserializeObject<string[]>(stateList) : new string[] { };
            var zip = searchResult.GetValues("zip").FirstOrDefault();
            var region = searchResult.GetValues("region").FirstOrDefault();
            var map = searchResult.GetValues("map").FirstOrDefault();
            var mapInfo = map != null ? JsonConvert.DeserializeObject<MapInfo>(map) : new MapInfo();
            var options = searchResult.Values.Where(x => x.Value == "1").Select(x => x.Key).ToArray();

            var providerAddLoc = searchResult.GetValues("providerAddLoc").FirstOrDefault();
            var free = searchResult.GetValues("map").FirstOrDefault() == "1";

            var monday = searchResult.GetValues("monday").FirstOrDefault();
            var tuesday = searchResult.GetValues("tuesday").FirstOrDefault();
            var wednesday = searchResult.GetValues("wednesday").FirstOrDefault();
            var thursday = searchResult.GetValues("thursday").FirstOrDefault();
            var friday = searchResult.GetValues("friday").FirstOrDefault();
            var saturday = searchResult.GetValues("saturday").FirstOrDefault();
            var sunday = searchResult.GetValues("sunday").FirstOrDefault();
            var spMonday = searchResult.GetValues("spMonday").FirstOrDefault();
            var spTuesday = searchResult.GetValues("spTuesday").FirstOrDefault();
            var spWednesday = searchResult.GetValues("spWednesday").FirstOrDefault();
            var spThursday = searchResult.GetValues("spThursday").FirstOrDefault();
            var spFriday = searchResult.GetValues("spFriday").FirstOrDefault();
            var spSaturday = searchResult.GetValues("spSaturday").FirstOrDefault();
            var spSunday = searchResult.GetValues("spSunday").FirstOrDefault();

            var contact = searchResult.GetValues("contact").FirstOrDefault();
            var contactSpanish = searchResult.GetValues("contactSpanish").FirstOrDefault();
            var email = searchResult.GetValues("email").FirstOrDefault();
            var webLink = searchResult.GetValues("webLink").FirstOrDefault();
            var twitter = searchResult.GetValues("twitter").FirstOrDefault();
            var instagram = searchResult.GetValues("instagram").FirstOrDefault();
            var facebook = searchResult.GetValues("facebook").FirstOrDefault();
            
            var instructions = searchResult.GetValues("instructions").FirstOrDefault();
            var offers = searchResult.GetValues("offers").FirstOrDefault();
            var notes = searchResult.GetValues("notes").FirstOrDefault();

            return new Resource
            {
                Id = id,
                ProviderName = providerName,
                Description = description,
                Address = address,
                City = city,
                State = state.Length > 0 ? state[0] : null,
                Zip = zip,
                Region = region,
                Lat = mapInfo?.LatLng?.Length > 0 ? mapInfo.LatLng[0] : (double?)null,
                Lon = mapInfo?.LatLng?.Length > 1 ? mapInfo.LatLng[1] : (double?)null,
                Options = options,
                ProviderAddLoc = providerAddLoc,
                Free = free,
                Monday = monday,
                Tuesday = tuesday,
                Wednesday = wednesday,
                Thursday = thursday,
                Friday = friday,
                Saturday = saturday,
                Sunday = sunday,
                SpMonday = spMonday,
                SpTuesday = spTuesday,
                SpWednesday = spWednesday,
                SpThursday = spThursday,
                SpFriday = spFriday,
                SpSaturday = spSaturday,
                SpSunday = spSunday,
                Contact = contact,
                ContactSpanish = contactSpanish,
                Email = email,
                WebLink = webLink,
                Twitter = twitter,
                Instagram = instagram,
                Facebook = facebook,
                Instructions = instructions,
                Offers = offers,
                Notes = notes
            };
        }
    }
}
