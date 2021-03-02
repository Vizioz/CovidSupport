using CovidSupport.Api.Models;
using Examine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace CovidSupport.Api.Factories
{
    public class SocialServicesResourceFactory : ResourceFactoryBase
    {
        public SocialServicesResourceFactory(UmbracoHelper helper, IContentService contentService, string culture) : base(helper, contentService, culture)
        {
        }

        public override IResourceItem BuildResource(ISearchResult searchResult)
        {
            if (searchResult == null)
            {
                return null;
            }

            // Id
            int.TryParse(searchResult.Id, out int id);
            var category = this.GetNodeContentTypeAlias(id);
            var icon = this.GetIcon(searchResult);
            var updateDate = this.GetNode(id).UpdateDate;

            // Provider
            var serviceName = this.GetResultCultureValueWithFallback(searchResult, "serviceName");
            var providerName = this.GetResultCultureValueWithFallback(searchResult, "providerName");
            var shortDescription = this.GetResultCultureValueWithFallback(searchResult, "shortDescription");
            var longDescription = this.GetResultCultureValueWithFallback(searchResult, "longDescription");
            var classificationType = this.GetSingleNodeName(this.GetResultValue(searchResult, "classificationType"));

            if (string.IsNullOrEmpty(serviceName))
            {
                if (string.IsNullOrEmpty(providerName))
                {
                    serviceName = this.GetResultCultureValueWithFallback(searchResult, "nodeName");
                } else
                {
                    serviceName = providerName;
                    providerName = null;
                }
            }

            // Access
            var eligibility = this.GetResultCultureValueWithFallback(searchResult, "eligibility");
            var access = this.GetResultCultureValueWithFallback(searchResult, "resourceAccessNotes");
            var region = this.GetNodesName(this.GetResultValue(searchResult, "region"));
            var geographicRestrictions = this.GetResultCultureValueWithFallback(searchResult, "geographicRestrictions");
            var safeForUndocumentedIndividuals = this.GetResultBooleanValue(searchResult, "safeForUndocumentedIndividuals");
            var free = this.GetResultBooleanValue(searchResult, "free");
            var lowCost = this.GetResultBooleanValue(searchResult, "lowCost");
            var acceptsMedicare = this.GetResultBooleanValue(searchResult, "lowCost");
            var acceptsUninsuredPatients = this.GetResultBooleanValue(searchResult, "lowCost");
            var acceptsMedicaid = this.GetResultBooleanValue(searchResult, "lowCost");

            // Contact
            var webLink = this.GetResultValue(searchResult, "website");
            var email = this.GetResultValue(searchResult, "email");
            var twitter = this.GetResultValue(searchResult, "twitter");
            var instagram = this.GetResultValue(searchResult, "instagram");
            var facebook = this.GetResultValue(searchResult, "facebook");
            var primaryPhone = this.GetResultValue(searchResult, "primaryPhone");
            var languagePhones = this.GetLanguagePhones(this.GetResultValue(searchResult, "languagePhones"));
            var crisisPhone = this.GetResultValue(searchResult, "crisisPhone");
            var crisisPhoneInstructions = this.GetResultCultureValueWithFallback(searchResult, "crisisPhoneInstructions");
            var afterHoursPhone = this.GetResultValue(searchResult, "afterHoursPhone");
            var afterHoursPhoneInstructions = this.GetResultCultureValueWithFallback(searchResult, "afterHoursPhoneInstructions");

            // Location
            var address = this.GetResultValue(searchResult, "streetAddress");
            var city = this.GetResultValue(searchResult, "city");
            var stateList = this.GetResultValue(searchResult, "state");
            var state = stateList != null ? JsonConvert.DeserializeObject<string[]>(stateList) : new string[] { };
            var zip = this.GetResultValue(searchResult, "zip");
            var mapInfo = this.GetMapInfo(searchResult, "map");

            // Opening Times
            var statusList = this.GetResultCultureValueWithFallback(searchResult, "status");
            var status = statusList != null ? JsonConvert.DeserializeObject<string[]>(statusList) : new string[] { };
            var statusVal = status.Length > 0 ? status[0] : string.Empty;
            var open = !string.Equals(statusVal, "Temporarily Closed", StringComparison.InvariantCultureIgnoreCase) 
                && !string.Equals(statusVal, "Permanently Closed", StringComparison.InvariantCultureIgnoreCase);
            var openingHours = this.GetOpeningTimes(searchResult);
            var holidays = this.GetResultCultureValueWithFallback(searchResult, "holidays");
            var specialHours = this.GetResultCultureValueWithFallback(searchResult, "specialHours");

            // Populations Served
            var populations = this.GetNodesName(this.GetResultValue(searchResult, "populationsServed"));

            // Languages Supported
            var languages = this.GetNodesName(this.GetResultValue(searchResult, "languagesSupported"));

            // Categories
            var tags = this.GetNodesName(this.GetResultValue(searchResult, "tags"));

            // Options
            var options = searchResult.Values.Where(x => x.Value == "1").Where(x => x.Key != "needsTranslation" && x.Key != "sortOrder").Select(x => x.Key.ToLowerInvariant()).ToList();

            if (!string.IsNullOrEmpty(specialHours))
            {
                options.Add("seniorhours");
            }

            return new SocialServiceResource()
            {
                Id = id,
                Name = serviceName,
                ProviderAddLoc = providerName,
                Description = shortDescription,
                LongDescription = longDescription,
                Eligibility = eligibility,
                ResourceAccessNotes = access,
                Region = region,
                Category = category,
                ClassificationType = classificationType,
                GeographicRestrictions = geographicRestrictions,
                SafeForUndocumentedIndividuals = safeForUndocumentedIndividuals,
                Free = free,
                LowCost = lowCost,
                AcceptsMedicare = acceptsMedicare,
                AcceptsUninsuredPatients = acceptsUninsuredPatients,
                AcceptsMedicaid = acceptsMedicaid,
                WebLink = webLink,
                Email = email,
                Twitter = twitter,
                Facebook = facebook,
                Instagram = instagram,
                Contact = primaryPhone,
                LanguagePhones = languagePhones,
                CrisisPhone = crisisPhone,
                CrisisPhoneInstructions = crisisPhoneInstructions,
                AfterHoursPhone = afterHoursPhone,
                AfterHoursPhoneInstructions = afterHoursPhoneInstructions,
                Address = address,
                City = city,
                State = state.Length > 0 ? state[0] : null,
                Zip = zip,
                Lat = mapInfo?.Lat,
                Lng = mapInfo?.Lng,
                Status = statusVal,
                OpenHours = openingHours.Where(x => x.Hours.Any()).ToList(),
                HolidaysHours = holidays,
                SpecialHours = specialHours,
                PopulationsServed = populations,
                LanguagesSupported = languages,
                Tags = tags,
                Options = options.ToArray(),
                IsOpen = open,
                Icon = icon,
                LastUpdate = updateDate
            };
        }

        public override IEnumerable<IResourceItem> BuildResources(IEnumerable<ISearchResult> searchResults)
        {
            return searchResults.Select(BuildResource);
        }

        public override IEnumerable<IResourceItemBase> BuildResourcesList(IEnumerable<ISearchResult> searchResults)
        {
            return searchResults.Select(BuildResourceListItem);
        }

        public override IContent BuildContent(JToken resourceItem, string resourceTypeAlias, int categoryNodeId)
        {
            var resource = resourceItem.ToObject<SocialServiceResource>();

            var content = this.Create(resource.Name, categoryNodeId, resourceTypeAlias);
            this.SetContentValues(content, resource);

            return content;
        }

        public override IContent BuildContent(JToken resourceItem, IContent content)
        {
            var resource = resourceItem.ToObject<SocialServiceResource>();
            this.SetContentValues(content, resource);

            return content;
        }

        private IResourceItemBase BuildResourceListItem(ISearchResult searchResult)
        {
            if (searchResult == null)
            {
                return null;
            }

            int.TryParse(searchResult.Id, out int id);
            var category = this.GetNodeContentTypeAlias(id);
            var icon = this.GetIcon(searchResult);

            var providerName = this.GetResultCultureValueWithFallback(searchResult, "providerName");
            var serviceName = this.GetResultCultureValueWithFallback(searchResult, "serviceName");

            if (string.IsNullOrEmpty(serviceName))
            {
                if (string.IsNullOrEmpty(providerName))
                {
                    serviceName = this.GetResultCultureValueWithFallback(searchResult, "nodeName");
                }
                else
                {
                    serviceName = providerName;
                    providerName = null;
                }
            }

            var classificationType = this.GetSingleNodeName(this.GetResultValue(searchResult, "classificationType"));
            var region = this.GetNodesName(this.GetResultValue(searchResult, "region"));
            var address = this.GetResultValue(searchResult, "streetAddress");
            var city = this.GetResultValue(searchResult, "city");
            var stateList = this.GetResultValue(searchResult, "state");
            var state = stateList != null ? JsonConvert.DeserializeObject<string[]>(stateList) : new string[] { };
            var zip = this.GetResultValue(searchResult, "zip");
            var mapInfo = this.GetMapInfo(searchResult, "map");
            var tags = this.GetNodesName(searchResult.GetValues("tags").FirstOrDefault());
            var options = searchResult.Values.Where(x => x.Value == "1").Where(x => x.Key != "needsTranslation" && x.Key != "sortOrder").Select(x => x.Key.ToLowerInvariant()).ToList();
            
            if (!string.IsNullOrEmpty(this.GetResultCultureValueWithFallback(searchResult, "specialHours")))
            {
                options.Add("seniorhours");
            }

            var statusList = this.GetResultCultureValueWithFallback(searchResult, "status");
            var status = statusList != null ? JsonConvert.DeserializeObject<string[]>(statusList) : new string[] { };
            var statusVal = status.Length > 0 ? status[0] : string.Empty;
            var open = !string.Equals(statusVal, "Temporarily Closed", StringComparison.InvariantCultureIgnoreCase)
                && !string.Equals(statusVal, "Permanently Closed", StringComparison.InvariantCultureIgnoreCase);

            return new SocialServiceResourceListItem()
            {
                Id = id,
                Name = serviceName,
                ProviderAddLoc = providerName,
                Region = region,
                Category = category,
                ClassificationType = classificationType,
                Address = address,
                City = city,
                State = state.Length > 0 ? state[0] : null,
                Zip = zip,
                Tags = tags,
                Options = options.ToArray(),
                IsOpen = open,
                Icon = icon,
                Lat = mapInfo?.Lat,
                Lng = mapInfo?.Lng,
            };
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
                var phonesArray = JsonConvert.DeserializeObject<JArray>(str);

                foreach (var phoneVal in phonesArray)
                {
                    var language = this.GetSingleNodeName(phoneVal.Value<string>("language"));
                    var phoneNumber = phoneVal.Value<string>("phoneNumber");

                    if (!string.IsNullOrEmpty(language) || !string.IsNullOrEmpty(phoneNumber))
                    {
                        phones.Add(new LanguagePhone { 
                            Language = language,
                            PhoneNumber = phoneNumber
                        });
                    }
                }
            }
            catch (Exception)
            {
                return phones;
            }

            return phones;
        }

        private void SetContentValues(IContent content, SocialServiceResource resource)
        {

        }
    }
}
