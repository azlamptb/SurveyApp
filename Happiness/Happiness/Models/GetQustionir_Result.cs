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
    
    public partial class GetQustionir_Result
    {
        public long id { get; set; }
        public string Question { get; set; }
        public string Answers { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string Reporting_auth_name { get; set; }
        public string EmotionName { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
    }
}
