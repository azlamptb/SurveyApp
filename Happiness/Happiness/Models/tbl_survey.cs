//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Happiness.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class tbl_survey
    {
        public long id { get; set; }
        public int usr_id { get; set; }
        public long Emotion_id { get; set; }
        public bool Mailstatus { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> Modifieddate { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
    }
}
