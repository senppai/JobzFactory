ALTER TABLE [dbo].[Ville]
    ADD CONSTRAINT [FK_Ville_Pays] FOREIGN KEY ([_idPays]) REFERENCES [dbo].[Pays] ([id]) ON DELETE CASCADE;
GO

ALTER TABLE [dbo].[Profil]
    ADD CONSTRAINT [FK_Profil_Ville] FOREIGN KEY ([_idVille]) REFERENCES [dbo].[Ville] ([id]);
GO

ALTER TABLE [dbo].[Profil]
    ADD CONSTRAINT [FK_Profil_SecteurActivite] FOREIGN KEY ([_idSecteur]) REFERENCES [dbo].[SecteurActivite] ([id]);
GO

ALTER TABLE [dbo].[Profil_CV]
    ADD CONSTRAINT [FK_Profil_CV_Profil] FOREIGN KEY ([_idProfil]) REFERENCES [dbo].[Profil] ([id]) ON DELETE CASCADE;
GO

ALTER TABLE [dbo].[Profil_Link]
    ADD CONSTRAINT [FK_Profil_Link_Profil] FOREIGN KEY ([_idProfil]) REFERENCES [dbo].[Profil] ([id]) ON DELETE CASCADE;
GO

ALTER TABLE [dbo].[Recruteur_Contact]
    ADD CONSTRAINT [FK_Recruteur_Contact_Recruteur] FOREIGN KEY ([_idRecruteur]) REFERENCES [dbo].[Recruteur] ([id]) ON DELETE CASCADE;
GO

ALTER TABLE [dbo].[Offre]
    ADD CONSTRAINT [FK_Offre_Recruteur] FOREIGN KEY ([_idRecruteur]) REFERENCES [dbo].[Recruteur] ([id]) ON DELETE CASCADE;
GO

ALTER TABLE [dbo].[Offre]
    ADD CONSTRAINT [FK_Offre_Ville] FOREIGN KEY ([_idVille]) REFERENCES [dbo].[Ville] ([id]);
GO

ALTER TABLE [dbo].[Offre]
    ADD CONSTRAINT [FK_Offre_SecteurActivite] FOREIGN KEY ([_idSecteurActivite]) REFERENCES [dbo].[SecteurActivite] ([id]);
GO

ALTER TABLE [dbo].[Offre]
    ADD CONSTRAINT [FK__Offre__numberOff__5224328E] FOREIGN KEY ([numberOffre]) REFERENCES [dbo].[Codification] ([id]);
GO

ALTER TABLE [dbo].[Offre_Historique]
    ADD CONSTRAINT [FK_Offre_Historique_Offre] FOREIGN KEY ([_idOffre]) REFERENCES [dbo].[Offre] ([id]) ON DELETE CASCADE;
GO

ALTER TABLE [dbo].[Offre_Historique]
    ADD CONSTRAINT [FK_Offre_Historique_ActionOffre] FOREIGN KEY ([_idTypeAction]) REFERENCES [dbo].[ActionOffre] ([id]);
GO

ALTER TABLE [dbo].[Offre_Postuler]
    ADD CONSTRAINT [FK_Offre_Postuler_Offre] FOREIGN KEY ([_idOffre]) REFERENCES [dbo].[Offre] ([id]) ON DELETE CASCADE;
GO

ALTER TABLE [dbo].[Offre_Postuler]
    ADD CONSTRAINT [FK_Offre_Postuler_Profil] FOREIGN KEY ([_idProfil]) REFERENCES [dbo].[Profil] ([id]);
GO
