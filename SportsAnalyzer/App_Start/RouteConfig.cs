using System.Web.Mvc;
using System.Web.Routing;

namespace SportsAnalyzer
{
  public class RouteConfig
  {
    public static void RegisterRoutes(RouteCollection routes)
    {
      routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

      // TODO: This route is default ony for developing phase and tests
      routes.MapRoute(
          name: "FootballController",
          url: "{controller}/{action}/{startRound}/{endRound}/{league}/{seasonYear}",
          defaults: new
          {
            controller = "Home",
            action = "Index",
            startRound = UrlParameter.Optional,
            endRound = UrlParameter.Optional,
            league = UrlParameter.Optional,
            seasonYear = UrlParameter.Optional
          }
      );
    }
  }
}
