﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Sprint
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "close_issue",
                url: "{ownerName}/{repoName}/{issueId}/close",
                defaults: new { controller = "Home", action = "CompletedIssue" }
            );

            routes.MapRoute(
                name: "remove_issue",
                url: "{ownerName}/{repoName}/{issueId}/remove",
                defaults: new { controller = "Home", action = "RemoveIssue" }
            );

            routes.MapRoute(
                name: "add_issue",
                url: "{ownerName}/{repoName}/{issueId}/add",
                defaults: new { controller = "Home", action = "AddIssue" }
            );

            routes.MapRoute(
                name: "sprint_list",
                url: "{ownerName}/{repoName}/",
                defaults: new { controller = "Home", action = "List" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
