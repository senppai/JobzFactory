CREATE TABLE [dbo].[Profil] (
    [id]            INT            IDENTITY (1, 1) NOT NULL,
    [nom]           NVARCHAR (50)  NOT NULL,
    [prenom]        NVARCHAR (50)  NOT NULL,
    [dateNaissance] DATE           NULL,
    [adresseMail]   NVARCHAR (200) NOT NULL,
    [gsm]           NVARCHAR (12)  NULL,
    [Tel]           NVARCHAR (12)  NULL,
    [motPasse]      NVARCHAR (256) NULL,
    [dateCreation]  DATETIME       NOT NULL,
    [titre]         NVARCHAR (200) NULL,
    [_idVille]      INT            NULL,
    [adresse]       NVARCHAR (200) NULL,
    [_idSecteur]    INT            NULL,
    CONSTRAINT [PK_Profil] PRIMARY KEY CLUSTERED ([id] ASC)
);
