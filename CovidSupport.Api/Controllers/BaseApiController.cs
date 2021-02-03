using System;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Cors;
using CovidSupport.Api.Constants;
using Examine;
using Newtonsoft.Json;
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

        protected string ApiLanguage { get; set; }

        protected string CultureName { get; set; }

        protected string ResourcesIndexName { get; private set; }

        protected IIndex Index { get; private set; }
                
        protected ISearcher Searcher => this.Index.GetSearcher();

        protected readonly IVariationContextAccessor _variationContextAccessor;

        protected BaseApiController(IVariationContextAccessor variationContextAccessor)
        {
            _variationContextAccessor = variationContextAccessor;
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            this.SetConfiguration();
            this.SetWebsiteProvider();
        }

        private void SetWebsiteProvider()
        {
            var uri = this.ControllerContext.Request.RequestUri;
            var apiUrl = uri.Authority.Trim('/');

            if (uri.Segments.Length > 1)
            {
                var segment = uri.Segments[1].Trim('/');

                if (segment != ApiConstants.ApiName)
                {
                    this.ApiLanguage = segment;
                    apiUrl = apiUrl + "/" + this.ApiLanguage;
                }
            }

            var d = this.Services.DomainService.GetAll(true);
            var domain = this.Services.DomainService.GetAll(true)
                .FirstOrDefault(x => string.Equals(this.TrimDomainName(x.DomainName), apiUrl, StringComparison.InvariantCultureIgnoreCase));

            if (domain == null)
            {
                throw new Exception($"No domain found for {apiUrl}");
            }

            this.WebsiteUrl = apiUrl;
            this.CultureName = domain.LanguageIsoCode.ToLowerInvariant();

            Thread.CurrentThread.CurrentCulture = new CultureInfo(this.CultureName);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(this.CultureName);
            _variationContextAccessor.VariationContext = new VariationContext(this.CultureName);

            var websiteId = domain.RootContentId ?? (int)default;
            var website = this.Umbraco.Content(websiteId);

            if (website == null)
            {
                throw new Exception($"No website root found for {apiUrl}");
            }

            this.Website = website;
            this.ResourcesIndexName = "CommunityResourceIndex-" + website.Name;

            if (ExamineManager.Instance.TryGetIndex(this.ResourcesIndexName, out var index))
            {
                this.Index = index;
            }
        }

        private void SetConfiguration()
        {
            this.Configuration.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            this.Configuration.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter());
            this.Configuration.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            this.Configuration.Formatters.JsonFormatter.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;            
            this.Configuration.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

            //this.Configuration.Formatters.JsonFormatter.UseDataContractJsonSerializer = true;
            //this.Configuration.Formatters.Remove(GlobalConfiguration.Configuration.Formatters.XmlFormatter);
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
