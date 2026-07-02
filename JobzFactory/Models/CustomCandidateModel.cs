using System.Collections.Generic;

namespace JobzFactory.Models
{
    /// <summary>
    /// Serialized into the FormsAuthentication ticket UserData and rebuilt into a
    /// JF.DAL.Security.CustomPrincipal on each request (see Global.asax).
    /// </summary>
    public class CustomCandidateModel
    {
        public int id { get; set; }
        public string nom { get; set; }
        public string prenom { get; set; }
        public string adresseMail { get; set; }
        public IList<string> roles { get; set; }
    }
}
