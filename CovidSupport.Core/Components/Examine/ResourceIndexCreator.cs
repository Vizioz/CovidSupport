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
            var resourcesNodes = this.GetResourceNodes();

            foreach (var resourcesNode in resourcesNodes)
            {
                indexes.Add(this.CreateWebsiteResourceIndex(resourcesNode));
            }

            return indexes;
        }

        public IIndex Create(IContent content)
        {
            var resourceNode = this.GetResourceNode(content);

            if (!this.ResourceIndexExists(resourceNode.WebsiteName))
            {
                return this.CreateWebsiteResourceIndex(resourceNode);
            }
            
            throw new Exception("Resource index for " + resourceNode.WebsiteName + " already exists.");
        }

        public virtual IContentValueSetValidator GetPublishedContentValueSetValidator(int parentId)
        {
            IEnumerable<string> includeItems = null;

            var resourcesContainer = this.ContentTypeService.GetContainers("Resources", 1).FirstOrDefault();

            if (resourcesContainer != null)
            {
                includeItems = this.ContentTypeService.GetAll().Where(x => x.ParentId == resourcesContainer.Id)
                    .Select(x => x.Alias);
            }

            return new ContentValueSetValidator(true, true, this.PublicAccessService, parentId, includeItems);
        }

        private IEnumerable<WebsiteResourcesNode> GetResourceNodes()
        {
            IEnumerable<WebsiteResourcesNode> resourceNodes;

            using (var cref = this.UmbracoContext.EnsureUmbracoContext())
            {
                try
                {
                    var cache = cref.UmbracoContext.Content;

                    resourceNodes = cache.GetByXPath("//website/communityResources").Select(x =>
                        new WebsiteResourcesNode
                            {Id = x.Id, WebsiteId = x.Parent.Id, WebsiteName = x.Parent.Name}).ToList();
                }
                catch (Exception e)
                {
                    resourceNodes = new List<WebsiteResourcesNode>();
                }
            }

            return resourceNodes;
        }

        private WebsiteResourcesNode GetResourceNode(IContent content)
        {
            WebsiteResourcesNode resourceNode;

            using (var cref = this.UmbracoContext.EnsureUmbracoContext())
            {
                try
                {
                    var cache = cref.UmbracoContext.Content;
                    var parent = cache.GetById(content.ParentId);

                    resourceNode = new WebsiteResourcesNode
                    {
                        Id = content.Id,
                        WebsiteId = content.ParentId,
                        WebsiteName = parent.Name
                    };
                }
                catch (Exception e)
                {
                    resourceNode = null;
                }
            }

            return resourceNode;
        }

        private IIndex CreateWebsiteResourceIndex(WebsiteResourcesNode resourcesNode)
        {
            var fields = new FieldDefinitionCollection();
            fields.AddOrUpdate(new FieldDefinition("lat", FieldDefinitionTypes.Double));
            fields.AddOrUpdate(new FieldDefinition("lon", FieldDefinitionTypes.Double));
            
            var index = new UmbracoContentIndex(
                Constants.Examine.ResourceIndexName + "-" + resourcesNode.WebsiteName,
                this.CreateFileSystemLuceneDirectory(Constants.Examine.ResourceDirectory + "-" + resourcesNode.WebsiteName),
                fields,
                new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30),
                this.ProfilingLogger,
                this.LanguageService,
                this.GetPublishedContentValueSetValidator(resourcesNode.Id));

            

            return index;
        }

        private bool ResourceIndexExists(string websiteName)
        {
            return ExamineManager.Instance.TryGetIndex(Constants.Examine.ResourceIndexName + "-" + websiteName,
                out var index);
        }
    }
}
