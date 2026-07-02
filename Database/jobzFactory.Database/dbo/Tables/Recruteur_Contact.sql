CREATE TABLE [dbo].[Recruteur_Contact] (
    [id]           INT            IDENTITY (1, 1) NOT NULL,
    [nom]          NVARCHAR (50)  NOT NULL,
    [prenom]       NVARCHAR (50)  NOT NULL,
    [adresseMail]  NVARCHAR (200) NULL,
    [compte]       NVARCHAR (200) NULL,
    [motPasse]     NVARCHAR (256) NULL,
    [dateCreation] DATETIME       NOT NULL,
    [profil]       INT            NULL,
    [_idRecruteur] INT            NULL,
    CONSTRAINT [PK_Recruteur_Contact] PRIMARY KEY CLUSTERED ([id] ASC)
);
