/*
  JobzFactory — Reassign the 40 English job listings to recruteur.demo
  Run after jobzFactory_EnglishJobs.sql if jobs were seeded to fictional companies.
*/

USE [jobzFactory];
GO

SET NOCOUNT ON;

BEGIN TRANSACTION;

BEGIN TRY

    DECLARE @idRecruteurDemo INT = (
        SELECT TOP 1 rc._idRecruteur
        FROM dbo.Recruteur_Contact rc
        WHERE rc.compte = N'recruteur.demo'
    );

    IF @idRecruteurDemo IS NULL
        THROW 50001, N'recruteur.demo account not found — run jobzFactory_SampleData.sql first.', 1;

    DECLARE @EnglishRecruiters TABLE (nom NVARCHAR(200) PRIMARY KEY);
    INSERT INTO @EnglishRecruiters (nom) VALUES
        (N'Atlas Digital Group'),
        (N'Meridian Bank'),
        (N'Horizon Logistics'),
        (N'Nova Health Systems'),
        (N'Summit Consulting'),
        (N'Pacific Retail Group'),
        (N'Vertex Engineering'),
        (N'BrightPath Education'),
        (N'Sterling Finance Partners'),
        (N'CoreBuild Construction'),
        (N'Pulse Marketing Agency'),
        (N'Nexus Technology Partners');

    UPDATE o
    SET o._idRecruteur = @idRecruteurDemo
    FROM dbo.Offre o
    INNER JOIN dbo.Recruteur r ON r.id = o._idRecruteur
    WHERE r.nomRecruteur IN (SELECT nom FROM @EnglishRecruiters);

    DECLARE @updated INT = @@ROWCOUNT;

    COMMIT TRANSACTION;
    PRINT N'Reassigned ' + CAST(@updated AS NVARCHAR(10)) + N' job(s) to recruteur.demo (Demo Recrutement).';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH
GO
