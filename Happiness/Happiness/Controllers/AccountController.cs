using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using Happiness.Filters;
using Happiness.Models;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Objects;
using System.IO;
using OfficeOpenXml;

namespace Happiness.Controllers
{
    [Authorize]
    [InitializeSimpleMembership]
    public class AccountController : Controller
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            if (Roles.IsUserInRole("admin") || IsAccess("Employee", User.Identity.Name, db))
            {
                return View();
            }
            else
            {
               return RedirectToAction("Noaccess", "Account");
            }
        }

        //
        // GET: /Account/Login

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid && WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
            {
                return RedirectToLocal(returnUrl);
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "The user name or password provided is incorrect.");
            return View(model);
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();

            return RedirectToAction("Login", "Account");
        }

        //
        // GET: /Account/Register

        [AllowAnonymous]
        public ActionResult Register()
        {
            SelectList list = new SelectList(Roles.GetAllRoles());
            ViewBag.Roles = list;
            return View();
        }

        //
        // POST: /Account/Register

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    model.Emp_isActive = true;
                    WebSecurity.CreateUserAndAccount(model.UserName, model.Password, new
                    {
                        EmployeeCode = model.EmployeeCode,
                        EmployeeName = model.EmployeeName,
                        Emp_Address = model.Emp_Address,
                        Emp_Email = model.Emp_Email,
                        Emp_isActive = model.Emp_isActive,
                        Emp_Tel = model.Emp_Tel,
                        Emp_Mob = model.Emp_Mob,
                        Emp_Reporting_Authority = model.Emp_Reporting_Authority
                    });
                    Roles.AddUserToRole(model.UserName, model.RoleID);
                    //WebSecurity.Login(model.UserName, model.Password);
                    return RedirectToAction("Index", "Account");
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/Disassociate

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Disassociate(string provider, string providerUserId)
        {
            string ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
            ManageMessageId? message = null;

            // Only disassociate the account if the currently logged in user is the owner
            if (ownerAccount == User.Identity.Name)
            {
                // Use a transaction to prevent the user from deleting their last login credential
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
                {
                    bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
                    if (hasLocalAccount || OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name).Count > 1)
                    {
                        OAuthWebSecurity.DeleteAccount(provider, providerUserId);
                        scope.Complete();
                        message = ManageMessageId.RemoveLoginSuccess;
                    }
                }
            }

            return RedirectToAction("Manage", new { Message = message });
        }

        //
        // GET: /Account/Manage

        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : "";
            ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(LocalPasswordModel model)
        {
            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.HasLocalPassword = hasLocalAccount;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasLocalAccount)
            {
                if (ModelState.IsValid)
                {
                    // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                    bool changePasswordSucceeded;
                    try
                    {
                        changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                    }
                    catch (Exception)
                    {
                        changePasswordSucceeded = false;
                    }

                    if (changePasswordSucceeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                    }
                }
            }
            else
            {
                // User does not have a local password so remove any validation errors caused by a missing
                // OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        WebSecurity.CreateAccount(User.Identity.Name, model.NewPassword);
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError("", String.Format("Unable to create local account. An account with the name \"{0}\" may already exist.", User.Identity.Name));
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/ExternalLogin

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback

        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
            if (!result.IsSuccessful)
            {
                return RedirectToAction("ExternalLoginFailure");
            }

            if (OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, createPersistentCookie: false))
            {
                return RedirectToLocal(returnUrl);
            }

            if (User.Identity.IsAuthenticated)
            {
                // If the current user is logged in add the new account
                OAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId, User.Identity.Name);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // User is new, ask for their desired membership name
                string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
                ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
                ViewBag.ReturnUrl = returnUrl;
                return View("ExternalLoginConfirmation", new RegisterExternalLoginModel { UserName = result.UserName, ExternalLoginData = loginData });
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetAllroles()
        {
            SelectList list = new SelectList(Roles.GetAllRoles().ToList(), "Id", "Name");
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        //
        // POST: /Account/ExternalLoginConfirmation

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLoginConfirmation(RegisterExternalLoginModel model, string returnUrl)
        {
            string provider = null;
            string providerUserId = null;

            if (User.Identity.IsAuthenticated || !OAuthWebSecurity.TryDeserializeProviderUserId(model.ExternalLoginData, out provider, out providerUserId))
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Insert a new user into the database
                using (UsersContext db = new UsersContext())
                {
                    UserProfile user = db.UserProfiles.FirstOrDefault(u => u.UserName.ToLower() == model.UserName.ToLower());
                    // Check if user already exists
                    if (user == null)
                    {
                        // Insert name into the profile table
                        db.UserProfiles.Add(new UserProfile { UserName = model.UserName });
                        db.SaveChanges();

                        OAuthWebSecurity.CreateOrUpdateAccount(provider, providerUserId, model.UserName);
                        OAuthWebSecurity.Login(provider, providerUserId, createPersistentCookie: false);

                        return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        ModelState.AddModelError("UserName", "User name already exists. Please enter a different user name.");
                    }
                }
            }

            ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(provider).DisplayName;
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // GET: /Account/ExternalLoginFailure

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_ExternalLoginsListPartial", OAuthWebSecurity.RegisteredClientData);
        }

        [ChildActionOnly]
        public ActionResult RemoveExternalLogins()
        {
            ICollection<OAuthAccount> accounts = OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name);
            List<ExternalLogin> externalLogins = new List<ExternalLogin>();
            foreach (OAuthAccount account in accounts)
            {
                AuthenticationClientData clientData = OAuthWebSecurity.GetOAuthClientData(account.Provider);

                externalLogins.Add(new ExternalLogin
                {
                    Provider = account.Provider,
                    ProviderDisplayName = clientData.DisplayName,
                    ProviderUserId = account.ProviderUserId,
                });
            }

            ViewBag.ShowRemoveButton = externalLogins.Count > 1 || OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            return PartialView("_RemoveExternalLoginsPartial", externalLogins);
        }

        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion

        #region UserProfileIndex
        UsersContext db = new UsersContext();
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
                    try
                    {
                        if (dataItem.EmployeeCode.ToUpper().Contains(search.ToUpper()) ||
                            dataItem.EmployeeName.ToUpper().Contains(search.ToUpper()) ||
                            Convert.ToString(dataItem.Emp_Mob).Contains(search.ToUpper()) ||
                            Convert.ToString(dataItem.Emp_Email).Contains(search.ToUpper()))
                        {
                            list.Add(dataItem);
                        }
                    }
                    catch (Exception ex)
                    {
                        continue;
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

        #region Edit
        public ActionResult Edit(long id = 0)
        {
            UserProfile UserPro = db.UserProfiles.Find(id);
            // division.PhotoPath = "~/Upload/" + division.PhotoPath;
            ViewBag.Report_ID = UserPro.Emp_Reporting_Authority;
            ViewBag.roleId = Roles.GetRolesForUser(UserPro.UserName);
            SelectList list = new SelectList(Roles.GetAllRoles());
            ViewBag.Roles = list;

            if (UserPro == null)
            {
                return HttpNotFound();
            }
            return View(UserPro);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserProfile UserProfile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    UserProfile.Emp_isActive = true;
                    db.Entry(UserProfile).State = EntityState.Modified;
                    db.SaveChanges();
                    var role= Roles.GetRolesForUser(UserProfile.UserName);
                    Roles.RemoveUserFromRole(UserProfile.UserName,role.ToList().SingleOrDefault());
                    Roles.AddUserToRole(UserProfile.UserName, UserProfile.RoleID);    
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
            return View(UserProfile);
        }
        #endregion

        #region ExcelUpload
        public ActionResult ExcelUpload()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ExcelUpload(FormCollection fromcollection)
        {
            if (Request != null)
            {
                HttpPostedFileBase file = Request.Files["UploadedFile"];
                string extension = Path.GetExtension(file.FileName);
                if (file != null && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    if (extension == ".xlsx")
                    {
                        string fileName = file.FileName;
                        string fileContentType = file.ContentType;
                        byte[] fileBytes = new byte[file.ContentLength];
                        var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                        using (var package = new ExcelPackage(file.InputStream))
                        {
                            var currentSheet = package.Workbook.Worksheets;
                            var workSheet = currentSheet.First();
                            var noOfCol = workSheet.Dimension.End.Column;
                            var noOfRow = workSheet.Dimension.End.Row;
                            for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                            {
                                try
                                {

                                    string EmployeeCode = workSheet.Cells[rowIterator, 1].Value.ToString();
                                    string employeeName = workSheet.Cells[rowIterator, 2].Value.ToString();
                                    string ReportAuth = workSheet.Cells[rowIterator, 3].Value.ToString();
                                    string EmployeeAddress = null;
                                    if (workSheet.Cells[rowIterator, 4].Value != null)
                                    {
                                        EmployeeAddress = workSheet.Cells[rowIterator, 4].Value.ToString();
                                    }
                                    string mobile = null;
                                    if (workSheet.Cells[rowIterator, 5].Value != null)
                                    {
                                        mobile = workSheet.Cells[rowIterator, 5].Value.ToString();
                                    }
                                    string telephone=null;
                                    if (workSheet.Cells[rowIterator, 6].Value != null)
                                    {
                                        telephone = workSheet.Cells[rowIterator, 6].Value.ToString();
                                    }
                                    string Email = null;
                                    if (workSheet.Cells[rowIterator, 7].Value != null)
                                    {
                                        Email = workSheet.Cells[rowIterator, 7].Value.ToString();
                                    }
                                    string username = workSheet.Cells[rowIterator, 8].Value.ToString();
                                    string password = workSheet.Cells[rowIterator, 9].Value.ToString();
                                    string RoleName = workSheet.Cells[rowIterator, 10].Value.ToString();

                                    var EmpC = db.UserProfiles.Where(u => u.EmployeeCode == EmployeeCode).FirstOrDefault();
                                    var UserN = db.UserProfiles.Where(u => u.UserName == username).FirstOrDefault();
                                    ReportingAuthority reportA = db.ReportingAuthority.Where(u => u.Reporting_auth_name == ReportAuth).FirstOrDefault();
                                    if (EmpC != null)
                                    {
                                        ModelState.AddModelError("",
                                          "Employee Code" + EmployeeCode + " Already Exists");
                                    }
                                    if (UserN != null)
                                    {
                                        ModelState.AddModelError("",
                                          "UserName " + username + " Already Exists");
                                    }
                                    if (reportA == null)
                                    {
                                        ModelState.AddModelError("",
                                          "Report Authority " + username + " Npt Exists");
                                    }
                                    if (EmpC == null && UserN == null && password != null && reportA != null && RoleName !=string.Empty)
                                    {

                                        bool Emp_isActive = true;
                                        WebSecurity.CreateUserAndAccount(username, password, new
                                        {
                                            EmployeeCode = EmployeeCode,
                                            EmployeeName = employeeName,
                                            Emp_Address = EmployeeAddress,
                                            Emp_Email = Email,
                                            Emp_isActive = Emp_isActive,
                                            Emp_Tel = telephone,
                                            Emp_Mob = mobile,
                                            Emp_Reporting_Authority = reportA.id
                                        });
                                        Roles.AddUserToRole(username, RoleName);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    ModelState.AddModelError("", "Error :" + ex.Message + "Row No:" + rowIterator);
                                    continue;
                                }
                            }
                        }

                    }
                    else
                    {

                        ModelState.AddModelError("", "Please Select valid Excel file");
                    }
                }

            }

            return View();
        }
        #endregion

        #region Roles

        public ActionResult CreateRoles()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        public ActionResult RoleIndex()
        {
            var roles = Roles.GetAllRoles();
            return View(roles);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateRoles(string RoleName)
        {
            if (Request.Form["RoleName"] != string.Empty)
            {
                Roles.CreateRole(Request.Form["RoleName"]);
                // ViewBag.ResultMessage = "Role created successfully !";

                return RedirectToAction("RoleIndex", "Account");
            }
            else
            {
                ModelState.AddModelError("RoleName","Please enter RoleName");
                return View();
            }
        }
        [Authorize(Roles = "Admin")]
        public ActionResult RoleDelete(string RoleName)
        {

            Roles.DeleteRole(RoleName);
            // ViewBag.ResultMessage = "Role deleted succesfully !";
            return Json("Record deleted successfully!");
        }
        #endregion

        #region Permission
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult Permission(string RoleName)
        {
            if (RoleName != null)
            {
                if (Roles.RoleExists(RoleName))
                {
                    ViewBag.RoleName = RoleName;
                    List<Permission> perm = new List<Models.Permission>();     
                    perm =  db.Permission.ToList().Where(u=>u.rolName == RoleName).ToList();
                    if (perm.Count > 0)
                    {
                        return View(perm);
                    }
                    else
                    {
                        perm.Add(new Permission() { id = 0, MenuName = "Reporting Authority", rolName = RoleName, permission = false });
                        perm.Add(new Permission() { id = 0, MenuName = "Employee", rolName = RoleName, permission = false });
                        perm.Add(new Permission() { id = 0, MenuName = "Emotions", rolName = RoleName, permission = false });
                        perm.Add(new Permission() { id = 0, MenuName = "Assign Report Head", rolName = RoleName, permission = false });
                        perm.Add(new Permission() { id = 0, MenuName = "Reports", rolName = RoleName, permission = false });
                        perm.Add(new Permission() { id = 0, MenuName = "Company Registration", rolName = RoleName, permission = false });
                        perm.Add(new Permission() { id = 0, MenuName = "Email configuration", rolName = RoleName, permission = false });

                        return View(perm);
                    }
                }
                else
                {
                    return RedirectToAction("RoleIndex", "Account");
                }
            }
            else
            {
                return RedirectToAction("RoleIndex", "Account");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Permission(List<Permission> Permission)
        {
            int i = 0;
            foreach (Permission per in Permission)
            {
                if (i == 0)
                {
                    List<Permission> perm = db.Permission.ToList().Where(u => u.rolName == per.rolName).ToList();
                    if (perm.Count > 0)
                    {
                        db.Permission.RemoveRange(perm);
                        db.SaveChanges();
                    }
                }
                db.Permission.Add(per);
                db.SaveChanges();
                i++;
            }
            return RedirectToAction("RoleIndex", "Account");
        }

        public static bool IsAccess(string MenuName,string username ,UsersContext db)
        {
            string[] role = Roles.GetRolesForUser(username);
            if (role.Count() > 0)
            {
                var temp = role[0];
                Permission Perm = db.Permission.Where(u => u.rolName == temp.ToString() && u.MenuName == MenuName).SingleOrDefault();
                if (Perm.permission == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public ActionResult Noaccess()
        {
            return View();
        }
        #endregion
    }


}
