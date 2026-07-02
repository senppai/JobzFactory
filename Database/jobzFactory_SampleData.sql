/*
  JobzFactory — sample Recruteur / Offre / Profil test data
  Run after jobzFactory_SeedData.sql

  Test logins (Recruteur portal):
    compte: recruteur.demo   motPasse: 123
    compte: tech.hr          motPasse: 123

  Test candidate (Profil portal):
    adresseMail: candidat.demo@test.local   motPasse: 123
*/

USE [jobzFactory];
GO

SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM dbo.Recruteur WHERE nomRecruteur = N'Demo Recrutement')
BEGIN
    DECLARE @now DATETIME = GETDATE();
    DECLARE @idVilleCasablanca INT = (SELECT TOP 1 id FROM dbo.Ville WHERE ville = N'Casablanca');
    DECLARE @idVilleRabat INT = (SELECT TOP 1 id FROM dbo.Ville WHERE ville = N'Rabat');
    DECLARE @idSecteurIT INT = (SELECT TOP 1 id FROM dbo.SecteurActivite WHERE SecteurActivite LIKE N'Informatique%');
    DECLARE @idSecteurRH INT = (SELECT TOP 1 id FROM dbo.SecteurActivite WHERE SecteurActivite LIKE N'Ressources%');

    /* Recruiters */
    INSERT INTO dbo.Recruteur (nomRecruteur, siteweb, adresseMail, dateCreation, url, description, adresseSocial)
    VALUES
        (N'Demo Recrutement', N'https://demo-recrutement.test', N'contact@demo-recrutement.test', @now,
         N'demo-recrutement', N'Société de démonstration pour tests locaux.', N'https://linkedin.com/company/demo-recrutement'),
        (N'Tech Solutions HR', N'https://tech-hr.test', N'hr@tech-hr.test', @now,
         N'tech-hr', N'Recrutement spécialisé IT.', NULL);

    DECLARE @idRecruteurDemo INT = (SELECT id FROM dbo.Recruteur WHERE nomRecruteur = N'Demo Recrutement');
    DECLARE @idRecruteurTech INT = (SELECT id FROM dbo.Recruteur WHERE nomRecruteur = N'Tech Solutions HR');

    INSERT INTO dbo.Recruteur_Contact (nom, prenom, adresseMail, compte, motPasse, dateCreation, profil, _idRecruteur)
    VALUES
        (N'Benali', N'Karim', N'karim@demo-recrutement.test', N'recruteur.demo', N'123', @now, 1, @idRecruteurDemo),
        (N'Alaoui', N'Sara', N'sara@tech-hr.test', N'tech.hr', N'123', @now, 1, @idRecruteurTech);

    /* Candidate profile */
    INSERT INTO dbo.Profil (nom, prenom, dateNaissance, adresseMail, gsm, Tel, motPasse, dateCreation, titre, _idVille, adresse, _idSecteur)
    VALUES
        (N'Martin', N'Alex', '1990-05-15', N'candidat.demo@test.local', N'0612345678', NULL, N'123', @now,
         N'Développeur .NET', @idVilleCasablanca, N'123 Avenue Test, Casablanca', @idSecteurIT);

    DECLARE @idProfil INT = (SELECT id FROM dbo.Profil WHERE adresseMail = N'candidat.demo@test.local');

    INSERT INTO dbo.Profil_CV (_idProfil, DateCreation, nomCV, extantion)
    VALUES (@idProfil, @now, N'CV_Alex_Martin', N'.pdf');

    /* Offer codification (numberOffre FK -> Codification.id) */
    DECLARE @annee VARCHAR(4) = CAST(YEAR(@now) AS VARCHAR(4));

    INSERT INTO dbo.Codification (annee, offre_number) VALUES (@annee, 1);
    DECLARE @cod1 INT = SCOPE_IDENTITY();

    INSERT INTO dbo.Codification (annee, offre_number) VALUES (@annee, 2);
    DECLARE @cod2 INT = SCOPE_IDENTITY();

    INSERT INTO dbo.Codification (annee, offre_number) VALUES (@annee, 3);
    DECLARE @cod3 INT = SCOPE_IDENTITY();

    /* Job offers */
    INSERT INTO dbo.Offre (message, _idRecruteur, _idVille, isPublie, titreOffre, _idSecteurActivite, url, isConfidentiel, adresseMailAPostuler, nomImage, numberOffre)
    VALUES
        (N'<p>Nous recherchons un développeur .NET pour renforcer notre équipe.</p><ul><li>C# / ASP.NET MVC</li><li>SQL Server</li><li>Entity Framework</li></ul>',
         @idRecruteurDemo, @idVilleCasablanca, 1, N'Développeur .NET Senior', @idSecteurIT,
         N'Développeur_.NET_Senior_1-' + @annee, 0, N'jobs@demo-recrutement.test', NULL, @cod1),

        (N'<p>Stage de 6 mois en développement web.</p>',
         @idRecruteurDemo, @idVilleRabat, 1, N'Stage Développeur Web', @idSecteurIT,
         N'Stage_Développeur_Web_2-' + @annee, 0, NULL, NULL, @cod2),

        (N'<p>Offre brouillon — non publiée.</p>',
         @idRecruteurTech, @idVilleCasablanca, 0, N'Chef de projet IT', @idSecteurIT,
         N'Chef_de_projet_IT_3-' + @annee, 0, N'hr@tech-hr.test', NULL, @cod3);

    DECLARE @idOffrePublie INT = (SELECT TOP 1 id FROM dbo.Offre WHERE titreOffre = N'Développeur .NET Senior');

    INSERT INTO dbo.Offre_Historique (_idOffre, dateAction, _idTypeAction, _idUtilisateur, commentaire)
    VALUES
        (@idOffrePublie, @now, 1, @idRecruteurDemo, NULL),
        (@idOffrePublie, DATEADD(MINUTE, 5, @now), 2, @idRecruteurDemo, NULL);

    INSERT INTO dbo.Offre_Postuler (_idOffre, _idProfil, DatePostuler, nomCV, message, extantion)
    VALUES
        (@idOffrePublie, @idProfil, @now, N'CV_Alex_Martin.pdf', N'Je suis intéressé par ce poste.', N'.pdf');

    PRINT N'Sample data inserted.';
END
ELSE
BEGIN
    PRINT N'Sample data already present — skipped.';
END
GO
