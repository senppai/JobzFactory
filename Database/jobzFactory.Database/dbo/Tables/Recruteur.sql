CREATE TABLE [dbo].[Recruteur] (
    [id]            INT            IDENTITY (1, 1) NOT NULL,
    [nomRecruteur]  NVARCHAR (50)  NULL,
    [siteweb]       NVARCHAR (50)  NULL,
    [adresseMail]   NVARCHAR (50)  NULL,
    [dateCreation]  DATETIME       NOT NULL,
    [url]           NVARCHAR (50)  NULL,
    [description]   NVARCHAR (MAX) NULL,
    [adresseSocial] NVARCHAR (200) NULL,
    CONSTRAINT [PK_Recruteur] PRIMARY KEY CLUSTERED ([id] ASC)
);
