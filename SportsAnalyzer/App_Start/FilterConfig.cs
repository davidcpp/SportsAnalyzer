using System.Web.Mvc;

namespace SportsAnalyzer
{
  public static class FilterConfig
  {
    public static void RegisterGlobalFilters(GlobalFilterCollection filters)
    {
      filters.Add(new HandleErrorAttribute());
    }
  }
}
