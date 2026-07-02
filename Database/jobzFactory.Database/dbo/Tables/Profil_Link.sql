CREATE TABLE [dbo].[Profil_Link] (
    [id]             INT           IDENTITY (1, 1) NOT NULL,
    [_idProfil]      INT           NOT NULL,
    [dateGeneration] DATETIME      NOT NULL,
    [token]          NVARCHAR (40) NOT NULL,
    [dateExpiration] DATETIME      NOT NULL,
    [dateActivation] DATETIME      NULL,
    [etat]           BIT           NOT NULL,
    CONSTRAINT [PK_Profil_Link] PRIMARY KEY CLUSTERED ([id] ASC)
);
