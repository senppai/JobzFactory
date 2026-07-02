/*
======================================================================================
 JobzFactory — Phase 2/3 migration for EXISTING databases
======================================================================================
 Run this once against an existing jobzFactory database before deploying the
 upgraded apps. New databases created from the DACPAC already include these changes.

 What it does:
  1. Widens Recruteur_Contact.motPasse and Profil.motPasse to NVARCHAR(256) so the
     PBKDF2 password hashes fit (the old NVARCHAR(50) column truncated them).
  2. Adds the missing non-clustered indexes and unique constraints.

 Existing plaintext passwords are NOT touched here. The login flow upgrades a
 plaintext password to a PBKDF2 hash on the first successful login, so existing
 accounts keep working with their current password (no forced reset needed).
======================================================================================
*/

-- 1. Widen password columns (idempotent: safe to re-run).
IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Recruteur_Contact' AND COLUMN_NAME = 'motPasse'
      AND CHARACTER_MAXIMUM_LENGTH <> 256)
BEGIN
    ALTER TABLE [dbo].[Recruteur_Contact] ALTER COLUMN [motPasse] NVARCHAR (256) NULL;
END
GO

IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Profil' AND COLUMN_NAME = 'motPasse'
      AND CHARACTER_MAXIMUM_LENGTH <> 256)
BEGIN
    ALTER TABLE [dbo].[Profil] ALTER COLUMN [motPasse] NVARCHAR (256) NULL;
END
GO

-- 2. Indexes (created only if they do not already exist).
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Offre_isPublie' AND object_id = OBJECT_ID('dbo.Offre'))
    CREATE NONCLUSTERED INDEX [IX_Offre_isPublie] ON [dbo].[Offre] ([isPublie] ASC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Offre_url' AND object_id = OBJECT_ID('dbo.Offre'))
    CREATE UNIQUE NONCLUSTERED INDEX [UX_Offre_url] ON [dbo].[Offre] ([url] ASC) WHERE [url] IS NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Recruteur_Contact_compte' AND object_id = OBJECT_ID('dbo.Recruteur_Contact'))
    CREATE UNIQUE NONCLUSTERED INDEX [UX_Recruteur_Contact_compte] ON [dbo].[Recruteur_Contact] ([compte] ASC) WHERE [compte] IS NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Profil_adresseMail' AND object_id = OBJECT_ID('dbo.Profil'))
    CREATE UNIQUE NONCLUSTERED INDEX [UX_Profil_adresseMail] ON [dbo].[Profil] ([adresseMail] ASC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Offre_Postuler_Offre_Profil' AND object_id = OBJECT_ID('dbo.Offre_Postuler'))
    CREATE UNIQUE NONCLUSTERED INDEX [UX_Offre_Postuler_Offre_Profil] ON [dbo].[Offre_Postuler] ([_idOffre] ASC, [_idProfil] ASC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Offre_Recruteur' AND object_id = OBJECT_ID('dbo.Offre'))
    CREATE NONCLUSTERED INDEX [IX_Offre_Recruteur] ON [dbo].[Offre] ([_idRecruteur] ASC);
GO
