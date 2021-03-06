﻿namespace SportsAnalyzer
{
  using System.Web.Optimization;

  public static class BundleConfig
  {
    // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
    public static void RegisterBundles(BundleCollection bundles)
    {
      bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
          "~/Scripts/bootstrap.min.js",
          "~/Scripts/respond.min.js"));

      bundles.Add(new ScriptBundle("~/bundles/chartjs").Include(
                "~/Scripts/Chart.js",
                "~/Scripts/ChartUtils.js"));

      bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
            "~/Scripts/jquery-{version}.js"));

      bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                  "~/Scripts/jquery.validate*"));

      // Use the development version of Modernizr to develop with and learn from. Then, when you're
      // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
      bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                  "~/Scripts/modernizr-*"));

      bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/bootstrap.min.css",
                "~/Content/Site.css",
                "~/Content/chartjs.css"));
    }
  }
}
