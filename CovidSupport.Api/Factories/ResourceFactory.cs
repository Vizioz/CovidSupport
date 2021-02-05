using CovidSupport.Api.Models;
using Examine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Web;

namespace CovidSupport.Api.Factories
{
    public class ResourceFactory : ResourceFactoryBase
    {
        public ResourceFactory(UmbracoHelper helper, string culture) : base(helper, culture)
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
            int.TryParse(this.GetResultValue(searchResult, "parentID"), out int parentId);
            var category = this.GetSingleNodeName(parentId);

            // Provider
            var providerName = this.GetResultValue(searchResult, "providerName");
            var providerAddLoc = this.GetResultValue(searchResult, "providerAddLoc");
            var free = this.GetResultBooleanValue(searchResult, "free");
            var description = this.GetResultValue(searchResult, "cuisine");

            // Location
            var address = this.GetResultValue(searchResult, "address");
            var city = this.GetResultValue(searchResult, "city");
            var region = this.GetNodesName(this.GetResultValue(searchResult, "region"));
            var stateList = this.GetResultValue(searchResult, "state");
            var state = stateList != null ? JsonConvert.DeserializeObject<string[]>(stateList) : new string[] { };
            var zip = this.GetResultValue(searchResult, "zip");
            var map = this.GetResultValue(searchResult, "map");
            var mapInfo = map != null ? JsonConvert.DeserializeObject<MapInfo>(map) : new MapInfo();

            // Opening Times
            var openingHours = new List<OpeningTimes>
            {
                this.GetDayOpeningTimes("monday", this.GetResultValue(searchResult, "monday")),
                this.GetDayOpeningTimes("tuesday", this.GetResultValue(searchResult, "tuesday")),
                this.GetDayOpeningTimes("wednesday", this.GetResultValue(searchResult, "wednesday")),
                this.GetDayOpeningTimes("thursday", this.GetResultValue(searchResult, "thursday")),
                this.GetDayOpeningTimes("friday", this.GetResultValue(searchResult, "friday")),
                this.GetDayOpeningTimes("saturday", this.GetResultValue(searchResult, "saturday")),
                this.GetDayOpeningTimes("sunday", this.GetResultValue(searchResult, "sunday"))
            };

            var specialHours = new List<OpeningTimes>
            {
                this.GetDayOpeningTimes("monday", this.GetResultValue(searchResult, "spMonday")),
                this.GetDayOpeningTimes("tuesday", this.GetResultValue(searchResult, "spTuesday")),
                this.GetDayOpeningTimes("wednesday", this.GetResultValue(searchResult, "spWednesday")),
                this.GetDayOpeningTimes("thursday", this.GetResultValue(searchResult, "spThursday")),
                this.GetDayOpeningTimes("friday", this.GetResultValue(searchResult, "spFriday")),
                this.GetDayOpeningTimes("saturday", this.GetResultValue(searchResult, "spSaturday")),
                this.GetDayOpeningTimes("sunday", this.GetResultValue(searchResult, "spSunday"))
            };

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
            var options = searchResult.Values.Where(x => x.Value == "1").Select(x => x.Key).ToArray();

            return new Resource
            {
                Id = id,
                Name = providerName,
                Description = description,
                Address = address,
                City = city,
                State = state.Length > 0 ? state[0] : null,
                Zip = zip,
                Region = region,
                Category = category,
                Lat = mapInfo?.Lat,
                Lon = mapInfo?.Lng,
                Options = options,
                ProviderAddLoc = providerAddLoc,
                Free = free,
                OpenHours = openingHours.Where(x => x.Hours.Any()).ToList(),
                SpecialHours = specialHours.Where(x => x.Hours.Any()).ToList(),
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

        public override IEnumerable<IResourceItem> BuildResources(IEnumerable<ISearchResult> searchResults)
        {
            return searchResults.Select(BuildResource); 
        }

        public override IEnumerable<IResourceItemBase> BuildResourcesList(IEnumerable<ISearchResult> searchResults)
        {
            return searchResults.Select(BuildResourceListItem);
        }

        private IResourceItemBase BuildResourceListItem(ISearchResult searchResult)
        {
            if (searchResult == null)
            {
                return null;
            }

            int.TryParse(searchResult.Id, out int id);
            int.TryParse(this.GetResultValue(searchResult, "parentID"), out int parentId);
            var category = this.GetSingleNodeName(parentId);

            var providerName = this.GetResultValue(searchResult, "providerName");
            var description = this.GetResultValue(searchResult, "cuisine");
            var address = this.GetResultValue(searchResult, "address");
            var city = this.GetResultValue(searchResult, "city");
            var region = this.GetNodesName(this.GetResultValue(searchResult, "region"));
            var stateList = this.GetResultValue(searchResult, "state");
            var state = stateList != null ? JsonConvert.DeserializeObject<string[]>(stateList) : new string[] { };
            var zip = this.GetResultValue(searchResult, "zip");
            var map = this.GetResultValue(searchResult, "map");
            var mapInfo = map != null ? JsonConvert.DeserializeObject<MapInfo>(map) : new MapInfo();
            var options = searchResult.Values.Where(x => x.Value == "1").Select(x => x.Key).ToArray();

            return new ResourceListItem
            {
                Id = id,
                Name = providerName,
                Description = description,
                Address = address,
                City = city,
                State = state.Length > 0 ? state[0] : null,
                Zip = zip,
                Region = region,
                Category = category,
                Lat = mapInfo?.LatLng?.Length > 0 ? mapInfo.LatLng[0] : (double?)null,
                Lon = mapInfo?.LatLng?.Length > 1 ? mapInfo.LatLng[1] : (double?)null,
                Options = options
            };
        }
    }
}
