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

                return this.Request.CreateResponse(HttpStatusCode.OK, settings, this.FormatterConfiguration);
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

                var items = results.Select(x => this.BuildResourceItem(x, this.CultureName));

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
        public HttpResponseMessage GetByCategory(string id)
        {
            try
            {
                IEnumerable<ResourceListItem> items;
                var categories = this.GetCategories();
                var category = this.FindInCategoryTree(categories, id);

                if (category != null)
                {
                    var results = this.Index.GetSearcher().CreateQuery("content").Field("parentID", category.Id.ToString()).Execute();
                    items = results.Select(x => this.BuildResourceListItem(x, this.CultureName));
                }
                else
                {
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Category not found.");
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

                    var query = (LuceneSearchQueryBase) searcher.CreateQuery("content");
                    query.QueryParser.AllowLeadingWildcard = true;

                    var val = "*" + regionNode.Key.ToString().Replace("-", string.Empty);
                    var results = query.Field("serviceRegions", val.MultipleCharacterWildcard()).Execute();

                    items = results.Select(x => this.BuildResourceListItem(x, this.CultureName));
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
                    var item = this.BuildResourceItem(result, this.CultureName);

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

        private ResourceListItem BuildResourceListItem(ISearchResult searchResult, string culture)
        {
            int.TryParse(searchResult.Id, out int id);

            var providerName = searchResult.GetValues("providerName_" + culture).FirstOrDefault();
            var serviceName = searchResult.GetValues("serviceName_" + culture).FirstOrDefault();
            var shortDescription = searchResult.GetValues("shortDescription_" + culture).FirstOrDefault();

            var regions = this.GetNodesName(searchResult.GetValues("serviceRegions").FirstOrDefault());

            var streetAddress = searchResult.GetValues("streetAddress").FirstOrDefault();
            var city = searchResult.GetValues("city").FirstOrDefault();
            var stateValue = searchResult.GetValues("state").FirstOrDefault();
            var stateKeys = stateValue != null ? JsonConvert.DeserializeObject<string[]>(stateValue) : new string[] { };
            var state = stateKeys.Length > 0 ? stateKeys[0] : null;
            var zip = searchResult.GetValues("zip").FirstOrDefault();

            var tags = this.GetNodesName(searchResult.GetValues("tags").FirstOrDefault());

            var map = searchResult.GetValues("map").FirstOrDefault();
            var mapInfo = map != null ? JsonConvert.DeserializeObject<MapInfo>(map) : new MapInfo();

            return new ResourceListItem
            {
                Id = id,
                ProviderName = providerName,
                ServiceName = serviceName,
                ShortDescription = shortDescription,
                ServiceRegions = regions,
                StreetAddress = streetAddress,
                City = city,
                State = state,
                Zip = zip,
                Tags = tags,
                Lat = mapInfo?.LatLng?.Length > 0 ? mapInfo.LatLng[0] : (double?)null,
                Lon = mapInfo?.LatLng?.Length > 1 ? mapInfo.LatLng[1] : (double?)null,
            };
        }

        private Resource BuildResourceItem(ISearchResult searchResult, string culture)
        {
            int.TryParse(searchResult.Id, out int id);

            var providerName = searchResult.GetValues("providerName_" + culture).FirstOrDefault();
            var serviceName = searchResult.GetValues("serviceName_" + culture).FirstOrDefault();
            var shortDescription = searchResult.GetValues("shortDescription_" + culture).FirstOrDefault();
            var longDescription = searchResult.GetValues("longDescription_" + culture).FirstOrDefault();

            var regions = this.GetNodesName(searchResult.GetValues("serviceRegions").FirstOrDefault());
            var eligibility = searchResult.GetValues("eligibility_" + culture).FirstOrDefault();
            var geographicRestrictions = searchResult.GetValues("geographicRestrictions_" + culture).FirstOrDefault();
            var safeForUndocumentedIndividuals = searchResult.GetValues("safeForUndocumentedIndividuals").FirstOrDefault() == "1";
            var free = searchResult.GetValues("free").FirstOrDefault() == "1";
            var lowCost = searchResult.GetValues("lowCost").FirstOrDefault() == "1";
            var resourceAccessNotes = searchResult.GetValues("resourceAccessNotes_" + culture).FirstOrDefault();

            var streetAddress = searchResult.GetValues("streetAddress").FirstOrDefault();
            var city = searchResult.GetValues("city").FirstOrDefault();
            var stateValue = searchResult.GetValues("state").FirstOrDefault();
            var stateKeys = stateValue != null ? JsonConvert.DeserializeObject<string[]>(stateValue) : new string[] { };
            var state = stateKeys.Length > 0 ? stateKeys[0] : null;
            var zip = searchResult.GetValues("zip").FirstOrDefault();

            var monday = this.GetOpeningHours(searchResult.GetValues("monday").FirstOrDefault());
            var tuesday = this.GetOpeningHours(searchResult.GetValues("tuesday").FirstOrDefault());
            var wednesday = this.GetOpeningHours(searchResult.GetValues("wednesday").FirstOrDefault());
            var thursday = this.GetOpeningHours(searchResult.GetValues("thursday").FirstOrDefault());
            var friday = this.GetOpeningHours(searchResult.GetValues("friday").FirstOrDefault());
            var saturday = this.GetOpeningHours(searchResult.GetValues("saturday").FirstOrDefault());
            var sunday = this.GetOpeningHours(searchResult.GetValues("sunday").FirstOrDefault());
            var holidays = searchResult.GetValues("holidays_" + culture).FirstOrDefault();
            var specialHours = searchResult.GetValues("specialHours_" + culture).FirstOrDefault();
            var statusValue = searchResult.GetValues("status_" + culture).FirstOrDefault();
            var statusKeys = statusValue != null ? JsonConvert.DeserializeObject<string[]>(statusValue) : new string[] { };
            var status = statusKeys.Length > 0 ? statusKeys[0] : null;

            var primaryPhone = searchResult.GetValues("primaryPhone").FirstOrDefault();
            var languagePhones = this.GetLanguagePhones(searchResult.GetValues("languagePhones").FirstOrDefault());
            var crisisPhone = searchResult.GetValues("crisisPhone").FirstOrDefault();
            var crisisPhoneInstructions = searchResult.GetValues("crisisPhoneInstructions_" + culture).FirstOrDefault();
            var afterHoursPhone = searchResult.GetValues("afterHoursPhone").FirstOrDefault();
            var afterHoursPhoneInstructions = searchResult.GetValues("afterHoursPhoneInstructions_" + culture).FirstOrDefault();
            var website = searchResult.GetValues("website").FirstOrDefault();
            var email = searchResult.GetValues("email").FirstOrDefault();
            var twitter = searchResult.GetValues("twitter").FirstOrDefault();
            var facebook = searchResult.GetValues("facebook").FirstOrDefault();

            var lgbtqia = searchResult.GetValues("lgbtqia").FirstOrDefault() == "1";
            var familiesWithChildren = searchResult.GetValues("familiesWithChildren").FirstOrDefault() == "1";
            var individualsWithSpecialNeeds = searchResult.GetValues("individualsWithSpecialNeeds").FirstOrDefault() == "1";
            var seniors = searchResult.GetValues("seniors").FirstOrDefault() == "1";
            var veterans = searchResult.GetValues("veterans").FirstOrDefault() == "1";
            var collegeStudents = searchResult.GetValues("collegeStudents").FirstOrDefault() == "1";
            var reEnteringIndividuals = searchResult.GetValues("reEnteringIndividuals").FirstOrDefault() == "1";
            var experiencingHomelessness = searchResult.GetValues("experiencingHomelessness").FirstOrDefault() == "1";
            var survivorsOfDomesticViolence = searchResult.GetValues("survivorsOfDomesticViolence").FirstOrDefault() == "1";
            var undocumentedIndividuals = searchResult.GetValues("undocumentedIndividuals").FirstOrDefault() == "1";
            var immigrants = searchResult.GetValues("immigrants").FirstOrDefault() == "1";

            var languagesSupported = this.GetNodesName(searchResult.GetValues("languagesSupported").FirstOrDefault());

            var acceptsMedicare = searchResult.GetValues("acceptsMedicare").FirstOrDefault() == "1";
            var acceptsMedicaid = searchResult.GetValues("acceptsMedicaid").FirstOrDefault() == "1";
            var acceptsUninsuredPatients = searchResult.GetValues("acceptsUninsuredPatients").FirstOrDefault() == "1";

            var tags = this.GetNodesName(searchResult.GetValues("tags").FirstOrDefault());

            var map = searchResult.GetValues("map").FirstOrDefault();
            var mapInfo = map != null ? JsonConvert.DeserializeObject<MapInfo>(map) : new MapInfo();

            return new Resource
            {
                Id = id,
                ProviderName = providerName,
                ServiceName = serviceName,
                ShortDescription = shortDescription,
                ServiceRegions = regions,
                StreetAddress = streetAddress,
                City = city,
                State = state,
                Zip = zip,
                Tags = tags,
                Lat = mapInfo?.LatLng?.Length > 0 ? mapInfo.LatLng[0] : (double?)null,
                Lon = mapInfo?.LatLng?.Length > 1 ? mapInfo.LatLng[1] : (double?)null,
                LongDescription = longDescription,
                Eligibility = eligibility,
                GeographicRestrictions = geographicRestrictions,
                SafeForUndocumentedIndividuals = safeForUndocumentedIndividuals,
                Free = free,
                LowCost = lowCost,
                ResourceAccessNotes = resourceAccessNotes,
                Monday = monday,
                Tuesday = tuesday,
                Wednesday = wednesday,
                Thursday = thursday,
                Friday = friday,
                Saturday = saturday,
                Sunday = sunday,
                Holidays = holidays,
                SpecialHours = specialHours,
                Status = status,
                PrimaryPhone = primaryPhone,
                LanguagePhones = languagePhones,
                CrisisPhone = crisisPhone,
                CrisisPhoneInstructions = crisisPhoneInstructions,
                AfterHoursPhone = afterHoursPhone,
                AfterHoursPhoneInstructions = afterHoursPhoneInstructions,
                Website = website,
                Email = email,
                Twitter = twitter,
                Facebook = facebook,
                LGBTQIA = lgbtqia,
                FamiliesWithChildren = familiesWithChildren,
                IndividualsWithSpecialNeeds = individualsWithSpecialNeeds,
                Seniors = seniors,
                Veterans = veterans,
                CollegeStudents = collegeStudents,
                ReEnteringIndividuals = reEnteringIndividuals,
                ExperiencingHomelessness = experiencingHomelessness,
                SurvivorsOfDomesticViolence = survivorsOfDomesticViolence,
                UndocumentedIndividuals = undocumentedIndividuals,
                Immigrants = immigrants,
                LanguagesSupported = languagesSupported,
                AcceptsMedicare = acceptsMedicare,
                AcceptsMedicaid = acceptsMedicaid,
                AcceptsUninsuredPatients = acceptsUninsuredPatients
            };
        }

        private string[] GetNodesName(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return new string[] { };
            }

            var ids = str.Split(',');

            return ids.Select(id => this.Umbraco.Content(id)).Where(x => x != null).Select(x => x.Name).ToArray();
        }

        private StartEndTime GetOpeningHours(string str)
        {
            var openingHours = new StartEndTime();

            if (string.IsNullOrEmpty(str))
            {
                return openingHours;
            }

            try
            {
                var openingHoursVal = JsonConvert.DeserializeObject<JArray>(str)?.FirstOrDefault();

                if (openingHoursVal != null)
                {
                    var startTime = openingHoursVal.Value<DateTime>("startTime");

                    if (startTime > DateTime.MinValue)
                    {
                        openingHours.StartTime =
                            DateTime.MinValue.AddHours(startTime.Hour).AddMinutes(startTime.Minute);
                    }

                    var endTime = openingHoursVal.Value<DateTime>("endTime");

                    if (endTime > DateTime.MinValue)
                    {
                        openingHours.EndTime =
                            DateTime.MinValue.AddHours(endTime.Hour).AddMinutes(endTime.Minute);
                    }
                }
            }
            catch (Exception)
            {
                return openingHours;
            }

            return openingHours;
        }

        private IEnumerable<LanguagePhone> GetLanguagePhones(string str)
        {
            var phones = new List<LanguagePhone>();

            if (string.IsNullOrEmpty(str))
            {
                return phones;
            }

            try
            {
                var languagePhonesVal = JsonConvert.DeserializeObject<JArray>(str);

                if (languagePhonesVal != null && languagePhonesVal.Any())
                {
                    foreach (var languagePhoneVal in languagePhonesVal)
                    {
                        var languageVal = languagePhoneVal.Value<string>("language");
                        var language = !string.IsNullOrEmpty(languageVal)
                            ? this.Umbraco.Content(languageVal)?.Name
                            : null;
                        var phoneNumber = languagePhoneVal.Value<string>("phoneNumber");

                        phones.Add(new LanguagePhone { Language = language, Phone = phoneNumber });
                    }
                }

                return phones;
            }
            catch (Exception)
            {
                return phones;
            }
        }
    }
}
