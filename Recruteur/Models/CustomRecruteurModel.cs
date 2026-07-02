using System.Collections.Generic;

namespace Recruteur.Models
{
    /// <summary>
    /// Serialized into the FormsAuthentication ticket userData and rebuilt into
    /// a JF.DAL.Security.CustomPrincipal on each request.
    /// </summary>
    public class CustomRecruteurModel
    {
        public int id { get; set; }
        public string nom { get; set; }
        public string prenom { get; set; }
        public string recruteur { get; set; }
        public int? idRecruteur { get; set; }
        public IList<string> roles { get; set; }
    }
}
