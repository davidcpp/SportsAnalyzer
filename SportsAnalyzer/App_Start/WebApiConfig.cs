using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace SportsAnalyzer
{
  public static class WebApiConfig
  {
    public static void Register(HttpConfiguration config)
    {
      // TODO: Add any additional configuration code.

      // Web API routes
      config.MapHttpAttributeRoutes();

      config.Routes.MapHttpRoute(
          name: "DefaultApi",
          routeTemplate: "Football/api/{controller}/{teamName}/{startRound}/{endRound}/{league}/{seasonYear}",
          defaults: new
          {
            teamName = RouteParameter.Optional,
            startRound = RouteParameter.Optional,
            endRound = RouteParameter.Optional,         
            league = RouteParameter.Optional,
            seasonYear = RouteParameter.Optional
          }
      );
    }
  }
}