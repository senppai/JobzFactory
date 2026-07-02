CREATE TABLE [dbo].[ActionOffre] (
    [id]     INT           IDENTITY (1, 1) NOT NULL,
    [action] NVARCHAR (50) NULL,
    CONSTRAINT [PK_ActionOffre] PRIMARY KEY CLUSTERED ([id] ASC)
);
