CREATE TABLE [dbo].[Pays] (
    [id]   INT            IDENTITY (1, 1) NOT NULL,
    [pays] NVARCHAR (100) NULL,
    CONSTRAINT [PK_Pays] PRIMARY KEY CLUSTERED ([id] ASC)
);
