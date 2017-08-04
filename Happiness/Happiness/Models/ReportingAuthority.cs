using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Happiness.Models
{
    [Table("tbl_reportingAuthority")]
    public class ReportingAuthority
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }

        [MaxLength(20)]
        [Required(ErrorMessage="Please Enter Report authority Name")]
        [Index(IsUnique = true)]   
        public string Reporting_auth_name { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? Modifieddate { get; set; }

        public string CreatedBy { get; set; }

        public string ModifiedBy { get; set; }


    }
}