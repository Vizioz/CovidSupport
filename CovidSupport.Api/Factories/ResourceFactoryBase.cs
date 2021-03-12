using CovidSupport.Api.Models;
using Examine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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

        protected OpenInfo OpenInfo(IEnumerable<OpeningTimes> openingTimes)
        {
            if (!openingTimes.Any())
            {
                return null;
            }

            var i = 0;
            var timeUtc = DateTime.UtcNow;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            var easternTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);
            return this.NextOpeningTimes(openingTimes, easternTime, ref i);
        }

        private OpenInfo NextOpeningTimes(IEnumerable<OpeningTimes> openingTimes, DateTime now, ref int i)
        {
            if (!openingTimes.Any() || i > 7)
            {
                i = 0;
                return null;
            }

            var day = openingTimes.FirstOrDefault(x =>
                x.Day.Equals(now.DayOfWeek.ToString(), StringComparison.InvariantCultureIgnoreCase));

            bool isOpen = false;
            var openTime = day != null ? this.OpenTime(now, day, out isOpen) : null;

            if (!string.IsNullOrEmpty(openTime))
            {
                return new OpenInfo
                {
                    IsOpenNow = i == 0 && isOpen,
                    OpenTime = openTime,
                    OpenDay = i == 0 ? "today" : i == 1 ? "tomorrow" : day.Day
                };
            }
            else
            {
                i++;
                var nextDay = now.AddDays(1).Date;
                return this.NextOpeningTimes(openingTimes, nextDay, ref i);
            }
        }

        private string OpenTime(DateTime now, OpeningTimes times, out bool isOpen)
        {
            isOpen = false;

            if (times == null && !times.Hours.Any())
            {
                return null;
            }

            foreach (var hour in times.Hours)
            {
                if (!string.IsNullOrEmpty(hour.StartTime) && !string.IsNullOrEmpty(hour.EndTime))
                {
                    int startHour, startMinute;
                    var splitStart = hour.StartTime.Split(':');
                    int.TryParse(splitStart[0], out startHour);
                    int.TryParse(splitStart[1], out startMinute);
                    var start = new DateTime(now.Year, now.Month, now.Day, startHour, startMinute, 0);

                    if (start > now)
                    {
                        isOpen = false;
                        return hour.StartTime;
                    }

                    int endHour, endMinute;
                    var splitEnd = hour.EndTime.Split(':');
                    int.TryParse(splitEnd[0], out endHour);
                    int.TryParse(splitEnd[1], out endMinute);
                    var end = new DateTime(now.Year, now.Month, now.Day, endHour, endMinute, 0);

                    if (end > now)
                    {
                        isOpen = true;
                        return hour.StartTime;
                    }
                }
            }

            return null;
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
