using System;
using System.Linq;
using System.Web.Http;
using Examine;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.WebApi;

namespace CovidSupport.Api.Controllers
{
    public abstract class BaseApiController : UmbracoApiController
    {
        protected IPublishedContent Website { get; private set; }
        
        protected string WebsiteUrl { get; private set; }

        protected string ResourcesIndexName { get; private set; }

        protected IIndex Index { get; private set; }

        protected HttpConfiguration FormatterConfiguration { get; private set; }
        
        protected BaseApiController()
        {
            this.SetConfiguration();
            this.SetWebsiteProvider();
        }
        
        protected ISearcher Searcher => this.Index.GetSearcher();

        private void SetWebsiteProvider()
        {
            var host = this.ApplicationUrl.Host.Trim('/');
            this.WebsiteUrl = host;

            var domain = this.Services.DomainService.GetAll(true)
                .FirstOrDefault(x => string.Equals(this.TrimDomainName(x.DomainName), host, StringComparison.InvariantCultureIgnoreCase));

            if (domain != null)
            {
                var websiteId = domain.RootContentId ?? (int)default;
                var website = this.Umbraco.Content(websiteId);

                if (website != null)
                {
                    this.Website = website;
                    this.ResourcesIndexName = "CommunityResourceIndex-" + website.Name;

                    if (ExamineManager.Instance.TryGetIndex(this.ResourcesIndexName, out var index))
                    {
                        this.Index = index;
                    }
                }
            }
        }

        private void SetConfiguration()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter());
            config.Formatters.JsonFormatter.UseDataContractJsonSerializer = false;
            this.FormatterConfiguration = config;
        }

        private string TrimDomainName(string domain)
        {
            var https = "https://";
            var indexHttps = domain.IndexOf(https, StringComparison.OrdinalIgnoreCase);

            if (indexHttps == 0)
            {
                domain = domain.Remove(indexHttps, https.Length);
            }

            var http = "http://";
            var indexHttp = domain.IndexOf(http, StringComparison.OrdinalIgnoreCase);

            if (indexHttp == 0)
            {
                domain = domain.Remove(indexHttp, http.Length);
            }

            return domain.Trim('/');
        }
    }
}
