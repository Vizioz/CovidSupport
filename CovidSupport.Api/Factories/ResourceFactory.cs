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
    public class ResourceFactory : ResourceFactoryBase
    {
        public ResourceFactory(UmbracoHelper helper, IContentService contentService, string culture) : base(helper, contentService, culture)
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
            var open = !this.GetResultBooleanValue(searchResult, "businessClosed");

            // Provider
            var providerName = this.GetResultValue(searchResult, "providerName");
            var providerAddLoc = this.GetResultValue(searchResult, "providerAddLoc");
            var free = this.GetResultBooleanValue(searchResult, "free");
            var classificationType = this.GetSingleNodeName(this.GetResultValue(searchResult, "classificationType"));

            if (string.IsNullOrEmpty(classificationType))
            {
                classificationType = this.GetResultValue(searchResult, "cuisine");
            }

            // Location
            var address = this.GetResultValue(searchResult, "address");
            var city = this.GetResultValue(searchResult, "city");
            var region = this.GetRegions(searchResult);
            var stateList = this.GetResultValue(searchResult, "state");
            var state = stateList != null ? JsonConvert.DeserializeObject<string[]>(stateList) : new string[] { };
            var zip = this.GetResultValue(searchResult, "zip");
            var mapInfo = this.GetMapInfo(searchResult, "map");

            // Opening Times
            var openingHours = this.GetOpeningTimes(searchResult).Where(x => x.Hours.Any());
            var specialHours = this.GetOpeningTimes(searchResult, "sp").Where(x => x.Hours.Any());
            var openInfo = this.OpenInfo(openingHours);

            // Contact
            var contact = this.GetResultValue(searchResult, "contact");
            var contactSpanish = this.GetResultValue(searchResult, "contactSpanish");
            var email = this.GetResultValue(searchResult, "email");
            var webLink = this.GetResultValue(searchResult, "webLink");
            var twitter = this.GetResultValue(searchResult, "twitter");
            var instagram = this.GetResultValue(searchResult, "instagram");
            var facebook = this.GetResultValue(searchResult, "facebook");

            // Instructions
            var instructions = this.GetResultCultureValueWithFallback(searchResult, "instructions");
            var offers = this.GetResultCultureValueWithFallback(searchResult, "offers");
            var notes = this.GetResultCultureValueWithFallback(searchResult, "notes");

            // Options
            var options = searchResult.Values.Where(x => x.Value == "1").Where(x => x.Key != "status" &&x.Key != "businessClosed" && x.Key != "sortOrder").Select(x => x.Key.ToLowerInvariant()).ToList();

            if (specialHours.Any())
            {
                options.Add("specialhours");
            }

            return new Resource
            {
                Id = id,
                Name = providerName,
                Address = address,
                City = city,
                State = state.Length > 0 ? state[0] : null,
                Zip = zip,
                Region = region,
                Category = category,
                ClassificationType = classificationType,
                Lat = mapInfo?.Lat,
                Lng = mapInfo?.Lng,
                Options = options.ToArray(),
                IsOpen = open,
                Icon = icon,
                ProviderAddLoc = providerAddLoc,
                Free = free,
                OpenHours = openingHours.ToList(),
                SpecialHours = specialHours.ToList(),
                Contact = contact,
                ContactSpanish = contactSpanish,
                Email = email,
                WebLink = webLink,
                Twitter = twitter,
                Instagram = instagram,
                Facebook = facebook,
                Instructions = instructions,
                Offers = offers,
                Notes = notes,
                LastUpdate = updateDate,
                OpenInfo = openInfo
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
            var resource = resourceItem.ToObject<Resource>();

            var content = this.Create(resource.Name, categoryNodeId, resourceTypeAlias);
            this.SetContentValues(content, resource);

            return content;
        }

        public override IContent BuildContent(JToken resourceItem, IContent content)
        {
            var resource = resourceItem.ToObject<Resource>();
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
            var open = !this.GetResultBooleanValue(searchResult, "businessClosed");

            var providerName = this.GetResultValue(searchResult, "providerName");
            var providerAddLoc = this.GetResultValue(searchResult, "providerAddLoc");
            var classificationType = this.GetSingleNodeName(this.GetResultValue(searchResult, "classificationType"));

            if (string.IsNullOrEmpty(classificationType))
            {
                classificationType = this.GetResultValue(searchResult, "cuisine");
            }
            
            var address = this.GetResultValue(searchResult, "address");
            var city = this.GetResultValue(searchResult, "city");
            var region = this.GetRegions(searchResult);
            var stateList = this.GetResultValue(searchResult, "state");
            var state = stateList != null ? JsonConvert.DeserializeObject<string[]>(stateList) : new string[] { };
            var zip = this.GetResultValue(searchResult, "zip");
            var mapInfo = this.GetMapInfo(searchResult, "map");
            var options = searchResult.Values.Where(x => x.Value == "1").Where(x => x.Key != "status" && x.Key != "businessClosed" && x.Key != "sortOrder").Select(x => x.Key.ToLowerInvariant()).ToList();

            var openingHours = this.GetOpeningTimes(searchResult).Where(x => x.Hours.Any());
            var specialHours = this.GetOpeningTimes(searchResult, "sp").Where(x => x.Hours.Any());
            var openInfo = this.OpenInfo(openingHours);

            if (specialHours.Any())
            {
                options.Add("specialhours");
            }

            return new ResourceListItem
            {
                Id = id,
                Name = providerName,
                ProviderAddLoc = providerAddLoc,
                Address = address,
                City = city,
                State = state.Length > 0 ? state[0] : null,
                Zip = zip,
                Region = region,
                Category = category,
                ClassificationType = classificationType,
                Lat = mapInfo?.LatLng?.Length > 0 ? mapInfo.LatLng[0] : (double?)null,
                Lng = mapInfo?.LatLng?.Length > 1 ? mapInfo.LatLng[1] : (double?)null,
                Options = options.ToArray(),
                IsOpen = open,
                Icon = icon,
                OpenInfo = openInfo
            };
        }

        private void SetContentValues(IContent content, Resource resource)
        {
            this.SetPropertyValue(content, "providerName", resource.Name);
            this.SetPropertyValue(content, "providerAddLoc", resource.ProviderAddLoc);
            this.SetPropertyValue(content, "free", resource.Free);

            this.SetPropertyValue(content, "address", resource.Address);
            this.SetPropertyValue(content, "city", resource.City);
            this.SetPropertyValue(content, "region", resource.Region); // TODO
            this.SetPropertyValue(content, "state", resource.State); // TODO
            this.SetPropertyValue(content, "zip", resource.Zip);
            this.SetPropertyValue(content, "map", resource.Lat); // TODO

            this.SetPropertyValue(content, "monday", resource.OpenHours[0]); // TODO
            this.SetPropertyValue(content, "tuesday", resource.OpenHours[0]); // TODO
            this.SetPropertyValue(content, "wednesday", resource.OpenHours[0]); // TODO
            this.SetPropertyValue(content, "thursday", resource.OpenHours[0]); // TODO
            this.SetPropertyValue(content, "friday", resource.OpenHours[0]); // TODO
            this.SetPropertyValue(content, "saturday", resource.OpenHours[0]); // TODO
            this.SetPropertyValue(content, "sunday", resource.OpenHours[0]); // TODO
            this.SetPropertyValue(content, "spMonday", resource.OpenHours[0]); // TODO
            this.SetPropertyValue(content, "spTuesday", resource.OpenHours[0]); // TODO
            this.SetPropertyValue(content, "spWednesday", resource.OpenHours[0]); // TODO
            this.SetPropertyValue(content, "spThursday", resource.OpenHours[0]); // TODO
            this.SetPropertyValue(content, "spFriday", resource.OpenHours[0]); // TODO
            this.SetPropertyValue(content, "spSaturday", resource.OpenHours[0]); // TODO
            this.SetPropertyValue(content, "spSunday", resource.OpenHours[0]); // TODO

            this.SetPropertyValue(content, "contact", resource.Contact);
            this.SetPropertyValue(content, "contactSpanish", resource.ContactSpanish);
            this.SetPropertyValue(content, "email", resource.Email);
            this.SetPropertyValue(content, "webLink", resource.WebLink);
            this.SetPropertyValue(content, "twitter", resource.Twitter);
            this.SetPropertyValue(content, "instagram", resource.Instagram);
            this.SetPropertyValue(content, "facebook", resource.Facebook);

            this.SetPropertyValue(content, "instructions", resource.Instructions);
            this.SetPropertyValue(content, "offers", resource.Offers);
            this.SetPropertyValue(content, "notes", resource.Notes);

            this.SetPropertyValue(content, "options", resource.Options.FirstOrDefault(x => x == "Options") != null); // TODO
        }

        private void SetPropertyValue(IContent content, string propertyAlias, object value, string culture = null)
        {
            if (content.HasProperty(propertyAlias))
            {
                content.SetValue(propertyAlias, value, culture);
            }
        }
    }
}
