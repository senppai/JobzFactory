CREATE TABLE [dbo].[Profil_CV] (
    [id]           INT           IDENTITY (1, 1) NOT NULL,
    [_idProfil]    INT           NOT NULL,
    [DateCreation] DATETIME      NOT NULL,
    [nomCV]        NVARCHAR (50) NOT NULL,
    [extantion]    NVARCHAR (5)  NOT NULL,
    CONSTRAINT [PK_Profil_CV] PRIMARY KEY CLUSTERED ([id] ASC)
);
