using System.Collections.Generic;

namespace CovidSupport.Api.Models
{
    public class OpeningTimes
    {
        public OpeningTimes()
        {
            this.Hours = new List<StartEndTime>();
        }

        public OpeningTimes(string day)
        {
            this.Day = day;
            this.Hours = new List<StartEndTime>();
        }

        public string Day { get; set; }

        public IEnumerable<StartEndTime> Hours { get; set; }
    }
}
