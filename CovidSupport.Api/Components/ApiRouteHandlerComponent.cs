using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Mvc;
using System.Web.Routing;
using CovidSupport.Api.Constants;
using Umbraco.Core;
using Umbraco.Core.Composing;

namespace CovidSupport.Api.Components
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class ApiRouteHandlerComposer : ComponentComposer<ApiRouteHandlerComponent>
    {
    }

    public class ApiRouteHandlerComponent : IComponent
    {
        public void Initialize()
        {
            GlobalConfiguration.Configure(Register);

            RouteTable.Routes.MapHttpRoute("CovidSupportApi",
                ApiConstants.ApiName + "/" + ApiConstants.Version + "/{controller}/{action}/{id}",
                new { id = UrlParameter.Optional },
                new { controller = "Resource" });

            RouteTable.Routes.MapHttpRoute("CovidSupportApiMultilingual",
                "{language}/" + ApiConstants.ApiName + "/" + ApiConstants.Version + "/{controller}/{action}/{id}",
                new { id = UrlParameter.Optional, language = UrlParameter.Optional },
                new { controller = "Resource" });
        }

        public void Terminate()
        {
        }

        public static void Register(HttpConfiguration config)
        {
            var corsAttr = new EnableCorsAttribute("*", "*", "*");
            config.EnableCors(corsAttr);
        }
    }
}
