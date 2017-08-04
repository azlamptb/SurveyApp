using Happiness.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Happiness.Controllers
{
    public class SettingsController : Controller
    {
        //
        // GET: /Settings/

      
        private UsersContext db = new UsersContext();


        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Company()
        {
            if (Roles.IsUserInRole("admin") || AccountController.IsAccess("Company Registration", User.Identity.Name, db))
            {
                var company = db.CompanyModel.ToList();
                if (company.Count > 0)
                {
                    foreach (CompanyModel c in company)
                    {
                        ViewBag.id = c.id;
                        ViewBag.CompanyName = c.CompanyName;
                        ViewBag.Address = c.Address;
                        ViewBag.Mob = c.Mob;
                        ViewBag.Tel = c.Tel;
                        ViewBag.EmailId = c.EmailId;
                        ViewBag.Fax = c.Fax;

                    }

                }
                else
                {
                    ViewBag.id = 1;
                }
                return View();
            }
            else
            {
                return RedirectToAction("Noaccess", "Account");
            }

          
        }

        [HttpPost]
        public ActionResult Company(CompanyModel companyModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var company = db.CompanyModel.ToList();
                    if (company.Count == 0)
                    {
                        db.CompanyModel.Add(companyModel);
                        db.SaveChanges();
                    }
                    else
                    {
                        CompanyModel comp = db.CompanyModel.Where(d => d.id == companyModel.id).FirstOrDefault();
                        comp.CompanyName = companyModel.CompanyName;
                        comp.Address = companyModel.Address;
                        comp.Mob = companyModel.Mob;
                        comp.Tel = companyModel.Tel;
                        comp.EmailId = companyModel.EmailId;
                        comp.Fax = companyModel.Fax;
                        db.Entry(comp).State = EntityState.Modified;
                        db.SaveChanges();

                    }

                }
                catch (Exception ex)
                {
                    //if (ex.Message.Contains("DBException"))
                    //{
                    ModelState.AddModelError("", "You are updating with existing data!! please correct the data");
                    //}
                }

            }
            ViewBag.id = companyModel.id;
            ViewBag.CompanyName = companyModel.CompanyName;
            ViewBag.Address = companyModel.Address;
            ViewBag.Mob = companyModel.Mob;
            ViewBag.Tel = companyModel.Tel;
            ViewBag.EmailId = companyModel.EmailId;
            ViewBag.Fax = companyModel.Fax;
            return View(companyModel);
        }

        #region emailconfig

        [HttpGet]
        public ActionResult EmailConfig()
        {
            if (Roles.IsUserInRole("admin") || AccountController.IsAccess("Email configuration", User.Identity.Name, db))
            {
                var emailconfig = db.EmailConfig.ToList();
                if (emailconfig.Count > 0)
                {
                    foreach (EmailConfig c in emailconfig)
                    {
                        ViewBag.id = c.id;
                        ViewBag.smtphostname = c.smtphostname;
                        ViewBag.smtpPort = c.smtpPort;
                        ViewBag.SSLEnabled = c.SSLEnabled;
                        ViewBag.SendEmail = c.SendEmail;
                        ViewBag.Passowrd = c.Passowrd;
                    }

                }
                else
                {
                    ViewBag.id = 1;
                }
                return View();
            }
            else
            {
                return RedirectToAction("Noaccess", "Account");
            }


           
        }

        [HttpPost]
        public ActionResult EmailConfig(EmailConfig EmailConfig)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var emailconf = db.EmailConfig.ToList();
                    if (emailconf.Count == 0)
                    {
                        db.EmailConfig.Add(EmailConfig);
                        db.SaveChanges();
                    }
                    else
                    {
                        EmailConfig emailcon = db.EmailConfig.Where(d => d.id == EmailConfig.id).FirstOrDefault();
                        emailcon.smtphostname = EmailConfig.smtphostname;
                        emailcon.smtpPort = EmailConfig.smtpPort;
                        emailcon.SSLEnabled = EmailConfig.SSLEnabled;
                        emailcon.SendEmail = EmailConfig.SendEmail;
                        emailcon.Passowrd = EmailConfig.Passowrd;
                        db.Entry(emailcon).State = EntityState.Modified;
                        db.SaveChanges();

                    }

                }
                catch (Exception ex)
                {
                    //if (ex.Message.Contains("DBException"))
                    //{
                    ModelState.AddModelError("", "You are updating with existing data!! please correct the data");
                    //}
                }

            }
            ViewBag.id = EmailConfig.id;
            ViewBag.smtphostname = EmailConfig.smtphostname;
            ViewBag.smtpPort = EmailConfig.smtpPort;
            ViewBag.SSLEnabled = EmailConfig.SSLEnabled;
            ViewBag.SendEmail = EmailConfig.SendEmail;
            ViewBag.Passowrd = EmailConfig.Passowrd;
            return View(EmailConfig);
        }

        #endregion
    }
}
