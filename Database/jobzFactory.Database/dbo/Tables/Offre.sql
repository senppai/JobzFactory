CREATE TABLE [dbo].[Offre] (
    [id]                   INT            IDENTITY (1, 1) NOT NULL,
    [message]              NVARCHAR (MAX) NOT NULL,
    [_idRecruteur]         INT            NOT NULL,
    [_idVille]             INT            NULL,
    [isPublie]             BIT            NOT NULL,
    [titreOffre]           NVARCHAR (400) NULL,
    [_idSecteurActivite]    INT            NULL,
    [url]                  NVARCHAR (400) NULL,
    [isConfidentiel]       BIT            NULL,
    [adresseMailAPostuler] NVARCHAR (100) NULL,
    [nomImage]             NVARCHAR (50)  NULL,
    [numberOffre]          INT            NULL,
    CONSTRAINT [PK_Offre] PRIMARY KEY CLUSTERED ([id] ASC)
);
