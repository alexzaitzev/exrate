using System.Web.Optimization;

namespace Exrate
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/scripts").Include(
                "~/Scripts/knockout-{version}.js",
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/bootstrap.js",
                "~/Scripts/cookie.js",
                "~/Scripts/accounting.js",
                "~/Scripts/director.js",
                "~/Scripts/jquery-scrolltofixed.js"));

            bundles.Add(new StyleBundle("~/bundles/css").Include(
                "~/Styles/site.css",
                "~/Styles/hint.css"));
            bundles.Add(new StyleBundle("~/Styles/bootstrap/bubdles").Include(
                "~/Styles/bootstrap/bootstrap.css",
                "~/Styles/bootstrap/bootstrap-responsive.css"));

            BundleTable.EnableOptimizations = true;
        }
    }
}