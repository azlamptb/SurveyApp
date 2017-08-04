using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Happiness.Models;
using System.Data.Entity;
using System.Web.Security;

namespace Happiness.Controllers
{
    public class ReportingAuthorityController : Controller
    {
        //
        // GET: /ReportingAuthority/
        UsersContext db = new UsersContext();
        [OutputCache(Duration = 30)]
        public ActionResult Index()
        {
            if (Roles.IsUserInRole("admin") || AccountController.IsAccess("Reporting Authority", User.Identity.Name, db))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Noaccess", "Account");
            }
        }
        [HttpGet]
        public ActionResult Create()
        {
            if (Roles.IsUserInRole("admin") || AccountController.IsAccess("Reporting Authority", User.Identity.Name, db))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Noaccess", "Account");
            }
        }
       
       
        public ActionResult Edit(long id = 0)
        {
            if (Roles.IsUserInRole("admin") || AccountController.IsAccess("Reporting Authority", User.Identity.Name, db))
            {
                ReportingAuthority division = db.ReportingAuthority.Find(id);
                // division.PhotoPath = "~/Upload/" + division.PhotoPath;
                if (division == null)
                {
                    return HttpNotFound();
                }
                return View(division);
            }
            else
            {
                return RedirectToAction("Noaccess", "Account");
            }
           
        }

        //
        // POST: /Student/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ReportingAuthority ReportingAuthority)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(ReportingAuthority).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    //if (ex.Message.Contains("DBException"))
                    //{
                    ModelState.AddModelError("", "You are updating with existing data!! please correct the data");
                    //}
                }
            }
            return View(ReportingAuthority);
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
       // [Authorize(Roles = "Admin")]
        public ActionResult Create(ReportingAuthority ReportingAuthority)
        {
            if (ModelState.IsValid)
            {

                try
                {
                    ReportingAuthority.CreatedBy = User.Identity.Name;
                    ReportingAuthority.CreatedDate = DateTime.Now;
                    ReportingAuthority.Modifieddate = null;
                    db.ReportingAuthority.Add(ReportingAuthority);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Reporting Authority Name : " + ReportingAuthority.Reporting_auth_name + " Already Exists");
                }

                //db.StudentModel.Add(studentmodel);
                //db.SaveChanges();
                //return RedirectToAction("Index");
            }

            return View(ReportingAuthority);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            ReportingAuthority division = db.ReportingAuthority.Find(id);
            db.ReportingAuthority.Remove(division);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpPost]
        public JsonResult DeleteAJAX(int divsID)
        {

            ReportingAuthority division = db.ReportingAuthority.Find(divsID);
            db.ReportingAuthority.Remove(division);
            db.SaveChanges();
            return Json("Record deleted successfully!");
        }
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        private int SortString(string s1, string s2, string sortDirection)
        {
            return sortDirection == "asc" ? s1.CompareTo(s2) : s2.CompareTo(s1);
        }

        private int SortInteger(string s1, string s2, string sortDirection)
        {
            long i1 = long.Parse(s1);
            long i2 = long.Parse(s2);
            return sortDirection == "asc" ? i1.CompareTo(i2) : i2.CompareTo(i1);
        }

        private int SortDateTime(string s1, string s2, string sortDirection)
        {
            DateTime d1 = DateTime.Parse(s1);
            DateTime d2 = DateTime.Parse(s2);
            return sortDirection == "asc" ? d1.CompareTo(d2) : d2.CompareTo(d1);
        }
        private List<ReportingAuthority> FilterData(ref int recordFiltered, int start, int length, string search, int sortColumn, string sortDirection)
        {
            List<ReportingAuthority> list = new List<ReportingAuthority>();
            if (search == null)
            {
                list = db.ReportingAuthority.ToList();

            }
            else
            {
                // simulate search
                foreach (ReportingAuthority dataItem in db.ReportingAuthority.ToList())
                {
                    if (dataItem.Reporting_auth_name.ToUpper().Contains(search.ToUpper()))
                    {
                        list.Add(dataItem);
                    }
                }
            }

            // simulate sort
            if (sortColumn == 0)
            {// sort Name
                list.Sort((x, y) => SortString(x.Reporting_auth_name, y.Reporting_auth_name, sortDirection));
            }
            //else if (sortColumn == 1)
            //{// sort Name
            //    list.Sort((x, y) => SortInteger(x.StudentRegno, y.StudentRegno, sortDirection));
            //}
            //else if (sortColumn == 2)
            //{// sort Age
            //    list.Sort((x, y) => SortInteger(x.StudentAppNo, y.StudentAppNo, sortDirection));
            //}
            //else if (sortColumn == 3)
            //{
            //    list.Sort((x, y) => SortInteger(x.RechargeCardNo, y.RechargeCardNo, sortDirection));
            //    // sort DoB
            //    // list.Sort((x, y) => SortDateTime(x.DoB, y.DoB, sortDirection));
            //}

            recordFiltered = list.Count;

            // get just one page of data
            list = list.GetRange(start, Math.Min(length, list.Count - start));

            return list;
        }
        public class DataTableData
        {
            public int draw { get; set; }
            public int recordsTotal { get; set; }
            public int recordsFiltered { get; set; }
            public List<ReportingAuthority> data { get; set; }
        }

        public JsonResult Search()
        {
            List<ReportingAuthority> list = new List<ReportingAuthority>();
            var json = Json(db.ReportingAuthority.ToList());
            return Json(db.ReportingAuthority.ToList(), JsonRequestBehavior.AllowGet);
            //return Json(db.StudentModel.ToList(), JsonRequestBehavior.AllowGet);
        }
        public ActionResult GettAllData(int draw, int start, int length)
        {
            string search = Request.QueryString["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";
            if (length == -1)
            {
                // length = TOTAL_ROWS;
            }

            // note: we only sort one column at a time
            if (Request.QueryString["order[0][column]"] != null)
            {
                sortColumn = int.Parse(Request.QueryString["order[0][column]"]);
            }
            if (Request.QueryString["order[0][dir]"] != null)
            {
                sortDirection = Request.QueryString["order[0][dir]"];
            }

            DataTableData dataTableData = new DataTableData();
            dataTableData.draw = draw;
            // dataTableData.recordsTotal = TOTAL_ROWS;
            int recordsFiltered = 0;
            dataTableData.data = FilterData(ref recordsFiltered, start, length, search, sortColumn, sortDirection);
            dataTableData.recordsFiltered = recordsFiltered;

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }
    }
}
