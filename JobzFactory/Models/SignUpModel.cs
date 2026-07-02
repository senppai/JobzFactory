using System.ComponentModel.DataAnnotations;

namespace JobzFactory.Models
{
    public class SignUpModel
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50)]
        public string prenom { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50)]
        public string nom { get; set; }

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress]
        [StringLength(200)]
        public string adresseMail { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string motPasse { get; set; }

        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("motPasse", ErrorMessage = "The passwords do not match.")]
        public string confirmMotPasse { get; set; }
    }
}
