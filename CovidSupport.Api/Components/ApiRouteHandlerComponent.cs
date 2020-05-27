﻿using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
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
            RouteTable.Routes.MapHttpRoute("CovidSupportApi", "api/v1/{controller}/{action}/{id}",
                new {id = UrlParameter.Optional}, new {controller = "Resource"});
        }

        public void Terminate()
        {
        }
    }
}