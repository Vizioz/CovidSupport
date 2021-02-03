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
            int.TryParse(searchResult.GetValues("parentID").FirstOrDefault(), out int parentId);
            var category = this.GetSingleNodeName(parentId);

            // Provider
            var providerName = searchResult.GetValues("providerName").FirstOrDefault();
            var providerAddLoc = searchResult.GetValues("providerAddLoc").FirstOrDefault();
            var free = searchResult.GetValues("free").FirstOrDefault() == "1";
            var description = searchResult.GetValues("cuisine").FirstOrDefault();

            // Location
            var address = searchResult.GetValues("address").FirstOrDefault();
            var city = searchResult.GetValues("city").FirstOrDefault();
            var region = this.GetNodesName(searchResult.GetValues("region").FirstOrDefault());
            var stateList = searchResult.GetValues("state").FirstOrDefault();
            var state = stateList != null ? JsonConvert.DeserializeObject<string[]>(stateList) : new string[] { };
            var zip = searchResult.GetValues("zip").FirstOrDefault();            
            var map = searchResult.GetValues("map").FirstOrDefault();
            var mapInfo = map != null ? JsonConvert.DeserializeObject<MapInfo>(map) : new MapInfo();

            // Opening Times
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

            // Contact
            var contact = searchResult.GetValues("contact").FirstOrDefault();
            var contactSpanish = searchResult.GetValues("contactSpanish").FirstOrDefault();
            var email = searchResult.GetValues("email").FirstOrDefault();
            var webLink = searchResult.GetValues("webLink").FirstOrDefault();
            var twitter = searchResult.GetValues("twitter").FirstOrDefault();
            var instagram = searchResult.GetValues("instagram").FirstOrDefault();
            var facebook = searchResult.GetValues("facebook").FirstOrDefault();

            // Instructions
            var instructions = searchResult.GetValues("instructions_" + this.Culture).FirstOrDefault();
            var offers = searchResult.GetValues("offers_" + this.Culture).FirstOrDefault();
            var notes = searchResult.GetValues("notes_" + this.Culture).FirstOrDefault();

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
            int.TryParse(searchResult.GetValues("parentID").FirstOrDefault(), out int parentId);
            var category = this.GetSingleNodeName(parentId);

            var providerName = searchResult.GetValues("providerName").FirstOrDefault();
            var description = searchResult.GetValues("cuisine").FirstOrDefault();
            var address = searchResult.GetValues("address").FirstOrDefault();
            var city = searchResult.GetValues("city").FirstOrDefault();
            var region = this.GetNodesName(searchResult.GetValues("region").FirstOrDefault());
            var stateList = searchResult.GetValues("state").FirstOrDefault();
            var state = stateList != null ? JsonConvert.DeserializeObject<string[]>(stateList) : new string[] { };
            var zip = searchResult.GetValues("zip").FirstOrDefault();
            var map = searchResult.GetValues("map").FirstOrDefault();
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
