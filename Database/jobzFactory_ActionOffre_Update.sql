USE [jobzFactory];
GO

SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM dbo.ActionOffre WHERE id = 5)
BEGIN
    SET IDENTITY_INSERT dbo.ActionOffre ON;
    INSERT INTO dbo.ActionOffre (id, action) VALUES (5, N'Updated');
    SET IDENTITY_INSERT dbo.ActionOffre OFF;
END

IF NOT EXISTS (SELECT 1 FROM dbo.ActionOffre WHERE id = 6)
BEGIN
    SET IDENTITY_INSERT dbo.ActionOffre ON;
    INSERT INTO dbo.ActionOffre (id, action) VALUES (6, N'Deleted');
    SET IDENTITY_INSERT dbo.ActionOffre OFF;
END

UPDATE dbo.ActionOffre SET action = N'Created' WHERE id = 1 AND action = N'Création';
UPDATE dbo.ActionOffre SET action = N'Published' WHERE id = 2 AND action = N'Publication';
UPDATE dbo.ActionOffre SET action = N'Unpublished' WHERE id = 3 AND action = N'Désactivation';
UPDATE dbo.ActionOffre SET action = N'Approved' WHERE id = 4 AND action = N'Approbation';

PRINT N'ActionOffre types 5 (Updated) and 6 (Deleted) are ready.';
GO
