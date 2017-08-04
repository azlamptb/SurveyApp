using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Happiness.Models;
namespace Happiness.Controllers
{
    public class HomeController : Controller
    {
        UsersContext db = new UsersContext();
        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            ViewBag.users = db.UserProfiles.Count();
            ViewBag.Reportingauth = db.ReportingAuthority.Count();
            ViewBag.emotions = db.Emotion.Count();
            ViewBag.emotionsdet = db.Emotion.ToList();
            //ViewBag.Roles = roles 

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
