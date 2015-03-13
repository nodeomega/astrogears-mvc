// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BundleConfig.cs" company="Jonathan Russell">
//   Copyright (c) Jonathan Russell - All Rights Reserved
// </copyright>
// <summary>
//   The Bundle Configuration.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AstroGearsMVC
{
    using System;
    using System.Web.Optimization;

    using JetBrains.Annotations;

    /// <summary>
    /// The Bundle Configuration.
    /// </summary>
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862

        /// <summary>
        /// Registers the bundles.
        /// </summary>
        /// <param name="bundles">The bundles.</param>
        public static void RegisterBundles([NotNull] BundleCollection bundles)
        {
            ////bundles.UseCdn = true;

            ////const string JqueryCdnPath = "http://ajax.aspnetcdn.com/ajax/jQuery/jquery-2.1.1.min.js";

            ////bundles.Add(new ScriptBundle("~/bundles/jquery", JqueryCdnPath).Include("~/Scripts/jquery-{version}.js"));
            if (bundles == null)
            {
                throw new ArgumentNullException("bundles");
            }

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include("~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/jquery-ui").Include(
                "~/Scripts/jquery-ui-{version}.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(
                new StyleBundle("~/Content/css").Include("~/Content/bootstrap.min.css", new CssRewriteUrlTransform())
                    .Include("~/Content/site.css")
                    .Include("~/Content/font-awesome.min.css", new CssRewriteUrlTransform()));

            bundles.Add(new ScriptBundle("~/bundles/common").Include("~/Scripts/common-library.js"));

            // Set EnableOptimizations to false for debugging. For more information,
            // visit http://go.microsoft.com/fwlink/?LinkId=301862
            BundleTable.EnableOptimizations = true;
        }
    }
}
