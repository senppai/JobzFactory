/*
  JobzFactory — SQL Server database schema
  Reverse-engineered from JF.DAL/Context/Model.edmx (Entity Framework 6)

  Target: SQL Server 2012+ (ProviderManifestToken 2012)
  Database name: jobzFactory

  Run order:
    1. This script (schema)
    2. jobzFactory_SeedData.sql (reference data)
*/

USE master;
GO

IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'jobzFactory')
BEGIN
    CREATE DATABASE [jobzFactory];
END
GO

USE [jobzFactory];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ------------------------------------------------------------------ */
/* Lookup / reference tables (no incoming FKs)                        */
/* ------------------------------------------------------------------ */

IF OBJECT_ID(N'dbo.ActionOffre', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ActionOffre (
        id      INT IDENTITY(1,1) NOT NULL,
        action  NVARCHAR(50) NULL,
        CONSTRAINT PK_ActionOffre PRIMARY KEY CLUSTERED (id)
    );
END
GO

IF OBJECT_ID(N'dbo.Codification', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Codification (
        id            INT IDENTITY(1,1) NOT NULL,
        annee         VARCHAR(4) NULL,
        offre_number  INT NULL,
        CONSTRAINT PK_Codification PRIMARY KEY CLUSTERED (id)
    );
END
GO

IF OBJECT_ID(N'dbo.Pays', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Pays (
        id    INT IDENTITY(1,1) NOT NULL,
        pays  NVARCHAR(100) NULL,
        CONSTRAINT PK_Pays PRIMARY KEY CLUSTERED (id)
    );
END
GO

IF OBJECT_ID(N'dbo.SecteurActivite', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SecteurActivite (
        id               INT IDENTITY(1,1) NOT NULL,
        SecteurActivite  NVARCHAR(50) NOT NULL,
        CONSTRAINT PK_SecteurActivite PRIMARY KEY CLUSTERED (id)
    );
END
GO

IF OBJECT_ID(N'dbo.Recruteur', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Recruteur (
        id              INT IDENTITY(1,1) NOT NULL,
        nomRecruteur    NVARCHAR(50) NULL,
        siteweb         NVARCHAR(50) NULL,
        adresseMail     NVARCHAR(50) NULL,
        dateCreation    DATETIME NOT NULL,
        url             NVARCHAR(50) NULL,
        description     NVARCHAR(MAX) NULL,
        adresseSocial   NVARCHAR(200) NULL,
        CONSTRAINT PK_Recruteur PRIMARY KEY CLUSTERED (id)
    );
END
GO

/* ------------------------------------------------------------------ */
/* Geography                                                          */
/* ------------------------------------------------------------------ */

IF OBJECT_ID(N'dbo.Ville', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Ville (
        id       INT IDENTITY(1,1) NOT NULL,
        ville    NVARCHAR(50) NULL,
        _idPays  INT NULL,
        CONSTRAINT PK_Ville PRIMARY KEY CLUSTERED (id)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ville_Pays')
BEGIN
    ALTER TABLE dbo.Ville
        ADD CONSTRAINT FK_Ville_Pays
        FOREIGN KEY (_idPays) REFERENCES dbo.Pays (id)
        ON DELETE CASCADE;
END
GO

/* ------------------------------------------------------------------ */
/* Candidate profiles                                                 */
/* ------------------------------------------------------------------ */

IF OBJECT_ID(N'dbo.Profil', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Profil (
        id              INT IDENTITY(1,1) NOT NULL,
        nom             NVARCHAR(50) NOT NULL,
        prenom          NVARCHAR(50) NOT NULL,
        dateNaissance   DATE NULL,
        adresseMail     NVARCHAR(200) NOT NULL,
        gsm             NVARCHAR(12) NULL,
        Tel             NVARCHAR(12) NULL,
        motPasse        NVARCHAR(50) NULL,
        dateCreation    DATETIME NOT NULL,
        titre           NVARCHAR(200) NULL,
        _idVille        INT NULL,
        adresse         NVARCHAR(200) NULL,
        _idSecteur      INT NULL,
        CONSTRAINT PK_Profil PRIMARY KEY CLUSTERED (id)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Profil_Ville')
BEGIN
    ALTER TABLE dbo.Profil
        ADD CONSTRAINT FK_Profil_Ville
        FOREIGN KEY (_idVille) REFERENCES dbo.Ville (id);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Profil_SecteurActivite')
BEGIN
    ALTER TABLE dbo.Profil
        ADD CONSTRAINT FK_Profil_SecteurActivite
        FOREIGN KEY (_idSecteur) REFERENCES dbo.SecteurActivite (id);
END
GO

IF OBJECT_ID(N'dbo.Profil_CV', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Profil_CV (
        id            INT IDENTITY(1,1) NOT NULL,
        _idProfil     INT NOT NULL,
        DateCreation  DATETIME NOT NULL,
        nomCV         NVARCHAR(50) NOT NULL,
        extantion     NVARCHAR(5) NOT NULL,
        CONSTRAINT PK_Profil_CV PRIMARY KEY CLUSTERED (id)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Profil_CV_Profil')
BEGIN
    ALTER TABLE dbo.Profil_CV
        ADD CONSTRAINT FK_Profil_CV_Profil
        FOREIGN KEY (_idProfil) REFERENCES dbo.Profil (id)
        ON DELETE CASCADE;
END
GO

IF OBJECT_ID(N'dbo.Profil_Link', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Profil_Link (
        id               INT IDENTITY(1,1) NOT NULL,
        _idProfil        INT NOT NULL,
        dateGeneration   DATETIME NOT NULL,
        token            NVARCHAR(40) NOT NULL,
        dateExpiration   DATETIME NOT NULL,
        dateActivation   DATETIME NULL,
        etat             BIT NOT NULL,
        CONSTRAINT PK_Profil_Link PRIMARY KEY CLUSTERED (id)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Profil_Link_Profil')
BEGIN
    ALTER TABLE dbo.Profil_Link
        ADD CONSTRAINT FK_Profil_Link_Profil
        FOREIGN KEY (_idProfil) REFERENCES dbo.Profil (id)
        ON DELETE CASCADE;
END
GO

/* ------------------------------------------------------------------ */
/* Recruiter contacts                                                 */
/* ------------------------------------------------------------------ */

IF OBJECT_ID(N'dbo.Recruteur_Contact', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Recruteur_Contact (
        id            INT IDENTITY(1,1) NOT NULL,
        nom           NVARCHAR(50) NOT NULL,
        prenom        NVARCHAR(50) NOT NULL,
        adresseMail   NVARCHAR(200) NULL,
        compte        NVARCHAR(200) NULL,
        motPasse      NVARCHAR(50) NULL,
        dateCreation  DATETIME NOT NULL,
        profil        INT NULL,
        _idRecruteur  INT NULL,
        CONSTRAINT PK_Recruteur_Contact PRIMARY KEY CLUSTERED (id)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Recruteur_Contact_Recruteur')
BEGIN
    ALTER TABLE dbo.Recruteur_Contact
        ADD CONSTRAINT FK_Recruteur_Contact_Recruteur
        FOREIGN KEY (_idRecruteur) REFERENCES dbo.Recruteur (id)
        ON DELETE CASCADE;
END
GO

/* ------------------------------------------------------------------ */
/* Job offers                                                         */
/* ------------------------------------------------------------------ */

IF OBJECT_ID(N'dbo.Offre', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Offre (
        id                      INT IDENTITY(1,1) NOT NULL,
        message                 NVARCHAR(MAX) NOT NULL,
        _idRecruteur            INT NOT NULL,
        _idVille                INT NULL,
        isPublie                BIT NOT NULL,
        titreOffre              NVARCHAR(400) NULL,
        _idSecteurActivite      INT NULL,
        url                     NVARCHAR(400) NULL,
        isConfidentiel          BIT NULL,
        adresseMailAPostuler    NVARCHAR(100) NULL,
        nomImage                NVARCHAR(50) NULL,
        numberOffre             INT NULL,
        CONSTRAINT PK_Offre PRIMARY KEY CLUSTERED (id)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Offre_Recruteur')
BEGIN
    ALTER TABLE dbo.Offre
        ADD CONSTRAINT FK_Offre_Recruteur
        FOREIGN KEY (_idRecruteur) REFERENCES dbo.Recruteur (id)
        ON DELETE CASCADE;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Offre_Ville')
BEGIN
    ALTER TABLE dbo.Offre
        ADD CONSTRAINT FK_Offre_Ville
        FOREIGN KEY (_idVille) REFERENCES dbo.Ville (id);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Offre_SecteurActivite')
BEGIN
    ALTER TABLE dbo.Offre
        ADD CONSTRAINT FK_Offre_SecteurActivite
        FOREIGN KEY (_idSecteurActivite) REFERENCES dbo.SecteurActivite (id);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK__Offre__numberOff__5224328E')
BEGIN
    ALTER TABLE dbo.Offre
        ADD CONSTRAINT FK__Offre__numberOff__5224328E
        FOREIGN KEY (numberOffre) REFERENCES dbo.Codification (id);
END
GO

IF OBJECT_ID(N'dbo.Offre_Historique', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Offre_Historique (
        id               INT IDENTITY(1,1) NOT NULL,
        _idOffre         INT NOT NULL,
        dateAction       DATETIME NOT NULL,
        _idTypeAction    INT NOT NULL,
        _idUtilisateur   INT NOT NULL,
        commentaire      NVARCHAR(300) NULL,
        CONSTRAINT PK_Offre_Historique PRIMARY KEY CLUSTERED (id)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Offre_Historique_Offre')
BEGIN
    ALTER TABLE dbo.Offre_Historique
        ADD CONSTRAINT FK_Offre_Historique_Offre
        FOREIGN KEY (_idOffre) REFERENCES dbo.Offre (id)
        ON DELETE CASCADE;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Offre_Historique_ActionOffre')
BEGIN
    ALTER TABLE dbo.Offre_Historique
        ADD CONSTRAINT FK_Offre_Historique_ActionOffre
        FOREIGN KEY (_idTypeAction) REFERENCES dbo.ActionOffre (id);
END
GO

IF OBJECT_ID(N'dbo.Offre_Postuler', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Offre_Postuler (
        id             INT IDENTITY(1,1) NOT NULL,
        _idOffre       INT NOT NULL,
        _idProfil      INT NOT NULL,
        DatePostuler   DATETIME NOT NULL,
        nomCV          NVARCHAR(300) NOT NULL,
        message        NVARCHAR(MAX) NULL,
        extantion      NVARCHAR(5) NOT NULL,
        CONSTRAINT PK_Offre_Postuler PRIMARY KEY CLUSTERED (id)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Offre_Postuler_Offre')
BEGIN
    ALTER TABLE dbo.Offre_Postuler
        ADD CONSTRAINT FK_Offre_Postuler_Offre
        FOREIGN KEY (_idOffre) REFERENCES dbo.Offre (id)
        ON DELETE CASCADE;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Offre_Postuler_Profil')
BEGIN
    ALTER TABLE dbo.Offre_Postuler
        ADD CONSTRAINT FK_Offre_Postuler_Profil
        FOREIGN KEY (_idProfil) REFERENCES dbo.Profil (id);
END
GO

/* ------------------------------------------------------------------ */
/* SQL Server diagram storage (optional, from original DB)            */
/* ------------------------------------------------------------------ */

IF OBJECT_ID(N'dbo.sysdiagrams', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.sysdiagrams (
        name           NVARCHAR(128) NOT NULL,
        principal_id   INT NOT NULL,
        diagram_id     INT IDENTITY(1,1) NOT NULL,
        version        INT NULL,
        definition     VARBINARY(MAX) NULL,
        CONSTRAINT PK_sysdiagrams PRIMARY KEY CLUSTERED (diagram_id),
        CONSTRAINT UK_principal_name UNIQUE (principal_id, name)
    );
END
GO

PRINT N'jobzFactory schema created or already present.';
GO
