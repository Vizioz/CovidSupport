using System;
using System.Collections.Generic;

namespace CovidSupport.Api.Models
{
    public class ResourceListItem
    {
        public ResourceListItem()
        {
            this.Options = new string[] { };
        }

        public int Id { get; set; }

        public string ProviderName { get; set; }

        public string Description { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Zip { get; set; }

        public string Region { get; set; }

        public double? Lat { get; set; }

        public double? Lon { get; set; }

        public string[] Options { get; set; }

        public string ShortAddress
        {
            get
            {
                var values = new List<string>();

                if (!string.IsNullOrEmpty(this.Address))
                {
                    values.Add(this.Address);
                }

                if (!string.IsNullOrEmpty(this.City))
                {
                    values.Add(this.City);
                }

                return string.Join(", ", values);
            }
        }

        public string FullAddress
        {
            get
            {
                var values = new List<string>();

                if (!string.IsNullOrEmpty(this.Address))
                {
                    values.Add(this.Address);
                }

                if (!string.IsNullOrEmpty(this.City))
                {
                    values.Add(this.City);
                }

                if (!string.IsNullOrEmpty(this.State) || !string.IsNullOrEmpty(this.Zip))
                {
                    var state = new List<string>();

                    if (!string.IsNullOrEmpty(this.State))
                    {
                        state.Add(this.State);
                    }

                    if (!string.IsNullOrEmpty(this.Zip))
                    {
                        state.Add(this.Zip);
                    }

                    values.Add(string.Join(" ", state));
                }

                return string.Join(", ", values);
            }
        }
    }
}
