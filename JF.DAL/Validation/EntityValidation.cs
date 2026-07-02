using System.ComponentModel.DataAnnotations;

namespace JF.DAL.Context
{
    // Non-generated partials that attach validation metadata to the EF6
    // Database-First entities (regenerating the T4 template will not overwrite these).
    // This makes ModelState.IsValid meaningful in the controllers that bind entities.

    [MetadataType(typeof(OffreMetadata))]
    public partial class Offre
    {
        public class OffreMetadata
        {
            [Required(ErrorMessage = "Title is required.")]
            [StringLength(150, ErrorMessage = "Title must be 150 characters or fewer.")]
            public string titreOffre { get; set; }

            [Required(ErrorMessage = "Description is required.")]
            [StringLength(8000)]
            public string message { get; set; }

            [StringLength(200)]
            [EmailAddress(ErrorMessage = "Application email is not valid.")]
            public string adresseMailAPostuler { get; set; }

            [Required]
            public int C_idRecruteur { get; set; }

            // url is server-generated, so it is not marked Required.
            [StringLength(200)]
            public string url { get; set; }

            [StringLength(150)]
            public string nomImage { get; set; }
        }
    }

    [MetadataType(typeof(RecruteurMetadata))]
    public partial class Recruteur
    {
        public class RecruteurMetadata
        {
            [Required(ErrorMessage = "Company name is required.")]
            [StringLength(120)]
            public string nomRecruteur { get; set; }

            [StringLength(200)]
            [EmailAddress(ErrorMessage = "Company email is not valid.")]
            public string adresseMail { get; set; }

            [StringLength(300)]
            [Url(ErrorMessage = "Website is not a valid URL.")]
            public string siteweb { get; set; }

            // url is server-generated, so it is not marked Required.
            [StringLength(200)]
            public string url { get; set; }

            [StringLength(2000)]
            public string description { get; set; }

            [StringLength(200)]
            public string adresseSocial { get; set; }
        }
    }

    [MetadataType(typeof(Recruteur_ContactMetadata))]
    public partial class Recruteur_Contact
    {
        public class Recruteur_ContactMetadata
        {
            [Required(ErrorMessage = "Username is required.")]
            [StringLength(100)]
            public string compte { get; set; }

            [Required(ErrorMessage = "Password is required.")]
            [StringLength(256)]
            public string motPasse { get; set; }

            [Required]
            [StringLength(50)]
            public string nom { get; set; }

            [Required]
            [StringLength(50)]
            public string prenom { get; set; }

            [StringLength(100)]
            [EmailAddress]
            public string adresseMail { get; set; }
        }
    }

    [MetadataType(typeof(ProfilMetadata))]
    public partial class Profil
    {
        public class ProfilMetadata
        {
            [Required]
            [StringLength(50)]
            public string nom { get; set; }

            [Required]
            [StringLength(50)]
            public string prenom { get; set; }

            [Required]
            [StringLength(200)]
            [EmailAddress]
            public string adresseMail { get; set; }

            [StringLength(20)]
            public string gsm { get; set; }

            [StringLength(20)]
            public string Tel { get; set; }
        }
    }
}
