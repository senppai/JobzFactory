using System;
using JF.DAL.Context;

namespace Recruteur.Models
{
    public static class AnnonceHistoryHelper
    {
        public const int ActionCreate = 1;
        public const int ActionPublish = 2;
        public const int ActionUnpublish = 3;
        public const int ActionApprove = 4;
        public const int ActionUpdate = 5;
        public const int ActionDelete = 6;

        public static void Add(jobzFactoryEntities db, int offreId, int actionTypeId, string comment = null)
        {
            var recruiterId = VarSessionRecruteur.compteRecruteur.C_idRecruteur.Value;

            db.Offre_Historique.Add(new Offre_Historique
            {
                C_idOffre = offreId,
                dateAction = DateTime.Now,
                C_idTypeAction = actionTypeId,
                C_idUtilisateur = recruiterId,
                commentaire = comment
            });
        }
    }
}
