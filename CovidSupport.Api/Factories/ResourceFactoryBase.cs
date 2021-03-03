using CovidSupport.Api.Models;
using Examine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
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

        protected IEnumerable<OpeningTimes> GetOpeningTimes(ISearchResult searchResult, string aliasPrefix = "")
        {
            var monLabel = !string.IsNullOrEmpty(aliasPrefix) ? aliasPrefix + "Monday" : "monday";
            var tuesLabel = !string.IsNullOrEmpty(aliasPrefix) ? aliasPrefix + "Tuesday" : "tuesday";
            var wedLabel = !string.IsNullOrEmpty(aliasPrefix) ? aliasPrefix + "Wednesday" : "wednesday";
            var thursLabel = !string.IsNullOrEmpty(aliasPrefix) ? aliasPrefix + "Thursday" : "thursday";
            var friLabel = !string.IsNullOrEmpty(aliasPrefix) ? aliasPrefix + "Friday" : "friday";
            var satLabel = !string.IsNullOrEmpty(aliasPrefix) ? aliasPrefix + "Saturday" : "saturday";
            var sunLabel = !string.IsNullOrEmpty(aliasPrefix) ? aliasPrefix + "Sunday" : "sunday";

            var openingHours = new List<OpeningTimes>
            {
                this.GetDayOpeningTimes("monday", this.GetResultValue(searchResult, monLabel)),
                this.GetDayOpeningTimes("tuesday", this.GetResultValue(searchResult, tuesLabel)),
                this.GetDayOpeningTimes("wednesday", this.GetResultValue(searchResult, wedLabel)),
                this.GetDayOpeningTimes("thursday", this.GetResultValue(searchResult, thursLabel)),
                this.GetDayOpeningTimes("friday", this.GetResultValue(searchResult, friLabel)),
                this.GetDayOpeningTimes("saturday", this.GetResultValue(searchResult, satLabel)),
                this.GetDayOpeningTimes("sunday", this.GetResultValue(searchResult, sunLabel))
            };

            return openingHours;
        }

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
                    var title = openingHoursVal.Value<string>("title");
                    var startTime = openingHoursVal.Value<DateTime?>("startTime");
                    var endTime = openingHoursVal.Value<DateTime?>("endTime");

                    if (startTime != null || endTime != null)
                    {
                        var startEndTime = new StartEndTime { 
                            Title = title
                        };

                        if (startTime != null)
                        {
                            startEndTime.StartTime = ((DateTime)startTime).ToString("HH:mm:ss");
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

            return ids.Select(id => this.GetNode(id)).Where(x => x != null).Select(x => x.Name).ToArray();
        }

        protected string GetSingleNodeName(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }

            return this.GetNode(str.Split(',')[0])?.Name;
        }

        protected string GetSingleNodeName(int id)
        {
            if (id <= 0)
            {
                return string.Empty;
            }

            return this.GetNode(id)?.Name;
        }

        protected string GetNodeContentTypeAlias(int id)
        {
            if (id <= 0)
            {
                return string.Empty;
            }

            return this.GetNode(id)?.ContentType.Alias;
        }

        protected IPublishedContent GetNode(int id)
        {
            return this.helper.Content(id);
        }

        protected IPublishedContent GetNode(string id)
        {
            return this.helper.Content(id);
        }

        protected Region[] GetRegions(ISearchResult searchResult)
        {
            var ids = this.GetResultValue(searchResult, "region")?.Split(',') ?? new string[] { };
            var regionNodes = ids.Select(id => this.GetNode(id)).Where(x => x != null);
            var regions = regionNodes.Select(x => new Region { Name = x.Name, Id = x.Value<string>("regionId") });

            return regions.ToArray();
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
            var icon = this.GetResourceTypeIcon(searchResult);

            if (string.IsNullOrEmpty(icon))
            {
                icon = this.GetDocumentTypeIcon(searchResult);
            }

            return icon;
        }

        private string GetResourceTypeIcon(ISearchResult searchResult)
        {
            var classificationType = this.GetResultValue(searchResult, "classificationType");

            if (!string.IsNullOrEmpty(classificationType))
            {
                var content = this.helper.Content(classificationType);
                var icon = content.Value("classificationIcon");
                object iconValue = icon?.GetType().GetProperty("ClassName")?.GetValue(icon, null);

                return this.GetFaIcon(iconValue.ToString());
            }

            return null;
        }

        private string GetDocumentTypeIcon(ISearchResult searchResult)
        {
            var icon = this.GetResultValue(searchResult, "icon");

            return this.GetFaIcon(icon);
        }

        private string GetFaIcon(string icon)
        {
            if (!string.IsNullOrEmpty(icon) && icon.StartsWith("icon-"))
            {
                return icon.Split(' ')[0].Replace("icon-", string.Empty);
            }
            else
            {
                return icon;
            }
        }
    }
}
