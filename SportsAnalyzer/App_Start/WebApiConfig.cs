namespace SportsAnalyzer
{
  using System.Web.Http;

  public static class WebApiConfig
  {
    public static void Register(HttpConfiguration config)
    {
      // TODO: Add any additional configuration code.

      // Web API routes
      config.MapHttpAttributeRoutes();

      config.Routes.MapHttpRoute(
          name: "DefaultApi",
          routeTemplate: "Football/api/{controller}/{action}"
      );
    }
  }
}