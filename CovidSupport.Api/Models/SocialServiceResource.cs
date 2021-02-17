using System.Collections.Generic;

namespace CovidSupport.Api.Models
{
    public class SocialServiceResource : IResourceItem
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Zip { get; set; }

        public string[] Region { get; set; }

        public string Category { get; set; }

        public string[] Options { get; set; }

        public string Icon { get; set; }

        public double? Lat { get; set; }

        public double? Lon { get; set; }

        public List<OpeningTimes> OpenHours { get; set; }

        public string Contact { get; set; }

        public string Email { get; set; }

        public string WebLink { get; set; }

        public string Twitter { get; set; }

        public string Instagram { get; set; }

        public string Facebook { get; set; }

        public bool Free { get; set; }

        public string ServiceProviderName { get; set; }

        public string LongDescription { get; set; }

        public string Eligibility { get; set; }

        public string ResourceAccessNotes { get; set; }

        public string GeographicRestrictions { get; set; }

        public bool SafeForUndocumentedIndividuals { get; set; }

        public bool LowCost { get; set; }

        public bool AcceptsMedicare { get; set; }

        public bool AcceptsUninsuredPatients { get; set; }

        public bool AcceptsMedicaid { get; set; }

        public string CrisisPhone { get; set; }

        public string CrisisPhoneInstructions { get; set; }

        public string AfterHoursPhone { get; set; }

        public string AfterHoursPhoneInstructions { get; set; }

        public IEnumerable<LanguagePhone> LanguagePhones { get; set; }

        public string Status { get; set; }

        public string HolidaysOpeningTimes { get; set; }

        public string SpecialHoursOpeningTimes { get; set; }

        public IEnumerable<string> PopulationsServed { get; set; }

        public IEnumerable<string> LanguagesSupported { get; set; }

        public IEnumerable<string> Tags { get; set; }
    }
}
