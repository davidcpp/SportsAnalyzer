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

      // TODO: This route is default ony for developing phase and tests
      routes.MapRoute(
          name: "Default",
          url: "{controller}/{action}/{startRound}/{endRound}/{teamName}/{league}/{seasonYear}",
          defaults: new
          {
            controller = "Football",
            action = "Stats",
            startRound = UrlParameter.Optional,
            endRound = UrlParameter.Optional,
            teamName = UrlParameter.Optional,
            league = UrlParameter.Optional,
            seasonYear = UrlParameter.Optional
          }
      );
    }
  }
}
