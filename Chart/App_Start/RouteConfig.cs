using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MIMSChart
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapMvcAttributeRoutes();

            routes.MapRoute(
               name: "DefaultH",
               url: "{controller}/{action}/{StartDate}/{EndDate}",
               defaults: new { controller = "Home", action = "Index", StartDate = DateTime.Now.Date.AddDays(-31), EndDate = DateTime.Now }
           );

          //  routes.MapRoute(
          //    name: "Default",
          //    url: "{controller}/{action}/{id}",
          //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional}
          //);

           
        }
    }
}
