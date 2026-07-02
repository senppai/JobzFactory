/*
  JobzFactory — minimum reference data required by the application
  Derived from hard-coded _idTypeAction values in AnnonceController.cs
*/

USE [jobzFactory];
GO

SET NOCOUNT ON;

/* Offer history action types (ids 1–4 used in code) */
IF NOT EXISTS (SELECT 1 FROM dbo.ActionOffre)
BEGIN
    SET IDENTITY_INSERT dbo.ActionOffre ON;

    INSERT INTO dbo.ActionOffre (id, action) VALUES
        (1, N'Created'),
        (2, N'Published'),
        (3, N'Unpublished'),
        (4, N'Approved'),
        (5, N'Updated'),
        (6, N'Deleted');

    SET IDENTITY_INSERT dbo.ActionOffre OFF;
END
GO

/* Example geography — extend as needed for dropdowns */
IF NOT EXISTS (SELECT 1 FROM dbo.Pays)
BEGIN
    INSERT INTO dbo.Pays (pays) VALUES (N'Maroc');
END
GO

DECLARE @idPays INT = (SELECT TOP 1 id FROM dbo.Pays ORDER BY id);

IF NOT EXISTS (SELECT 1 FROM dbo.Ville)
BEGIN
    INSERT INTO dbo.Ville (ville, _idPays) VALUES
        (N'Casablanca', @idPays),
        (N'Rabat', @idPays),
        (N'Marrakech', @idPays),
        (N'Fès', @idPays),
        (N'Tanger', @idPays);
END
GO

/* Activity sectors for Offre / Profil dropdowns */
IF NOT EXISTS (SELECT 1 FROM dbo.SecteurActivite)
BEGIN
    INSERT INTO dbo.SecteurActivite (SecteurActivite) VALUES
        (N'Informatique / IT'),
        (N'Finance / Banque'),
        (N'Industrie'),
        (N'Commerce / Distribution'),
        (N'Santé'),
        (N'Éducation / Formation'),
        (N'BTP / Construction'),
        (N'Transport / Logistique'),
        (N'Marketing / Communication'),
        (N'Ressources humaines');
END
GO

PRINT N'Seed data applied.';
GO
