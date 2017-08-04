using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Happiness.Models
{
    [Table("tbl_survey")]
    public class Survey
    {
        [Key]
        public long id { get; set; }

        public int usr_id { get; set; }

        [ForeignKey("usr_id")]
        public UserProfile UserProfile { get; set; }

        public long Emotion_id { get; set; }

        [ForeignKey("Emotion_id")]
        public Emotion Emotion { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? Modifieddate { get; set; }

        public string CreatedBy { get; set; }

        public string ModifiedBy { get; set; }

    }

    public class SurveyChild
    {
        [Key]
        public long id { get; set; }

        public long SurveyMaster_id { get; set; }

        [ForeignKey("SurveyMaster_id")]
        public Survey Survey { get; set; }

        public string Question { get; set; }

        public string Answers { get; set; }
    }

    public class SurveyModel
    {
        [Key]
        public long id { get; set; }

        [Required]
        public int usr_id { get; set; }

        [ForeignKey("usr_id")]
        public UserProfile UserProfile { get; set; }

        [Required]
        public long Emotion_id { get; set; }

        [ForeignKey("Emotion_id")]
        public Emotion Emotion { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime Modifieddate { get; set; }

        public string CreatedBy { get; set; }

        public string ModifiedBy { get; set; }

        public virtual ICollection<SurveyChild> surveyChild { get; set; }
    }
}