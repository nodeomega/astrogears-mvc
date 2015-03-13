// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebApiConfig.cs" company="Jonathan Russell">
//   Copyright (c) Jonathan Russell - All Rights Reserved
// </copyright>
// <summary>
//   Defines the WebApiConfig type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AstroGearsMVC
{
    using System;
    using System.Web.Http;

    using JetBrains.Annotations;

    /// <summary>
    /// The WebAPI Configuration
    /// </summary>
    public static class WebApiConfig
    {
        /// <summary>
        /// Registers the specified configuration.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public static void Register([NotNull] HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}", new { id = RouteParameter.Optional });
        }
    }
}
