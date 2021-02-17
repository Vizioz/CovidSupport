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
    public abstract class ResourceFactoryBase : IResourceFactory
    {
        private UmbracoHelper helper;

        private IContentService contentService;

        private string defaultCulture = "en_US";

        protected string Culture;

        protected ResourceFactoryBase(UmbracoHelper umbracoHelper, IContentService contentService, string culture)
        {
            this.helper = umbracoHelper;
            this.contentService = contentService;
            this.Culture = culture;
        }

        public abstract IResourceItem BuildResource(ISearchResult searchResult);

        public abstract IEnumerable<IResourceItemBase> BuildResourcesList(IEnumerable<ISearchResult> searchResults);

        public abstract IEnumerable<IResourceItem> BuildResources(IEnumerable<ISearchResult> searchResults);

        public abstract IContent BuildContent(JToken resourceItem, string resourceTypeAlias, int categoryNodeId);

        public abstract IContent BuildContent(JToken resourceItem, IContent content);

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

        protected IContent Create(string name, int parentId, string alias)
        {
            return this.contentService.Create(name, parentId, alias);
        }

        protected MapInfo GetMapInfo(ISearchResult searchResult, string mapPropertyAlias)
        {
            var map = this.GetResultValue(searchResult, mapPropertyAlias);
            var mapInfo = map != null ? JsonConvert.DeserializeObject<MapInfo>(map) : new MapInfo();

            return mapInfo;
        }

        protected string GetIcon(ISearchResult searchResult)
        {
            var icon = this.GetResultValue(searchResult, "icon");

            if (!string.IsNullOrEmpty(icon) && icon.StartsWith("icon-fa"))
            {
                return icon.Split(' ')[0].Replace("icon-", string.Empty);
            }
            else
            {
                return null;
            }
        }
    }
}
