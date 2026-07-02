using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JobzFactory.Models
{
    public class PostulerModel
    {
        //Offre.
        public int idOffre { get; set; }
        public string titreOffre { get; set; }
        public string offre { get; set; }
        public string url { get; set; }
        public string adresseMailAPostuler { get; set; }
        //Les infomrations profil.
        public string adresseMail { get; set; }
        public string gsm { get; set; }
        public string nom { get; set; }
        public string prenom { get; set; }
        public string tel { get; set; }
        public string nomCV { get; set; }
        public HttpPostedFileBase fichieCV { get; set; }
        public int? savedCvId { get; set; }
        public string message { get; set; }
    }
}