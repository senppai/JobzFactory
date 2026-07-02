CREATE TABLE [dbo].[Offre_Postuler] (
    [id]           INT            IDENTITY (1, 1) NOT NULL,
    [_idOffre]     INT            NOT NULL,
    [_idProfil]    INT            NOT NULL,
    [DatePostuler] DATETIME       NOT NULL,
    [nomCV]        NVARCHAR (300) NOT NULL,
    [message]      NVARCHAR (MAX) NULL,
    [extantion]    NVARCHAR (5)   NOT NULL,
    CONSTRAINT [PK_Offre_Postuler] PRIMARY KEY CLUSTERED ([id] ASC)
);
