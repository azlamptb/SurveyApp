using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Globalization;
using System.Web.Security;

namespace Happiness.Models
{
    public class UsersContext : DbContext
    {
        public UsersContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<ReportingAuthority> ReportingAuthority { get; set; }
        public DbSet<ReportingAuthorityAllocation> ReportingAuthorityAllocation { get; set; }
        public DbSet<ReportingAuthorityAllocationChild> ReportingAuthorityAllocationChild { get; set; }
        public DbSet<Emotion> Emotion { get; set; }
        public DbSet<EmotionChild> EmotionChild { get; set; }
        public DbSet<Survey> Survey { get; set; }
        public DbSet<SurveyChild> SurveyChild { get; set; }
        public DbSet<CompanyModel> CompanyModel { get; set; }
        public DbSet<EmailConfig> EmailConfig { get; set; }
        public DbSet<Permission> Permission { get; set; }
        
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
        }




    }

    [Table("UserProfile")]
    public class UserProfile
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [MaxLength(20)]
        [Required(ErrorMessage = "Please enter User Name")]
        [Index(IsUnique = true)]
        public string UserName { get; set; }

        [MaxLength(20)]
        [Required(ErrorMessage = "Please enter Employee code")]
        [Index(IsUnique = true)]
        public string EmployeeCode { get; set; }

        [MaxLength(20)]
        [Required(ErrorMessage = "Please enter Employee Name")]
        [Index(IsUnique = true)]
        public string EmployeeName { get; set; }

        public long Emp_Reporting_Authority { get; set; }

        [ForeignKey("Emp_Reporting_Authority")]
        public ReportingAuthority ReportingAuthority { get; set; }

        public string Emp_Address { get; set; }

        public string Emp_Mob { get; set; }

        public string Emp_Tel { get; set; }

        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage="Please enter valid email address")]
        public string Emp_Email { get; set; }

        [Required]
        public bool Emp_isActive { get; set; }

        [NotMapped]
        public string RoleID { get; set; }

    }

    public class RegisterExternalLoginModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        public string ExternalLoginData { get; set; }
    }

    public class LocalPasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterModel
    {
        [Display(Name = "User name")]
        [Required(ErrorMessage = "Please enter UserName")]
        public string UserName { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Please enter Password")]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }


        [Index(IsUnique = true)]
        [Required(ErrorMessage = "Please enter Employee Code")]
        public string EmployeeCode { get; set; }

        [Required(ErrorMessage="Please enter Employee Name")]
        public string EmployeeName { get; set; }

        public long Emp_Reporting_Authority { get; set; }

        [ForeignKey("Emp_Reporting_Authority")]
        public ReportingAuthority ReportingAuthority { get; set; }

        public string Emp_Address { get; set; }

        public string Emp_Mob { get; set; }

        public string Emp_Tel { get; set; }

        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Please enter valid email address")]
        public string Emp_Email { get; set; }

        [Required]
        public bool Emp_isActive { get; set; }

        [NotMapped]
        public String RoleID { get; set; }
    }

    public class ExternalLogin
    {
        public string Provider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderUserId { get; set; }
    }
}
