using System;

namespace CovidSupport.Api.Models
{
    public class StartEndTime
    {
        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public string StartTimeString => this.StartTime?.ToString("h:mm tt");

        public string EndTimeString => this.EndTime?.ToString("h:mm tt");

        public string StartEndTimeString
        {
            get
            {
                if (!string.IsNullOrEmpty(this.StartTimeString) && !string.IsNullOrEmpty(this.EndTimeString))
                {
                    return this.StartTimeString + " - " + this.EndTimeString;
                }
                else if (!string.IsNullOrEmpty(this.StartTimeString))
                {
                    return "Opens " + this.StartTimeString;
                }
                else if (!string.IsNullOrEmpty(this.EndTimeString))
                {
                    return "Closes " + this.StartTimeString;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
