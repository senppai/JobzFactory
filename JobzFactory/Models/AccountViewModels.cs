using System;
using System.ComponentModel.DataAnnotations;

namespace JobzFactory.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string adresseMail { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string motPasse { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string adresseMail { get; set; }
    }

    public class ResetViewModel
    {
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string motPasse { get; set; }

        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("motPasse", ErrorMessage = "The passwords do not match.")]
        public string confirmMotPasse { get; set; }

        public string token { get; set; }
    }

    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50)]
        [Display(Name = "First name")]
        public string prenom { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50)]
        [Display(Name = "Last name")]
        public string nom { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of birth")]
        public DateTime? dateNaissance { get; set; }

        [Display(Name = "Mobile")]
        [StringLength(12)]
        public string gsm { get; set; }

        [Display(Name = "Phone")]
        [StringLength(12)]
        public string Tel { get; set; }

        [Display(Name = "Address")]
        [StringLength(200)]
        public string adresse { get; set; }

        [Display(Name = "City")]
        public int? C_idVille { get; set; }

        [Display(Name = "Sector")]
        public int? C_idSecteur { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Your current password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string ancienMotPasse { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string nouveauMotPasse { get; set; }

        [Required(ErrorMessage = "Please confirm the new password.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("nouveauMotPasse", ErrorMessage = "The passwords do not match.")]
        public string confirmMotPasse { get; set; }
    }
}
