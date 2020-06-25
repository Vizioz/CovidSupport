using System;
using System.Collections.Generic;
using System.Linq;
using CovidSupport.Core.Models;
using Examine;
using Lucene.Net.Analysis.Standard;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Examine;
using Umbraco.Web;
using Umbraco.Web.Search;

namespace CovidSupport.Core.Components.Examine
{
    public class ResourceIndexCreator : LuceneIndexCreator, IUmbracoIndexesCreator
    {
        protected IProfilingLogger ProfilingLogger { get; }

        protected ILocalizationService LanguageService { get; }

        protected IPublicAccessService PublicAccessService { get; }

        protected IUmbracoContextFactory UmbracoContext { get; }

        protected IContentTypeService ContentTypeService { get; }

        public ResourceIndexCreator(IProfilingLogger profilingLogger, ILocalizationService languageService,
            IPublicAccessService publicAccessService, IUmbracoContextFactory context, IContentTypeService contentTypeService)
        {
            this.ProfilingLogger = profilingLogger ?? throw new System.ArgumentNullException(nameof(profilingLogger));
            this.LanguageService = languageService ?? throw new System.ArgumentNullException(nameof(languageService));
            this.PublicAccessService = publicAccessService ?? throw new System.ArgumentNullException(nameof(publicAccessService));
            this.UmbracoContext = context ?? throw new System.ArgumentNullException(nameof(context));
            this.ContentTypeService = contentTypeService ?? throw new System.ArgumentNullException(nameof(contentTypeService));
        }

        public override IEnumerable<IIndex> Create()
        {
            var indexes = new List<IIndex>();
            var siteNodes = this.GetSiteNodes();

            foreach (var site in siteNodes)
            {
                indexes.Add(this.CreateWebsiteResourceIndex(site));
            }

            return indexes;
        }

        public IIndex Create(IContent content)
        {
            var siteNode = this.GetSiteNode(content);

            if (!this.ResourceIndexExists(siteNode.WebsiteName))
            {
                return this.CreateWebsiteResourceIndex(siteNode);
            }
            
            throw new Exception("Resource index for " + siteNode.WebsiteName + " already exists.");
        }

        public virtual IContentValueSetValidator GetPublishedContentValueSetValidator(int siteId)
        {
            IEnumerable<string> includeItems = null;

            var resourcesContainer = this.ContentTypeService.GetContainers("Resources", 1).FirstOrDefault();

            if (resourcesContainer != null)
            {
                includeItems = this.ContentTypeService.GetAll().Where(x => x.ParentId == resourcesContainer.Id)
                    .Select(x => x.Alias);
            }

            return new ContentValueSetValidator(true, true, this.PublicAccessService, siteId, includeItems);
        }

        private IEnumerable<WebsiteNode> GetSiteNodes()
        {
            var siteNodes = new List<WebsiteNode>();

            using (var cref = this.UmbracoContext.EnsureUmbracoContext())
            {
                try
                {
                    var cache = cref.UmbracoContext.Content;
                    var sites = cache.GetAtRoot().Where(x => x.ContentType.Alias == "website");

                    foreach (var site in sites)
                    {
                        var siteNode = new WebsiteNode
                        {
                            WebsiteId = site.Id,
                            WebsiteName = site.Name
                        };

                        siteNodes.Add(siteNode);
                    }
                }
                catch (Exception e)
                {
                    siteNodes = new List<WebsiteNode>();
                }
            }

            return siteNodes;
        }

        private WebsiteNode GetSiteNode(IContent content)
        {
            return new WebsiteNode
            {
                WebsiteId = content.Id,
                WebsiteName = content.Name
            };
        }

        private IIndex CreateWebsiteResourceIndex(WebsiteNode site)
        {
            var fields = new FieldDefinitionCollection();
            fields.AddOrUpdate(new FieldDefinition("lat", FieldDefinitionTypes.Double));
            fields.AddOrUpdate(new FieldDefinition("lon", FieldDefinitionTypes.Double));
            
            var index = new UmbracoContentIndex(
                Constants.Examine.ResourceIndexName + "-" + site.WebsiteName,
                this.CreateFileSystemLuceneDirectory(Constants.Examine.ResourceDirectory + "-" + site.WebsiteName),
                fields,
                new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30),
                this.ProfilingLogger,
                this.LanguageService,
                this.GetPublishedContentValueSetValidator(site.WebsiteId));

            

            return index;
        }

        private bool ResourceIndexExists(string websiteName)
        {
            return ExamineManager.Instance.TryGetIndex(Constants.Examine.ResourceIndexName + "-" + websiteName,
                out var index);
        }
    }
}
