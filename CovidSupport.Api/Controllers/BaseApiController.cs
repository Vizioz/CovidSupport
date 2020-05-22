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
            var allDomains = this.Services.DomainService.GetAll(true);


            var host = this.ApplicationUrl.Host.Trim('/');
            var domain = this.Services.DomainService.GetAll(true).FirstOrDefault(d => d.DomainName.Trim('/') == host);

            if (domain != null)
            {
                var websiteId = domain.RootContentId ?? (int)default;
                var website = this.Umbraco.Content(websiteId);

                if (website != null)
                {
                    this.Website = website;
                    this.WebsiteUrl = host;
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
    }
}
