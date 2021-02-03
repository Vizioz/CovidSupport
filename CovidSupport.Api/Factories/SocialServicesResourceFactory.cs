using CovidSupport.Api.Models;
using Examine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Web;

namespace CovidSupport.Api.Factories
{
    public class SocialServicesResourceFactory : ResourceFactoryBase
    {
        public SocialServicesResourceFactory(UmbracoHelper helper, string culture) : base(helper, culture)
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
            var providerName = searchResult.GetValues("providerName_" + this.Culture).FirstOrDefault();
            var serviceName = searchResult.GetValues("serviceName_" + this.Culture).FirstOrDefault();
            var shortDescription = searchResult.GetValues("shortDescription_" + this.Culture).FirstOrDefault();
            var longDescription = searchResult.GetValues("longDescription_" + this.Culture).FirstOrDefault();

            // Access
            var eligibility = searchResult.GetValues("eligibility_" + this.Culture).FirstOrDefault();
            var access = searchResult.GetValues("resourceAccessNotes_" + this.Culture).FirstOrDefault();
            var region = this.GetNodesName(searchResult.GetValues("region").FirstOrDefault());
            var geographicRestrictions = searchResult.GetValues("geographicRestrictions_" + this.Culture).FirstOrDefault();
            var safeForUndocumentedIndividuals = searchResult.GetValues("safeForUndocumentedIndividuals").FirstOrDefault() == "1";
            var free = searchResult.GetValues("free").FirstOrDefault() == "1";
            var lowCost = searchResult.GetValues("lowCost").FirstOrDefault() == "1";
            var acceptsMedicare = searchResult.GetValues("lowCost").FirstOrDefault() == "1";
            var acceptsUninsuredPatients = searchResult.GetValues("lowCost").FirstOrDefault() == "1";
            var acceptsMedicaid = searchResult.GetValues("lowCost").FirstOrDefault() == "1";

            // Contact
            var webLink = searchResult.GetValues("website").FirstOrDefault();
            var email = searchResult.GetValues("email").FirstOrDefault();
            var twitter = searchResult.GetValues("twitter").FirstOrDefault();
            var instagram = searchResult.GetValues("instagram").FirstOrDefault();
            var facebook = searchResult.GetValues("facebook").FirstOrDefault();
            var primaryPhone = searchResult.GetValues("primaryPhone").FirstOrDefault();
            var languagePhones = this.GetLanguagePhones(searchResult.GetValues("languagePhones").FirstOrDefault());
            var crisisPhone = searchResult.GetValues("crisisPhone").FirstOrDefault();
            var crisisPhoneInstructions = searchResult.GetValues("crisisPhoneInstructions_" + this.Culture).FirstOrDefault();
            var afterHoursPhone = searchResult.GetValues("afterHoursPhone").FirstOrDefault();
            var afterHoursPhoneInstructions = searchResult.GetValues("afterHoursPhoneInstructions_" + this.Culture).FirstOrDefault();

            // Location
            var address = searchResult.GetValues("streetAddress").FirstOrDefault();
            var city = searchResult.GetValues("city").FirstOrDefault();
            var stateList = searchResult.GetValues("state").FirstOrDefault();
            var state = stateList != null ? JsonConvert.DeserializeObject<string[]>(stateList) : new string[] { };
            var zip = searchResult.GetValues("zip").FirstOrDefault();

            // Opening Times
            var statusList = searchResult.GetValues("status").FirstOrDefault();
            var status = statusList != null ? JsonConvert.DeserializeObject<string[]>(statusList)[0] : string.Empty;

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

            var holidays = searchResult.GetValues("holidays_" + this.Culture).FirstOrDefault();
            var specialHours = searchResult.GetValues("specialHours_" + this.Culture).FirstOrDefault();

            // Populations Served
            var populations = this.GetNodesName(searchResult.GetValues("populationsServed").FirstOrDefault());

            // Languages Supported
            var languages = this.GetNodesName(searchResult.GetValues("languagesSupported").FirstOrDefault());

            // Categories
            var tags = this.GetNodesName(searchResult.GetValues("tags").FirstOrDefault());

            return new SocialServiceResource()
            {
                Id = id,
                Name = serviceName,
                ServiceProviderName = providerName,
                Description = shortDescription,
                LongDescription = longDescription,
                Eligibility = eligibility,
                ResourceAccessNotes = access,
                Region = region,
                Category = category,
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
                Status = status,
                OpenHours = openingHours.Where(x => x.Hours.Any()).ToList(),
                HolidaysOpeningTimes = holidays,
                SpecialHoursOpeningTimes = specialHours,
                PopulationsServed = populations,
                LanguagesSupported = languages,
                Tags = tags
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

            var serviceName = searchResult.GetValues("serviceName_" + this.Culture).FirstOrDefault();
            var shortDescription = searchResult.GetValues("shortDescription_" + this.Culture).FirstOrDefault();
            var region = this.GetNodesName(searchResult.GetValues("region").FirstOrDefault());
            var address = searchResult.GetValues("streetAddress").FirstOrDefault();
            var city = searchResult.GetValues("city").FirstOrDefault();
            var stateList = searchResult.GetValues("state").FirstOrDefault();
            var state = stateList != null ? JsonConvert.DeserializeObject<string[]>(stateList) : new string[] { };
            var zip = searchResult.GetValues("zip").FirstOrDefault();
            var tags = this.GetNodesName(searchResult.GetValues("tags").FirstOrDefault());

            return new SocialServiceResourceListItem()
            {
                Id = id,
                Name = serviceName,
                Description = shortDescription,
                Region = region,
                Category = category,
                Address = address,
                City = city,
                State = state.Length > 0 ? state[0] : null,
                Zip = zip,
                Tags = tags
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
    }
}
