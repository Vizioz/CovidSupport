using Umbraco.Core.Services;
using Umbraco.Web;

namespace CovidSupport.Api.Factories
{
    public class ResourceFactoryProvider
    {
        public static IResourceFactory GetResourceFactory(string resourceType, UmbracoHelper helper, IContentService contentService, string culture)
        {
            if (helper == null)
            {
                helper = Umbraco.Web.Composing.Current.UmbracoHelper;
            }

            if (contentService == null)
            {
                contentService = Umbraco.Core.Composing.Current.Services.ContentService;
            }

            switch (resourceType)
            {
                case "socialServices":
                    return new SocialServicesResourceFactory(helper, contentService, culture);
                default:
                    return new ResourceFactory(helper, contentService, culture);
            }
        }

        public static string GetResourceFactoryName(string resourceType)
        {
            switch (resourceType)
            {
                case "socialServices":
                    return "socialServices";
                default:
                    return "default";
            }
        }
    }
}
