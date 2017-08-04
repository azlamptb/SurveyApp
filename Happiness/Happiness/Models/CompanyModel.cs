using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Happiness.Models
{
    public class CompanyModel
    {
        [Key]
        public long id { get; set; }

        [Required(ErrorMessage = "Please enter the company name")]
        public string CompanyName { get; set; }

        public string Address { get; set; }

        public string Mob { get; set; }

        public string Tel { get; set; }

        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Please Enter Valid Email address")]
        public string EmailId { get; set; }

        public string Fax { get; set; }
    }
}