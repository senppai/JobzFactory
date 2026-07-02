CREATE TABLE [dbo].[Ville] (
    [id]      INT           IDENTITY (1, 1) NOT NULL,
    [ville]   NVARCHAR (50) NULL,
    [_idPays] INT           NULL,
    CONSTRAINT [PK_Ville] PRIMARY KEY CLUSTERED ([id] ASC)
);
