using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SportsAnalyzer
{
  public class RouteConfig
  {
    public static void RegisterRoutes(RouteCollection routes)
    {
      routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

      //routes.MapRoute(
      //    name: "Default",
      //    url: "{controller}/{action}/{id}",
      //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
      //);

      routes.MapRoute(
          name: "Default",
          url: "{controller}/{action}/{league}/{seasonYear}/{arg1}/{arg2}/{arg3}",
          defaults: new {
            controller ="Football",
            action = "Stats",
            league = UrlParameter.Optional,
            seasonYear = UrlParameter.Optional,
            arg1 = UrlParameter.Optional,
            arg2 = UrlParameter.Optional,
            arg3 = UrlParameter.Optional
          }
      );
    }
  }
}
