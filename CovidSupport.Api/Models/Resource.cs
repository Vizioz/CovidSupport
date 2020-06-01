using System.Collections.Generic;
using CovidSupport.Api.Interfaces;

namespace CovidSupport.Api.Models
{
    public class Resource : IResourceItem
    {
        public Resource()
        {
            this.ServiceRegions = new string[] { };
            this.LanguagePhones = new List<LanguagePhone>();
            this.Tags = new string[] { };
            this.LanguagesSupported = new string[] { };
        }

        public int Id { get; set; }

        public string ProviderName { get; set; }

        public string ServiceName { get; set; }

        public string ShortDescription { get; set; }

        public string[] ServiceRegions { get; set; }

        public string StreetAddress { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Zip { get; set; }
        
        public string[] Tags { get; set; }

        public double? Lat { get; set; }

        public double? Lon { get; set; }

        public string LongDescription { get; set; }

        public string Eligibility { get; set; }

        public string GeographicRestrictions { get; set; }

        public bool SafeForUndocumentedIndividuals { get; set; }

        public bool Free { get; set; }

        public bool LowCost { get; set; }

        public string ResourceAccessNotes { get; set; }

        public StartEndTime Monday { get; set; }

        public StartEndTime Tuesday { get; set; }

        public StartEndTime Wednesday { get; set; }

        public StartEndTime Thursday { get; set; }

        public StartEndTime Friday { get; set; }

        public StartEndTime Saturday { get; set; }

        public StartEndTime Sunday { get; set; }

        public string Holidays { get; set; }

        public string SpecialHours { get; set; }

        public string Status { get; set; }

        public string PrimaryPhone { get; set; }

        public IEnumerable<LanguagePhone> LanguagePhones { get; set; }

        public string CrisisPhone { get; set; }

        public string CrisisPhoneInstructions { get; set; }

        public string AfterHoursPhone { get; set; }

        public string AfterHoursPhoneInstructions { get; set; }
        
        public string Website { get; set; }

        public string Email { get; set; }

        public string Twitter { get; set; }

        public string Facebook { get; set; }

        public bool LGBTQIA { get; set; }

        public bool FamiliesWithChildren { get; set; }

        public bool IndividualsWithSpecialNeeds { get; set; }

        public bool Seniors { get; set; }

        public bool Veterans { get; set; }

        public bool CollegeStudents { get; set; }

        public bool ReEnteringIndividuals { get; set; }

        public bool ExperiencingHomelessness { get; set; }

        public bool SurvivorsOfDomesticViolence { get; set; }

        public bool UndocumentedIndividuals { get; set; }
        
        public bool Immigrants { get; set; }

        public string[] LanguagesSupported { get; set; }
        
        public bool AcceptsMedicare { get; set; }

        public bool AcceptsMedicaid { get; set; }

        public bool AcceptsUninsuredPatients { get; set; }
    }
}
