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
    public abstract class ResourceFactoryBase : IResourceFactory
    {
        private UmbracoHelper helper;

        private string defaultCulture = "en_US";

        protected string Culture;

        protected ResourceFactoryBase(UmbracoHelper umbracoHelper, string culture)
        {
            this.helper = umbracoHelper;
            this.Culture = culture;
        }

        public abstract IResourceItem BuildResource(ISearchResult searchResult);

        public abstract IEnumerable<IResourceItemBase> BuildResourcesList(IEnumerable<ISearchResult> searchResults);

        public abstract IEnumerable<IResourceItem> BuildResources(IEnumerable<ISearchResult> searchResults);

        protected OpeningTimes GetDayOpeningTimes(string day, string str)
        {
            var openingTimes = new OpeningTimes(day);

            if (string.IsNullOrEmpty(str))
            {
                return openingTimes;
            }

            try
            {
                var hours = new List<StartEndTime>();
                var openingHoursArray = JsonConvert.DeserializeObject<JArray>(str);//.FirstOrDefault();

                foreach (var openingHoursVal in openingHoursArray)
                {
                    var startTime = openingHoursVal.Value<DateTime?>("startTime");
                    var endTime = openingHoursVal.Value<DateTime?>("endTime");

                    if (startTime != null || endTime != null)
                    {
                        var startEndTime = new StartEndTime();

                        if (startTime != null)
                        {
                            startEndTime.StarTime = ((DateTime)startTime).ToString("HH:mm:ss");
                        }

                        if (endTime != null)
                        {
                            startEndTime.EndTime = ((DateTime)endTime).ToString("HH:mm:ss");
                        }

                        hours.Add(startEndTime);
                    }
                }

                openingTimes.Hours = hours;
            }
            catch (Exception)
            {
                return openingTimes;
            }

            return openingTimes;
        }

        protected string[] GetNodesName(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return new string[] { };
            }

            var ids = str.Split(',');

            return ids.Select(id => this.helper.Content(id)).Where(x => x != null).Select(x => x.Name).ToArray();
        }

        protected string GetSingleNodeName(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }

            return this.helper.Content(str.Split(',')[0])?.Name;
        }

        protected string GetSingleNodeName(int id)
        {
            if (id <= 0)
            {
                return string.Empty;
            }

            return this.helper.Content(id)?.Name;
        }

        protected string GetResultValue (ISearchResult searchResult, string property)
        {
            return searchResult.GetValues(property).FirstOrDefault();
        }

        protected bool GetResultBooleanValue(ISearchResult searchResult, string property)
        {
            return searchResult.GetValues(property).FirstOrDefault() == "1";
        }

        protected string GetResultCultureValueWithFallback(ISearchResult searchResult, string property)
        {
            var retVal = searchResult.GetValues(property + "_" + this.Culture).FirstOrDefault();

            if (retVal == null && this.Culture != defaultCulture)
            {
                retVal = searchResult.GetValues(property + "_" + defaultCulture).FirstOrDefault();
            }

            if (retVal == null && this.Culture != defaultCulture)
            {
                retVal = this.GetResultValue(searchResult, property);
            }

            return retVal;
        }
    }
}
