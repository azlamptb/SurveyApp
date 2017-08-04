using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Happiness.Models
{
    public class EmailConfig
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [Required(ErrorMessage="Please enter smtp hostname")]
        public string smtphostname { get; set; }

        [Required(ErrorMessage ="Please enter smtp Port")]
        public long smtpPort { get; set; }

        [Required(ErrorMessage ="Please Choose SSL type")]
        public bool SSLEnabled { get; set; }

        [Required(ErrorMessage="Please enter the email address")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage="Please enter valid email address")]
        public string SendEmail { get; set; }

        [Required(ErrorMessage="Please enter password")]
        [DataType(DataType.Password)]
        public string Passowrd { get; set; }
    }
}