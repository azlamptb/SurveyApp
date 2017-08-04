using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Happiness.Models
{
    [Table("tbl_rptauthallocationMaster")]
    public class ReportingAuthorityAllocation
    {
        [Key]
        public long id { get; set; }

        [Index(IsUnique=true)]
        public long Report_id { get; set; }

        [ForeignKey("Report_id")]
        public ReportingAuthority ReportingAuthority { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? Modifieddate { get; set; }

        public string CreatedBy { get; set; }

        public string ModifiedBy { get; set; }

        [NotMapped]
        public string Reporting_auth_name { get; set; }
    }

    [Table("tbl_rptauthallocationChild")]
    public class ReportingAuthorityAllocationChild
    {
        [Key]
        public long id { get; set; }

        public long Report_MasterID { get; set; }

        [ForeignKey("Report_MasterID")]
        public ReportingAuthorityAllocation ReportingAuthority { get; set; }

        public int EmpID { get; set; }

        [ForeignKey("EmpID")]
        public UserProfile UserProfile { get; set; }

        public bool IsMailEnabled { get; set; }
    }

    public class ReportAllocationModel
    {
        [Key]
        public long id { get; set; }

        [Required]
        public long Report_id { get; set; }

        [ForeignKey("Report_id")]
        public ReportingAuthority ReportingAuthority { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? Modifieddate { get; set; }

        public string CreatedBy { get; set; }

        public string ModifiedBy { get; set; }

        public virtual List<ReportingAuthorityAllocationChild> ReportingAuthorityAllocationChild { get; set; }

    }
}