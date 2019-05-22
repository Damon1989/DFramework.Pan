using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace DFramework.Pan.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Download",
                "download/{ownerId}/{*fullPath}",
                new
                {
                    controller = "File",
                    action = "Download",
                    fullPath = UrlParameter.Optional
                }
            );


            routes.MapRoute(
                "ZipDownload",
                "ZipDownload/{zipNodeId}/{ownerId}/{*fullPath}",
                new
                {
                    controller = "File",
                    action = "ZipDownload",
                    fullPath = UrlParameter.Optional
                }
            );

            routes.MapRoute(
                "File",
                "file/{ownerId}/{*fullPath}",
                new
                {
                    controller = "File",
                    action = "Index",
                    fullPath = UrlParameter.Optional
                });

            routes.MapRoute(
                "Isolate",
                "isolate/{fileId}",
                new
                {
                    controller = "File",
                    action = "Isolate"
                });

            routes.MapRoute(
                "Thumb",
                "Thumb/{ownerId}/{*fullPath}",
                new
                {
                    controller = "File",
                    action = "Thumb"
                });

            //ASP.NET Web API Route Config
            routes.MapHttpRoute(
                "DefaultApi",
                "api/{controller}/{id}",
                new {id = RouteParameter.Optional}
            );

            routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new {controller = "Home", action = "Index", id = UrlParameter.Optional}
            );
        }
    }
}