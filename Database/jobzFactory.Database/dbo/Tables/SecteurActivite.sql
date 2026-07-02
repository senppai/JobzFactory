CREATE TABLE [dbo].[SecteurActivite] (
    [id]              INT           IDENTITY (1, 1) NOT NULL,
    [SecteurActivite] NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_SecteurActivite] PRIMARY KEY CLUSTERED ([id] ASC)
);
