using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Happiness.Models;
using System.IO;
using System.Web.Helpers;
using System.Web.Hosting;
using System.Configuration;
using System.Data.Entity;
using System.Web.Security;

namespace Happiness.Controllers
{
    public class EmotionController : Controller
    {
        //
        // GET: /Emotion/
        UsersContext db = new UsersContext();

        public ActionResult Index()
        {
            if (Roles.IsUserInRole("admin") || AccountController.IsAccess("Emotions", User.Identity.Name, db))
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
            if (Roles.IsUserInRole("admin") || AccountController.IsAccess("Emotions", User.Identity.Name, db))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Noaccess", "Account");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(emotionModel emotionModel)
        {

            string reqhttp = ConfigurationManager.AppSettings["BaseUrl"].ToString();
            if (ModelState.IsValid)
            {
                var checkbox = Request.Form["IsQuestion_Available"];
                if (checkbox == "true")
                {
                    
                    try
                    {
                        Emotion emotion = new Emotion();
                        emotion.IsQuestion_Available = emotionModel.IsQuestion_Available = true;
                        emotion.Ismailenabled = emotionModel.Ismailenabled;
                        emotion.CreatedBy = emotionModel.CreatedBy = User.Identity.Name;
                        emotion.CreatedDate = emotionModel.CreatedDate = DateTime.Now;
                        emotion.Modifieddate = emotionModel.Modifieddate = null;
                        emotion.EmotionIcon = reqhttp+Url.Content(emotionModel.EmotionIcon); 
                        emotion.EmotionName = emotionModel.EmotionName;
                        db.Emotion.Add(emotion);
                        db.SaveChanges();
                        long emotionID = emotion.id;
                        foreach (EmotionChild child in emotionModel.Emotionchild)
                        {
                            child.EmotionID = emotionID;
                            db.EmotionChild.Add(child);
                            db.SaveChanges();
                        }
                        return RedirectToAction("Index");
                    }
                    catch(Exception ex)
                    {
                        ModelState.AddModelError("", "Emotion Name : " + emotionModel.EmotionName + " Already Exists");
                    }
                    

                }
                else
                {
                    try
                    {
                        Emotion emotion = new Emotion();
                        emotion.IsQuestion_Available = emotionModel.IsQuestion_Available = false;
                        emotion.Ismailenabled = emotionModel.Ismailenabled;
                        emotion.CreatedBy = emotionModel.CreatedBy = User.Identity.Name;
                        emotion.CreatedDate = emotionModel.CreatedDate = DateTime.Now;
                        emotion.Modifieddate = emotionModel.Modifieddate = null;
                        emotion.EmotionIcon =  reqhttp+Url.Content(emotionModel.EmotionIcon); 
                        emotion.EmotionName = emotionModel.EmotionName;
                        db.Emotion.Add(emotion);
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Emotion Name : " + emotionModel.EmotionName + " Already Exists");
                    }
                }

            }
            return View(emotionModel);
        }
        [HttpPost]
        public ActionResult Edit(emotionModel emotionModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string reqhttp = ConfigurationManager.AppSettings["BaseUrl"].ToString();
                    Emotion emoji = new Emotion();
                    emoji.id = emotionModel.id;
                    emoji.EmotionName = emotionModel.EmotionName;
                    if (Url.Content(emotionModel.EmotionIcon) == (from a in db.Emotion where a.id == emoji.id select a.EmotionIcon).SingleOrDefault())
                    {
                        emoji.EmotionIcon = Url.Content(emotionModel.EmotionIcon);
                    }
                    else
                    {
                        emoji.EmotionIcon = reqhttp + Url.Content(emotionModel.EmotionIcon);
                    }
                    emoji.Ismailenabled = emotionModel.Ismailenabled;
                    emoji.IsQuestion_Available = emotionModel.IsQuestion_Available;
                    emoji.ModifiedBy = User.Identity.Name;
                    emoji.Modifieddate = DateTime.Now;
                    db.Entry(emoji).State = EntityState.Modified;
                    db.SaveChanges();
                    if (emotionModel.IsQuestion_Available == true)
                    {
                        List<EmotionChild> emochild = db.EmotionChild.Where(u => u.EmotionID == emotionModel.id).ToList();
                        db.EmotionChild.RemoveRange(emochild);
                        db.SaveChanges();
                        foreach (EmotionChild emo in emotionModel.Emotionchild)
                        {
                            emo.EmotionID = emotionModel.id;
                            db.EmotionChild.Add(emo);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        List<EmotionChild> emochild = db.EmotionChild.Where(u => u.EmotionID == emotionModel.id).ToList();
                        db.EmotionChild.RemoveRange(emochild);
                        db.SaveChanges();
                    }

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
            return View(emotionModel);
        }

       
        [HttpGet]
        public ActionResult Edit(long id = 0)
        {
            if (Roles.IsUserInRole("admin") || AccountController.IsAccess("Emotions", User.Identity.Name, db))
            {
               
                Emotion Emotion = db.Emotion.Find(id);
                emotionModel emomodel = new emotionModel();
                if (Emotion.IsQuestion_Available == true)
                {
                    List<EmotionChild> emochild = db.EmotionChild.ToList().Where(u => u.EmotionID == Emotion.id).ToList();
                    ViewBag.child = emochild;
                    emomodel.Emotionchild = emochild;
                }
                emomodel.EmotionName = Emotion.EmotionName;
                emomodel.EmotionIcon = Emotion.EmotionIcon;
                emomodel.Ismailenabled = Emotion.Ismailenabled;
                emomodel.IsQuestion_Available = Emotion.IsQuestion_Available;
                // division.PhotoPath = "~/Upload/" + division.PhotoPath;
                if (Emotion == null)
                {
                    return HttpNotFound();
                }
                return View(emomodel);
            }
            else
            {
                return RedirectToAction("Noaccess", "Account");
            }
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult UploadFile()
        {
            string _imgname = string.Empty;
            if (System.Web.HttpContext.Current.Request.Files.AllKeys.Any())
            {
                string Dpath = Server.MapPath("/Upload");
                if (!Directory.Exists(Dpath))
                {
                    Directory.CreateDirectory(Dpath);
                }

                Dpath = Dpath + "/Emotion";
                if (!Directory.Exists(Dpath))
                {
                    Directory.CreateDirectory(Dpath);
                }

                var pic = System.Web.HttpContext.Current.Request.Files["MyImages"];
                if (pic.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(pic.FileName);
                    var _ext = Path.GetExtension(pic.FileName);

                    _imgname = Guid.NewGuid().ToString();
                    var _comPath = Server.MapPath("/Upload/Emotion/MVC_") + _imgname + _ext;
                    _imgname = "MVC_" + _imgname + _ext;

                    ViewBag.Msg = _comPath;
                    var path = _comPath;

                    // Saving Image in Original Mode
                    pic.SaveAs(path);

                    // resizing image
                    MemoryStream ms = new MemoryStream();
                    WebImage img = new WebImage(_comPath);

                    if (img.Width > 200)
                        img.Resize(200, 200);
                    img.Save(_comPath);
                    // end resize
                }
            }
            return Json(Convert.ToString(_imgname), JsonRequestBehavior.AllowGet);
        }
       

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            ReportingAuthorityAllocation division = db.ReportingAuthorityAllocation.Find(id);
            db.ReportingAuthorityAllocation.Remove(division);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpPost]
        public JsonResult DeleteAJAX(int divsID)
        {
            try
            {
                EmotionChild child = (from t in db.EmotionChild where t.EmotionID == divsID select t).FirstOrDefault();
                if (child != null)
                {
                    db.EmotionChild.Remove(child);
                    db.SaveChanges();
                }
                Emotion division = db.Emotion.Find(divsID);
                db.Emotion.Remove(division);
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
        
        private List<Emotion> FilterData(ref int recordFiltered, int start, int length, string search, int sortColumn, string sortDirection)
        {
            List<Emotion> list = new List<Emotion>();
            if (search == null)
            {
                list = db.Emotion.ToList();

            }
            else
            {
                // simulate search
                foreach (Emotion dataItem in db.Emotion.ToList())
                {
                    if (dataItem.EmotionName.ToUpper().Contains(search.ToUpper()))
                    {
                        list.Add(dataItem);
                    }
                }
            }

            // simulate sort
            if (sortColumn == 0)
            {// sort Name
                list.Sort((x, y) => SortString(x.EmotionName, y.EmotionName, sortDirection));
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
            public List<Emotion> data { get; set; }
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
