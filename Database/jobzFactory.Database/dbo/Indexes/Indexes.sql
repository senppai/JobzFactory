-- Non-clustered indexes and unique constraints for JobzFactory.
-- Published by the SSDT project alongside the table definitions.

-- Speeds up the public job listing (filtered by isPublie).
CREATE NONCLUSTERED INDEX [IX_Offre_isPublie]
    ON [dbo].[Offre] ([isPublie] ASC);
GO

-- Public job detail lookup by url; uniqueness prevents duplicate slugs.
CREATE UNIQUE NONCLUSTERED INDEX [UX_Offre_url]
    ON [dbo].[Offre] ([url] ASC)
    WHERE [url] IS NOT NULL;
GO

-- Employer login lookup by compte; uniqueness prevents duplicate accounts.
CREATE UNIQUE NONCLUSTERED INDEX [UX_Recruteur_Contact_compte]
    ON [dbo].[Recruteur_Contact] ([compte] ASC)
    WHERE [compte] IS NOT NULL;
GO

-- Candidate signup / apply lookup by email; uniqueness prevents duplicate profiles.
CREATE UNIQUE NONCLUSTERED INDEX [UX_Profil_adresseMail]
    ON [dbo].[Profil] ([adresseMail] ASC);
GO

-- Prevent a candidate from applying to the same offer twice.
CREATE UNIQUE NONCLUSTERED INDEX [UX_Offre_Postuler_Offre_Profil]
    ON [dbo].[Offre_Postuler] ([_idOffre] ASC, [_idProfil] ASC);
GO

-- Recruiter-scoped offer queries.
CREATE NONCLUSTERED INDEX [IX_Offre_Recruteur]
    ON [dbo].[Offre] ([_idRecruteur] ASC);
GO
