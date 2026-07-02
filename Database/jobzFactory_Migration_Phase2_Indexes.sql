/*
======================================================================================
 JobzFactory — Phase 2 indexes (companion to jobzFactory_Migration_Phase2.sql)
======================================================================================
 Filtered indexes / indexes on computed columns require QUOTED_IDENTIFIER ON and
 ANSI_NULLS ON at execution time. sqlcmd does not always enable QUOTED_IDENTIFIER
 by default, which caused Msg 1934 when running the main migration. This script sets
 the required options explicitly and is idempotent (safe to re-run).
======================================================================================
*/

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Offre_isPublie' AND object_id = OBJECT_ID('dbo.Offre'))
    CREATE NONCLUSTERED INDEX [IX_Offre_isPublie] ON [dbo].[Offre] ([isPublie] ASC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Offre_url' AND object_id = OBJECT_ID('dbo.Offre'))
    CREATE UNIQUE NONCLUSTERED INDEX [UX_Offre_url] ON [dbo].[Offre] ([url] ASC) WHERE [url] IS NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Recruteur_Contact_compte' AND object_id = OBJECT_ID('dbo.Recruteur_Contact'))
    CREATE UNIQUE NONCLUSTERED INDEX [UX_Recruteur_Contact_compte] ON [dbo.Recruteur_Contact] ([compte] ASC) WHERE [compte] IS NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Profil_adresseMail' AND object_id = OBJECT_ID('dbo.Profil'))
    CREATE UNIQUE NONCLUSTERED INDEX [UX_Profil_adresseMail] ON [dbo].[Profil] ([adresseMail] ASC) WHERE [adresseMail] IS NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Offre_Postuler_Offre_Profil' AND object_id = OBJECT_ID('dbo.Offre_Postuler'))
    CREATE UNIQUE NONCLUSTERED INDEX [UX_Offre_Postuler_Offre_Profil] ON [dbo].[Offre_Postuler] ([_idOffre] ASC, [_idProfil] ASC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Offre_Recruteur' AND object_id = OBJECT_ID('dbo.Offre'))
    CREATE NONCLUSTERED INDEX [IX_Offre_Recruteur] ON [dbo].[Offre] ([_idRecruteur] ASC);
GO
