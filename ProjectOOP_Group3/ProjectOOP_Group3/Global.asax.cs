using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Helpers;

namespace ProjectOOP_Group3
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            
            // Configure AntiForgery to not require specific user identity
            // This prevents issues when user context changes between form generation and submission
            AntiForgeryConfig.SuppressIdentityHeuristicChecks = true;
        }

        protected void Application_BeginRequest()
        {
            Response.ContentType = "text/html; charset=utf-8";
        }
    }
}
