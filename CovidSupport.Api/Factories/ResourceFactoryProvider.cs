using Umbraco.Web;

namespace CovidSupport.Api.Factories
{
    public class ResourceFactoryProvider
    {
        public static IResourceFactory GetResourceFactory(string resourceType, UmbracoHelper helper, string culture)
        {
            if (helper == null)
            {
                helper = Umbraco.Web.Composing.Current.UmbracoHelper;
            }

            switch (resourceType)
            {
                case "socialServices":
                    return new SocialServicesResourceFactory(helper, culture);
                default:
                    return new ResourceFactory(helper, culture);
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
