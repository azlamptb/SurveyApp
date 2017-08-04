using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Happiness.Models
{
  
    public class Permission
    {
        [Key]
        public long id { get; set; }

        [Required(ErrorMessage="Please Select Role name")]
        public string rolName { get; set; }

        [Required]
        public string MenuName { get; set; }

        [Required]
        public bool permission { get; set; }
        

    }
}