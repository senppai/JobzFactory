using System.ComponentModel.DataAnnotations;

namespace Recruteur.Models
{
    public class RecruteurViewModel
    {
        [Required(ErrorMessage = "Company name is required.")]
        [StringLength(100)]
        public string recruteur { get; set; }

        [Required(ErrorMessage = "Company email is required.")]
        [EmailAddress]
        [StringLength(100)]
        public string adresseMail { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50)]
        public string nom { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50)]
        public string prenom { get; set; }

        [Required(ErrorMessage = "Work email is required.")]
        [EmailAddress]
        [StringLength(100)]
        public string adresseMailContact { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
        [DataType(DataType.Password)]
        public string motPasse { get; set; }

        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare("motPasse", ErrorMessage = "Passwords do not match.")]
        public string confirmMotPasse { get; set; }
    }
}
