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

        public ResourceIndexCreator(IProfilingLogger profilingLogger, ILocalizationService languageService,
            IPublicAccessService publicAccessService, IUmbracoContextFactory context)
        {
            this.ProfilingLogger = profilingLogger ?? throw new System.ArgumentNullException(nameof(profilingLogger));
            this.LanguageService = languageService ?? throw new System.ArgumentNullException(nameof(languageService));
            this.PublicAccessService = publicAccessService ?? throw new System.ArgumentNullException(nameof(publicAccessService));
            this.UmbracoContext = context ?? throw new System.ArgumentNullException(nameof(context));
        }

        public override IEnumerable<IIndex> Create()
        {
            var indexes = new List<IIndex>();
            var resourcesNodes = this.GetResourceNodes();

            foreach (var resourcesNode in resourcesNodes)
            {
                indexes.Add(this.CreateWebsiteResourceIndex(resourcesNode));
            }

            return indexes;
        }

        public IIndex Create(IContent content)
        {
            string websiteName;

            using (var cref = this.UmbracoContext.EnsureUmbracoContext())
            {
                var cache = cref.UmbracoContext.Content;
                var parent = cache.GetById(content.ParentId);
                websiteName = parent.Name;
            }

            var resourceNode = new WebsiteResourcesNode
            {
                Id = content.Id,
                WebsiteId = content.ParentId,
                WebsiteName = websiteName
            };

            return this.CreateWebsiteResourceIndex(resourceNode);
        }

        private IEnumerable<WebsiteResourcesNode> GetResourceNodes()
        {
            IEnumerable<WebsiteResourcesNode> resourceNodes;

            using (var cref = this.UmbracoContext.EnsureUmbracoContext())
            {
                var cache = cref.UmbracoContext.Content;

                resourceNodes = cache.GetByXPath("//website/communityResources").Select(x => new WebsiteResourcesNode
                    {Id = x.Id, WebsiteId = x.Parent.Id, WebsiteName = x.Parent.Name}).ToList();
            }

            return resourceNodes;
        }

        private IIndex CreateWebsiteResourceIndex(WebsiteResourcesNode resourcesNode)
        {
            var index = new UmbracoContentIndex(
                Constants.Examine.ResourceIndexName + "-" + resourcesNode.WebsiteName,
                this.CreateFileSystemLuceneDirectory(Constants.Examine.ResourceDirectory + "-" + resourcesNode.WebsiteName),
                new FieldDefinitionCollection(),
                new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30),
                this.ProfilingLogger,
                this.LanguageService,
                this.GetPublishedContentValueSetValidator(resourcesNode.Id));

            return index;
        }

        public virtual IContentValueSetValidator GetPublishedContentValueSetValidator(int parentId)
        {
            return new ContentValueSetValidator(true, true, this.PublicAccessService, parentId);
        }
    }
}
