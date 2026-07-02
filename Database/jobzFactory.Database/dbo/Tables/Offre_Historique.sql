CREATE TABLE [dbo].[Offre_Historique] (
    [id]             INT            IDENTITY (1, 1) NOT NULL,
    [_idOffre]       INT            NOT NULL,
    [dateAction]     DATETIME       NOT NULL,
    [_idTypeAction]  INT            NOT NULL,
    [_idUtilisateur] INT            NOT NULL,
    [commentaire]    NVARCHAR (300) NULL,
    CONSTRAINT [PK_Offre_Historique] PRIMARY KEY CLUSTERED ([id] ASC)
);
