using System.Collections.Generic;
using Examine;
using Lucene.Net.Analysis.Standard;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Examine;
using Umbraco.Web.Search;
using Version = Lucene.Net.Util.Version;

namespace CovidSupport.Core.Examine
{
    public class ResourceIndexCreator : LuceneIndexCreator, IUmbracoIndexesCreator
    {
        protected IProfilingLogger ProfilingLogger { get; }
        protected ILocalizationService LanguageService { get; }
        protected IPublicAccessService PublicAccessService { get; }

        public ResourceIndexCreator(IProfilingLogger profilingLogger, ILocalizationService languageService,
            IPublicAccessService publicAccessService)
        {
            ProfilingLogger = profilingLogger ?? throw new System.ArgumentNullException(nameof(profilingLogger));
            LanguageService = languageService ?? throw new System.ArgumentNullException(nameof(languageService));
            PublicAccessService = publicAccessService ?? throw new System.ArgumentNullException(nameof(publicAccessService));
        }

        public override IEnumerable<IIndex> Create()
        {
            return new[]
            {
                CreateResourceIndex()
            };
        }

        private IIndex CreateResourceIndex()
        {
            var index = new UmbracoContentIndex(
                Constants.Examine.ResourceIndexName,
                CreateFileSystemLuceneDirectory(Constants.Examine.ResourceDirectory),
                new FieldDefinitionCollection(), 
                new StandardAnalyzer(Version.LUCENE_30), 
                ProfilingLogger,
                LanguageService,
                GetPublishedContentValueSetValidator());

            return index;
        }

        public virtual IContentValueSetValidator GetPublishedContentValueSetValidator()
        {
            return new ContentValueSetValidator(true, true, PublicAccessService, includeItemTypes: new string[] { "communityResource" });
        }
    }
}
