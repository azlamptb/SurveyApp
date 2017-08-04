using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Happiness.Models
{
    [Table("tbl_emotion")]
    public class Emotion
    {
        [Key]
        public long id { get; set; }


        [MaxLength(20)]
        [Required(ErrorMessage = "Please enter Emotion Name")]
        [Index(IsUnique = true)]
        public string EmotionName { get; set; }

        public string EmotionIcon { get; set; }

        public bool IsQuestion_Available { get; set; }

        public bool Ismailenabled { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? Modifieddate { get; set; }

        public string CreatedBy { get; set; }

        public string ModifiedBy { get; set; }

    }

    [Table("tbl_questionforemotion")]
    public class EmotionChild
    {
        [Key]
        public long id { get; set; }

        public long EmotionID { get; set; }

        [ForeignKey("EmotionID")]
        public Emotion Emotion { get; set; }

        public string Questions { get; set; }

    }

    public class emotionModel
    {
        [Key]
        public long id { get; set; }

        [Required(ErrorMessage="Please enter Emotion Name")]
        [Index(IsUnique=true)]
        public string EmotionName { get; set; }

        [Required(ErrorMessage="Please select Emotion Icon")]
        public string EmotionIcon { get; set; }

        [Required]
        public bool IsQuestion_Available { get; set; }

        [Required]
        public bool Ismailenabled { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? Modifieddate { get; set; }

        public string CreatedBy { get; set; }

        public string ModifiedBy { get; set; }

        public virtual List<EmotionChild> Emotionchild { get; set; }

    }
}