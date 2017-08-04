using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Happiness.Models;
using System.Data.Entity;
namespace Happiness.Controllers
{
    public class EmployeeSearchController : Controller
    {
        //
        // GET: /EmployeeSearch/

        public ActionResult Index()
        {
            return View();
        }

        #region UserProfileIndex
        UsersContext db = new UsersContext();
        public ActionResult Edit(long id = 0)
        {
            ReportingAuthorityAllocation ReportingAuthorityAllocation = db.ReportingAuthorityAllocation.Find(id);
            // division.PhotoPath = "~/Upload/" + division.PhotoPath;
            if (ReportingAuthorityAllocation == null)
            {
                return HttpNotFound();
            }
            return View(ReportingAuthorityAllocation);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            UserProfile division = db.UserProfiles.Find(id);
            db.UserProfiles.Remove(division);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpPost]
        public JsonResult DeleteAJAX(int divsID)
        {
            try
            {
                //UserProfile child = (from t in db.UserProfiles where t.Report_MasterID == divsID select t).FirstOrDefault();
                //db.ReportingAuthorityAllocationChild.Remove(child);
                //db.SaveChanges();
                UserProfile division = db.UserProfiles.Find(divsID);
                db.UserProfiles.Remove(division);
                db.SaveChanges();
                return Json("Record deleted successfully!");
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
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
        public static List<ReportingAuthorityAllocation> GetAllTransactions()
        {
            using (var context = new UsersContext())
            {
                return (from pd in context.ReportingAuthorityAllocation
                        join od in context.ReportingAuthority on pd.Report_id equals od.id
                        select new
                        {
                            id = pd.id,
                            Report_id = pd.Report_id,
                            Reporting_auth_name = od.Reporting_auth_name
                            //  StudentName = od.StudentName
                        }).AsEnumerable().Select(x => new ReportingAuthorityAllocation
                        {
                            id = x.id,
                            Report_id = x.Report_id,
                            Reporting_auth_name = x.Reporting_auth_name
                            //  StudentName = x.StudentName
                        }).ToList();

            }
        }
        private List<UserProfile> FilterData(ref int recordFiltered, int start, int length, string search, int sortColumn, string sortDirection)
        {
            List<UserProfile> list = new List<UserProfile>();
            if (search == null)
            {
                list = db.UserProfiles.ToList();

            }
            else
            {
                // simulate search
                foreach (UserProfile dataItem in db.UserProfiles.ToList())
                {
                    if (dataItem.EmployeeCode.ToUpper().Contains(search.ToUpper()) ||
                         dataItem.EmployeeName.ToUpper().Contains(search.ToUpper()) ||
                        dataItem.Emp_Mob.ToString().Contains(search.ToUpper()) ||
                        dataItem.Emp_Email.ToString().Contains(search.ToUpper()))
                    {
                        list.Add(dataItem);
                    }
                }
            }

            // simulate sort
            if (sortColumn == 0)
            {// sort Name
                list.Sort((x, y) => SortString(x.EmployeeCode, y.EmployeeCode, sortDirection));
            }
            if (sortColumn == 1)
            {// sort Name
                list.Sort((x, y) => SortString(x.EmployeeName, y.EmployeeName, sortDirection));
            }
            if (sortColumn == 3)
            {// sort Name
                list.Sort((x, y) => SortString(x.Emp_Email, y.Emp_Email, sortDirection));
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
            public List<UserProfile> data { get; set; }
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
        public JsonResult Employees()
        {
            return Json(db.UserProfiles.ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CardActivateOrDeactivate(int StudID)
        {

            try
            {
                UserProfile userpro = db.UserProfiles.Find(StudID);
                if (userpro.Emp_isActive == true)
                {
                    userpro.Emp_isActive = false;
                }
                else
                {
                    userpro.Emp_isActive = true;
                }
                db.Entry(userpro).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception ex)
            {

            }
            UserProfile studM = db.UserProfiles.Find(StudID);
            bool cardStatus = studM.Emp_isActive;

            return Json(studM.Emp_isActive, JsonRequestBehavior.AllowGet);
        }
        #endregion

    }
}
