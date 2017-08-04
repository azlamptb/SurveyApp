using Happiness.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using WebMatrix.WebData;

namespace Happiness.Controllers
{
    public class QuestionsForEmoji
    {
        #region Property

        public long id { get; set; }
        public long EmotionId { get; set; }
        public string Questions { get; set; }
        public string answers { get; set; }

        #endregion
    }
    public class happinessController : ApiController
    {
        // GET api/<controller>
        UsersContext db = new UsersContext();
        EmojidbEntities dbs = new EmojidbEntities();
        public IEnumerable<Emotion> GetEmotion()
        {
            List<Emotion> emo = db.Emotion.ToList();
            return emo;

        }

        // GET api/<controller>/5
        public IEnumerable<EmotionChild> GetQuestions(int id)
        {
            List<EmotionChild> questions = (from a in db.EmotionChild where a.EmotionID == id select a).ToList();
            return questions;
        }


        public long Getsum(int id, int s)
        {
            long a = id + s;
            return a;
        }

        // POST api/<controller>
        [System.Web.Http.HttpPost]
        public void applysurvey(string username, int count, long EmotionId, string Questions, string answers)
        {
            tbl_survey survey = new tbl_survey();
            string Question = Questions;
            string answer = answers;
            long emotionID = EmotionId;
            //  string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            UserProfile users = db.UserProfiles.ToList().Where(u => u.UserName == username).FirstOrDefault();

            survey.usr_id = users.UserId;
            survey.Emotion_id = emotionID;
            survey.Mailstatus = false;
            survey.CreatedDate = DateTime.Now.Date;
            survey.CreatedBy = users.UserName;
            dbs.tbl_survey.Add(survey);
            dbs.SaveChanges();

            if (count > 0)
            {
                SurveyChild childSurvey = new SurveyChild();
                childSurvey.Question = Question;
                childSurvey.Answers = answer;
                childSurvey.SurveyMaster_id = survey.id;
                db.SurveyChild.Add(childSurvey);
                db.SaveChanges();
            }
        }

        // PUT api/<controller>/5

        [System.Web.Http.HttpPost]
        public bool Authentication(string UserName, string Password)
        {
            if (WebSecurity.Login(UserName, Password, persistCookie: false))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}