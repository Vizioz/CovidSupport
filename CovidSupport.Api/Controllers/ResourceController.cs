using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CovidSupport.Api.Models;
using Examine;
using Examine.LuceneEngine.Search;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        ////[HttpGet]
        ////public HttpResponseMessage GetAll()
        ////{
        ////    try
        ////    {
        ////        var results = this.Searcher.CreateQuery("content").All().Execute();

        ////        var items = results.Select(this.BuildResourceListItem);

        ////        return this.Request.CreateResponse(HttpStatusCode.Accepted, items, this.FormatterConfiguration);
        ////    }
        ////    catch (ApiNotFoundException e)
        ////    {
        ////        return this.Request.CreateResponse(HttpStatusCode.BadRequest, e.Message);
        ////    }
        ////    catch (Exception e)
        ////    {
        ////        return this.Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message, this.FormatterConfiguration);
        ////    }
        ////}

        [HttpGet]
        public HttpResponseMessage GetByCategory(int id)
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

                var regionNode = int.TryParse(id, out int intId)
                    ? this.Website.DescendantOfType("regions").FirstChild(x => x.Id == intId)
                    : this.Website.DescendantOfType("regions").FirstChild(x => x.UrlSegment == id);

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
        public HttpResponseMessage Get(int id)
        {
            try
            {
                var result = this.Index.GetSearcher().CreateQuery("content").Id(id.ToString()).Execute(1).FirstOrDefault();

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
            var resourceTypesContainer =
                this.Services.ContentTypeService.GetContainers("Resource Types", 1).FirstOrDefault()?.Id ?? 0;

            var resourceGroupsContainer = this.Services.ContentTypeService.GetContainers("Resource Category Groups", 1)
                                              .FirstOrDefault()?.Id ?? 0;

            var resourceTypeItems = this.Services.ContentTypeService.GetAll()
                .Where(x => x.ParentId == resourceTypesContainer || x.ParentId == resourceGroupsContainer)
                .Select(x => x.Alias);

            var resourcesNode = this.Website.FirstChildOfType("communityResources");

            return this.GetChildrenCategories(resourcesNode, resourceTypeItems);
        }

        private ResourceCategory FindInCategoryTree(IEnumerable<ResourceCategory> categories, int id)
        {
            ResourceCategory findCategory = null;

            foreach (var category in categories)
            {
                if (category.Id == id)
                {
                    findCategory = category;
                }
                else if (category.Subcategories.Any())
                {
                    findCategory = this.FindInCategoryTree(category.Subcategories, id);
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
                ? regionsNode.Children.Select(x => new Region {Id = x.Id, Name = x.Name, Slug = x.UrlSegment})
                : new List<Region>();
        }

        private IEnumerable<ResourceCategory> GetChildrenCategories(IPublishedContent content, IEnumerable<string> typeItems)
        {
            var categories = new List<ResourceCategory>();
            
            if (content != null)
            {
                categories.AddRange(content.Children
                    .Where(x => typeItems.Contains(x.ContentType.Alias, StringComparer.InvariantCultureIgnoreCase))
                    .Select(x => new ResourceCategory
                        {Id = x.Id, Name = x.Name, Subcategories = this.GetChildrenCategories(x, typeItems)}));
            }

            return categories;
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
            // Base properties
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

            // Provider
            var providerAddLoc = searchResult.GetValues("providerAddLoc").FirstOrDefault();
            var free = searchResult.GetValues("map").FirstOrDefault() == "1";

            // Opening Hours
            var openingHours = new List<OpeningTimes>
            {
                this.GetDayOpeningTimes("monday", searchResult.GetValues("monday").FirstOrDefault()),
                this.GetDayOpeningTimes("tuesday", searchResult.GetValues("tuesday").FirstOrDefault()),
                this.GetDayOpeningTimes("wednesday", searchResult.GetValues("wednesday").FirstOrDefault()),
                this.GetDayOpeningTimes("thursday", searchResult.GetValues("thursday").FirstOrDefault()),
                this.GetDayOpeningTimes("friday", searchResult.GetValues("friday").FirstOrDefault()),
                this.GetDayOpeningTimes("saturday", searchResult.GetValues("saturday").FirstOrDefault()),
                this.GetDayOpeningTimes("sunday", searchResult.GetValues("sunday").FirstOrDefault())
            };
            openingHours = openingHours.Where(x => x.Hours.Any()).ToList();

            // Special opening Hours
            var specialHours = new List<OpeningTimes>
            {
                this.GetDayOpeningTimes("monday", searchResult.GetValues("spMonday").FirstOrDefault()),
                this.GetDayOpeningTimes("tuesday", searchResult.GetValues("spTuesday").FirstOrDefault()),
                this.GetDayOpeningTimes("wednesday", searchResult.GetValues("spWednesday").FirstOrDefault()),
                this.GetDayOpeningTimes("thursday", searchResult.GetValues("spThursday").FirstOrDefault()),
                this.GetDayOpeningTimes("friday", searchResult.GetValues("spFriday").FirstOrDefault()),
                this.GetDayOpeningTimes("saturday", searchResult.GetValues("spSaturday").FirstOrDefault()),
                this.GetDayOpeningTimes("sunday", searchResult.GetValues("spSunday").FirstOrDefault())
            };
            specialHours = specialHours.Where(x => x.Hours.Any()).ToList();

            // Contact
            var contact = searchResult.GetValues("contact").FirstOrDefault();
            var contactSpanish = searchResult.GetValues("contactSpanish").FirstOrDefault();
            var email = searchResult.GetValues("email").FirstOrDefault();
            var webLink = searchResult.GetValues("webLink").FirstOrDefault();
            var twitter = searchResult.GetValues("twitter").FirstOrDefault();
            var instagram = searchResult.GetValues("instagram").FirstOrDefault();
            var facebook = searchResult.GetValues("facebook").FirstOrDefault();
            
            // Instructions
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
                OpenHours = openingHours,
                SpecialHours = specialHours,
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

        private OpeningTimes GetDayOpeningTimes(string day, string str)
        {
            var openingTimes = new OpeningTimes(day);

            if (string.IsNullOrEmpty(str))
            {
                return openingTimes;
            }

            try
            {
                var hours = new List<StartEndTime>();
                var openingHoursArray = JsonConvert.DeserializeObject<JArray>(str);//.FirstOrDefault();

                foreach (var openingHoursVal in openingHoursArray)
                {
                    var startTime = openingHoursVal.Value<DateTime?>("startTime");
                    var endTime = openingHoursVal.Value<DateTime?>("endTime");

                    if (startTime != null || endTime != null)
                    {
                        var startEndTime = new StartEndTime();

                        if (startTime != null)
                        {
                            startEndTime.StarTime = ((DateTime)startTime).ToString("HH:mm:ss");
                        }

                        if (endTime != null)
                        {
                            startEndTime.EndTime = ((DateTime)endTime).ToString("HH:mm:ss");
                        }

                        hours.Add(startEndTime);
                    }
                }

                openingTimes.Hours = hours;
            }
            catch (Exception)
            {
                return openingTimes;
            }

            return openingTimes;
        }
    }
}
