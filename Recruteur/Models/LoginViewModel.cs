using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Recruteur.Models
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Adresse Mail")]
        public string compte { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string motPasse { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}