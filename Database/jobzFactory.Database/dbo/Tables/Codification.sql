CREATE TABLE [dbo].[Codification] (
    [id]           INT         IDENTITY (1, 1) NOT NULL,
    [annee]        VARCHAR (4) NULL,
    [offre_number] INT         NULL,
    CONSTRAINT [PK_Codification] PRIMARY KEY CLUSTERED ([id] ASC)
);
