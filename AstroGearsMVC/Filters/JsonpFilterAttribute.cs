namespace AstroGearsMVC.Filters
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Web.Mvc;

    /// <summary>
    /// The JsonpFilterAttribute.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public class JsonpFilterAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Called by the ASP.NET MVC framework after the action method executes.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        /// <exception cref="System.ArgumentNullException">filterContext is null.</exception>
        /// <exception cref="System.InvalidOperationException">JsonpFilterAttribute must be applied only on controllers and actions that return a  JsonResult object.</exception>
        public override void OnActionExecuted(
                ActionExecutedContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            // see if this request included a "callback" querystring parameter
            var callback = filterContext.HttpContext.Request.QueryString["callback"] ?? filterContext.HttpContext.Request.QueryString["jsoncallback"];

            if (string.IsNullOrEmpty(callback))
            {
                return;
            }
            
            // ensure that the result is a "JsonResult"
            var result = filterContext.Result as JsonResult;
            if (result == null)
            {
                throw new InvalidOperationException(
                    "JsonpFilterAttribute must be applied only on controllers and actions that return a JsonResult object.");
            }

            filterContext.Result = new JsonpResult
                                       {
                                           ContentEncoding = result.ContentEncoding,
                                           ContentType = result.ContentType,
                                           Data = result.Data,
                                           Callback = callback
                                       };
        }
    }
}