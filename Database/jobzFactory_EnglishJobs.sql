/*
  JobzFactory — Replace demo French jobs with 40 professional English listings
  Run against: jobzFactory database (local SQL Server)
*/

USE [jobzFactory];
GO

SET NOCOUNT ON;

BEGIN TRANSACTION;

BEGIN TRY

    /* ── English reference labels ── */
    UPDATE dbo.Pays SET pays = N'Morocco' WHERE pays = N'Maroc';

    UPDATE dbo.SecteurActivite SET SecteurActivite = N'Information Technology' WHERE SecteurActivite = N'Informatique / IT';
    UPDATE dbo.SecteurActivite SET SecteurActivite = N'Finance & Banking' WHERE SecteurActivite = N'Finance / Banque';
    UPDATE dbo.SecteurActivite SET SecteurActivite = N'Industry & Manufacturing' WHERE SecteurActivite = N'Industrie';
    UPDATE dbo.SecteurActivite SET SecteurActivite = N'Retail & Distribution' WHERE SecteurActivite = N'Commerce / Distribution';
    UPDATE dbo.SecteurActivite SET SecteurActivite = N'Healthcare' WHERE id = 5;
    UPDATE dbo.SecteurActivite SET SecteurActivite = N'Education & Training' WHERE id = 6;
    UPDATE dbo.SecteurActivite SET SecteurActivite = N'Construction & Engineering' WHERE SecteurActivite = N'BTP / Construction';
    UPDATE dbo.SecteurActivite SET SecteurActivite = N'Transport & Logistics' WHERE SecteurActivite = N'Transport / Logistique';
    UPDATE dbo.SecteurActivite SET SecteurActivite = N'Marketing & Communications' WHERE SecteurActivite = N'Marketing / Communication';
    UPDATE dbo.SecteurActivite SET SecteurActivite = N'Human Resources' WHERE SecteurActivite = N'Ressources humaines';

    /* ── Remove the two demo published jobs ── */
    DECLARE @DemoOfferIds TABLE (id INT PRIMARY KEY, numberOffre INT NULL);

    INSERT INTO @DemoOfferIds (id, numberOffre)
    SELECT id, numberOffre
    FROM dbo.Offre
    WHERE titreOffre IN (N'Développeur .NET Senior', N'Stage Développeur Web');

    DELETE op
    FROM dbo.Offre_Postuler op
    INNER JOIN @DemoOfferIds d ON d.id = op._idOffre;

    DELETE oh
    FROM dbo.Offre_Historique oh
    INNER JOIN @DemoOfferIds d ON d.id = oh._idOffre;

    DELETE o
    FROM dbo.Offre o
    INNER JOIN @DemoOfferIds d ON d.id = o.id;

    DELETE c
    FROM dbo.Codification c
    INNER JOIN @DemoOfferIds d ON d.numberOffre = c.id
    WHERE d.numberOffre IS NOT NULL;

    /* ── Lookup ids ── */
    DECLARE @now DATETIME = GETDATE();
    DECLARE @annee VARCHAR(4) = CAST(YEAR(@now) AS VARCHAR(4));

    DECLARE @idCasablanca INT = (SELECT TOP 1 id FROM dbo.Ville WHERE ville = N'Casablanca');
    DECLARE @idRabat INT = (SELECT TOP 1 id FROM dbo.Ville WHERE ville = N'Rabat');
    DECLARE @idMarrakech INT = (SELECT TOP 1 id FROM dbo.Ville WHERE ville = N'Marrakech');
    DECLARE @idFes INT = (SELECT TOP 1 id FROM dbo.Ville WHERE id = 4 OR ville LIKE N'F%');
    DECLARE @idTanger INT = (SELECT TOP 1 id FROM dbo.Ville WHERE ville = N'Tanger');

    DECLARE @idIT INT = (SELECT id FROM dbo.SecteurActivite WHERE SecteurActivite = N'Information Technology');
    DECLARE @idFinance INT = (SELECT id FROM dbo.SecteurActivite WHERE SecteurActivite = N'Finance & Banking');
    DECLARE @idIndustry INT = (SELECT id FROM dbo.SecteurActivite WHERE SecteurActivite = N'Industry & Manufacturing');
    DECLARE @idRetail INT = (SELECT id FROM dbo.SecteurActivite WHERE SecteurActivite = N'Retail & Distribution');
    DECLARE @idHealth INT = (SELECT id FROM dbo.SecteurActivite WHERE id = 5);
    DECLARE @idEducation INT = (SELECT id FROM dbo.SecteurActivite WHERE id = 6);
    DECLARE @idConstruction INT = (SELECT id FROM dbo.SecteurActivite WHERE SecteurActivite = N'Construction & Engineering');
    DECLARE @idLogistics INT = (SELECT id FROM dbo.SecteurActivite WHERE SecteurActivite = N'Transport & Logistics');
    DECLARE @idMarketing INT = (SELECT id FROM dbo.SecteurActivite WHERE SecteurActivite = N'Marketing & Communications');
    DECLARE @idHR INT = (SELECT id FROM dbo.SecteurActivite WHERE SecteurActivite = N'Human Resources');

    /* ── Assign all English jobs to recruteur.demo (Demo Recrutement) ── */
    DECLARE @idRecruteurDemo INT = (
        SELECT TOP 1 rc._idRecruteur
        FROM dbo.Recruteur_Contact rc
        WHERE rc.compte = N'recruteur.demo'
    );

    IF @idRecruteurDemo IS NULL
        THROW 50001, N'recruteur.demo account not found — run jobzFactory_SampleData.sql first.', 1;

    /* ── 40 professional job listings ── */
    DECLARE @Jobs TABLE (
        RowNum INT IDENTITY(1,1) PRIMARY KEY,
        titre NVARCHAR(200),
        recruiter NVARCHAR(200),
        villeId INT,
        secteurId INT,
        urlSlug NVARCHAR(200),
        email NVARCHAR(100),
        message NVARCHAR(MAX)
    );

    INSERT INTO @Jobs (titre, recruiter, villeId, secteurId, urlSlug, email, message) VALUES
    (N'Senior .NET Developer', N'Atlas Digital Group', @idCasablanca, @idIT, N'Senior_NET_Developer', N'careers@atlasdigital.example',
     N'<p><strong>About the role</strong></p><p>Design and deliver enterprise web applications using C#, ASP.NET MVC, and SQL Server.</p><p><strong>Requirements</strong></p><ul><li>5+ years of .NET development</li><li>Strong knowledge of Entity Framework and REST APIs</li><li>Experience with Agile teams</li></ul><p><strong>Benefits</strong></p><ul><li>Hybrid work model</li><li>Health insurance</li><li>Annual training budget</li></ul>'),

    (N'Full Stack JavaScript Engineer', N'Nexus Technology Partners', @idRabat, @idIT, N'Full_Stack_JavaScript_Engineer', N'talent@nexustech.example',
     N'<p><strong>About the role</strong></p><p>Build modern SPAs with React, Node.js, and cloud-native services.</p><p><strong>Requirements</strong></p><ul><li>3+ years full stack experience</li><li>TypeScript and microservices exposure</li><li>Fluent English</li></ul><p><strong>Benefits</strong></p><ul><li>Remote-friendly policy</li><li>Performance bonus</li></ul>'),

    (N'DevOps Engineer', N'Nexus Technology Partners', @idCasablanca, @idIT, N'DevOps_Engineer', N'talent@nexustech.example',
     N'<p><strong>About the role</strong></p><p>Automate CI/CD pipelines and manage Azure/AWS infrastructure.</p><p><strong>Requirements</strong></p><ul><li>Docker, Kubernetes, Terraform</li><li>Monitoring and incident response</li><li>Scripting in PowerShell or Python</li></ul>'),

    (N'Cybersecurity Analyst', N'Atlas Digital Group', @idRabat, @idIT, N'Cybersecurity_Analyst', N'careers@atlasdigital.example',
     N'<p><strong>About the role</strong></p><p>Protect corporate assets through threat detection and security audits.</p><p><strong>Requirements</strong></p><ul><li>SOC operations experience</li><li>SIEM tools and vulnerability management</li><li>Security certifications preferred</li></ul>'),

    (N'Data Engineer', N'Atlas Digital Group', @idCasablanca, @idIT, N'Data_Engineer', N'careers@atlasdigital.example',
     N'<p><strong>About the role</strong></p><p>Design data pipelines and warehousing solutions for analytics teams.</p><p><strong>Requirements</strong></p><ul><li>SQL, Python, ETL tooling</li><li>Cloud data platforms</li><li>Data modeling best practices</li></ul>'),

    (N'Product Manager - SaaS', N'Nexus Technology Partners', @idMarrakech, @idIT, N'Product_Manager_SaaS', N'talent@nexustech.example',
     N'<p><strong>About the role</strong></p><p>Own product roadmap, user research, and cross-functional delivery.</p><p><strong>Requirements</strong></p><ul><li>B2B SaaS background</li><li>Stakeholder management</li><li>Metrics-driven decision making</li></ul>'),

    (N'UI/UX Designer', N'Pulse Marketing Agency', @idCasablanca, @idIT, N'UI_UX_Designer', N'jobs@pulsemarketing.example',
     N'<p><strong>About the role</strong></p><p>Create intuitive interfaces and design systems for web and mobile products.</p><p><strong>Requirements</strong></p><ul><li>Figma and prototyping tools</li><li>Portfolio of shipped products</li><li>User testing experience</li></ul>'),

    (N'QA Automation Engineer', N'Atlas Digital Group', @idFes, @idIT, N'QA_Automation_Engineer', N'careers@atlasdigital.example',
     N'<p><strong>About the role</strong></p><p>Build automated test suites and improve release quality.</p><p><strong>Requirements</strong></p><ul><li>Selenium or Playwright</li><li>API and regression testing</li><li>CI integration</li></ul>'),

    (N'Cloud Solutions Architect', N'Nexus Technology Partners', @idCasablanca, @idIT, N'Cloud_Solutions_Architect', N'talent@nexustech.example',
     N'<p><strong>About the role</strong></p><p>Lead cloud migration projects and enterprise architecture decisions.</p><p><strong>Requirements</strong></p><ul><li>Azure or AWS certifications</li><li>8+ years in IT architecture</li><li>Client-facing consulting skills</li></ul>'),

    (N'IT Support Specialist', N'Atlas Digital Group', @idTanger, @idIT, N'IT_Support_Specialist', N'careers@atlasdigital.example',
     N'<p><strong>About the role</strong></p><p>Provide L2 technical support for internal users and infrastructure.</p><p><strong>Requirements</strong></p><ul><li>Windows/Linux administration</li><li>Ticketing systems</li><li>Strong communication skills</li></ul>'),

    (N'Financial Analyst', N'Meridian Bank', @idCasablanca, @idFinance, N'Financial_Analyst', N'hr@meridianbank.example',
     N'<p><strong>About the role</strong></p><p>Prepare forecasts, variance analysis, and management reporting.</p><p><strong>Requirements</strong></p><ul><li>Finance or accounting degree</li><li>Advanced Excel and BI tools</li><li>2+ years in banking or corporate finance</li></ul>'),

    (N'Risk Management Officer', N'Meridian Bank', @idRabat, @idFinance, N'Risk_Management_Officer', N'hr@meridianbank.example',
     N'<p><strong>About the role</strong></p><p>Monitor credit, operational, and compliance risks across portfolios.</p><p><strong>Requirements</strong></p><ul><li>Risk frameworks knowledge</li><li>Regulatory reporting experience</li><li>Attention to detail</li></ul>'),

    (N'Corporate Accountant', N'Sterling Finance Partners', @idCasablanca, @idFinance, N'Corporate_Accountant', N'apply@sterlingfinance.example',
     N'<p><strong>About the role</strong></p><p>Manage month-end close, reconciliations, and statutory reporting.</p><p><strong>Requirements</strong></p><ul><li>IFRS familiarity</li><li>ERP systems experience</li><li>Professional accounting qualification preferred</li></ul>'),

    (N'Investment Associate', N'Sterling Finance Partners', @idRabat, @idFinance, N'Investment_Associate', N'apply@sterlingfinance.example',
     N'<p><strong>About the role</strong></p><p>Support M&A due diligence, valuation models, and client presentations.</p><p><strong>Requirements</strong></p><ul><li>Financial modeling skills</li><li>Strong analytical writing</li><li>Master''s in Finance or MBA</li></ul>'),

    (N'Branch Operations Manager', N'Meridian Bank', @idMarrakech, @idFinance, N'Branch_Operations_Manager', N'hr@meridianbank.example',
     N'<p><strong>About the role</strong></p><p>Oversee daily branch operations, sales targets, and customer experience.</p><p><strong>Requirements</strong></p><ul><li>Retail banking leadership</li><li>Team coaching experience</li><li>Compliance awareness</li></ul>'),

    (N'Supply Chain Manager', N'Horizon Logistics', @idCasablanca, @idLogistics, N'Supply_Chain_Manager', N'jobs@horizonlogistics.example',
     N'<p><strong>About the role</strong></p><p>Optimize procurement, warehousing, and distribution networks.</p><p><strong>Requirements</strong></p><ul><li>End-to-end supply chain expertise</li><li>WMS/TMS tools</li><li>Vendor negotiation skills</li></ul>'),

    (N'Fleet Operations Coordinator', N'Horizon Logistics', @idTanger, @idLogistics, N'Fleet_Operations_Coordinator', N'jobs@horizonlogistics.example',
     N'<p><strong>About the role</strong></p><p>Coordinate routes, drivers, and delivery performance KPIs.</p><p><strong>Requirements</strong></p><ul><li>Transport planning experience</li><li>GPS/telematics tools</li><li>Problem-solving under pressure</li></ul>'),

    (N'Warehouse Supervisor', N'Horizon Logistics', @idFes, @idLogistics, N'Warehouse_Supervisor', N'jobs@horizonlogistics.example',
     N'<p><strong>About the role</strong></p><p>Lead inbound/outbound operations and inventory accuracy.</p><p><strong>Requirements</strong></p><ul><li>Supervisory experience in logistics</li><li>Safety and quality standards</li><li>Shift management</li></ul>'),

    (N'Registered Nurse', N'Nova Health Systems', @idCasablanca, @idHealth, N'Registered_Nurse', N'talent@novahealth.example',
     N'<p><strong>About the role</strong></p><p>Deliver patient care in a multidisciplinary hospital environment.</p><p><strong>Requirements</strong></p><ul><li>Valid nursing license</li><li>Clinical assessment skills</li><li>Empathy and teamwork</li></ul>'),

    (N'Medical Laboratory Technician', N'Nova Health Systems', @idRabat, @idHealth, N'Medical_Laboratory_Technician', N'talent@novahealth.example',
     N'<p><strong>About the role</strong></p><p>Perform diagnostic tests and maintain lab equipment standards.</p><p><strong>Requirements</strong></p><ul><li>Lab certification</li><li>Quality control procedures</li><li>Attention to protocol</li></ul>'),

    (N'Healthcare Administrator', N'Nova Health Systems', @idMarrakech, @idHealth, N'Healthcare_Administrator', N'talent@novahealth.example',
     N'<p><strong>About the role</strong></p><p>Manage clinic scheduling, billing workflows, and patient records.</p><p><strong>Requirements</strong></p><ul><li>Healthcare administration background</li><li>EMR systems knowledge</li><li>Organizational skills</li></ul>'),

    (N'Management Consultant', N'Summit Consulting', @idCasablanca, @idIndustry, N'Management_Consultant', N'recruitment@summitconsulting.example',
     N'<p><strong>About the role</strong></p><p>Advise clients on operations improvement and digital transformation.</p><p><strong>Requirements</strong></p><ul><li>Consulting or Big Four experience</li><li>Structured problem solving</li><li>Excellent presentation skills</li></ul>'),

    (N'Production Engineer', N'Vertex Engineering', @idFes, @idIndustry, N'Production_Engineer', N'jobs@vertexengineering.example',
     N'<p><strong>About the role</strong></p><p>Improve manufacturing efficiency, quality, and maintenance planning.</p><p><strong>Requirements</strong></p><ul><li>Industrial engineering degree</li><li>Lean/Six Sigma exposure</li><li>CAD and process mapping</li></ul>'),

    (N'Quality Assurance Manager', N'Vertex Engineering', @idCasablanca, @idIndustry, N'Quality_Assurance_Manager', N'jobs@vertexengineering.example',
     N'<p><strong>About the role</strong></p><p>Lead QA programs, audits, and continuous improvement initiatives.</p><p><strong>Requirements</strong></p><ul><li>ISO standards knowledge</li><li>Team leadership</li><li>Root cause analysis</li></ul>'),

    (N'Store Manager', N'Pacific Retail Group', @idMarrakech, @idRetail, N'Store_Manager', N'careers@pacificretail.example',
     N'<p><strong>About the role</strong></p><p>Drive sales performance, staffing, and in-store customer experience.</p><p><strong>Requirements</strong></p><ul><li>Retail management experience</li><li>P&L accountability</li><li>Visual merchandising skills</li></ul>'),

    (N'E-commerce Operations Lead', N'Pacific Retail Group', @idCasablanca, @idRetail, N'Ecommerce_Operations_Lead', N'careers@pacificretail.example',
     N'<p><strong>About the role</strong></p><p>Manage online catalog, fulfillment SLAs, and marketplace integrations.</p><p><strong>Requirements</strong></p><ul><li>E-commerce platform experience</li><li>Digital analytics</li><li>Cross-functional coordination</li></ul>'),

    (N'Digital Marketing Manager', N'Pulse Marketing Agency', @idRabat, @idMarketing, N'Digital_Marketing_Manager', N'jobs@pulsemarketing.example',
     N'<p><strong>About the role</strong></p><p>Plan and execute multi-channel campaigns with measurable ROI.</p><p><strong>Requirements</strong></p><ul><li>SEO, SEM, and social media expertise</li><li>Marketing automation tools</li><li>Budget management</li></ul>'),

    (N'Content Strategist', N'Pulse Marketing Agency', @idCasablanca, @idMarketing, N'Content_Strategist', N'jobs@pulsemarketing.example',
     N'<p><strong>About the role</strong></p><p>Develop editorial calendars and brand storytelling across channels.</p><p><strong>Requirements</strong></p><ul><li>Excellent English writing</li><li>Content performance analysis</li><li>B2B or B2C portfolio</li></ul>'),

    (N'Social Media Specialist', N'Pulse Marketing Agency', @idTanger, @idMarketing, N'Social_Media_Specialist', N'jobs@pulsemarketing.example',
     N'<p><strong>About the role</strong></p><p>Grow audience engagement and manage community interactions.</p><p><strong>Requirements</strong></p><ul><li>Platform analytics</li><li>Creative campaign execution</li><li>Trend monitoring</li></ul>'),

    (N'HR Business Partner', N'Summit Consulting', @idCasablanca, @idHR, N'HR_Business_Partner', N'recruitment@summitconsulting.example',
     N'<p><strong>About the role</strong></p><p>Partner with leadership on talent strategy, performance, and retention.</p><p><strong>Requirements</strong></p><ul><li>HR generalist background</li><li>Employee relations experience</li><li>HRIS proficiency</li></ul>'),

    (N'Talent Acquisition Specialist', N'Summit Consulting', @idRabat, @idHR, N'Talent_Acquisition_Specialist', N'recruitment@summitconsulting.example',
     N'<p><strong>About the role</strong></p><p>Manage full-cycle recruitment for corporate and consulting roles.</p><p><strong>Requirements</strong></p><ul><li>Sourcing and interviewing skills</li><li>ATS tools experience</li><li>Stakeholder communication</li></ul>'),

    (N'Learning & Development Coordinator', N'BrightPath Education', @idCasablanca, @idEducation, N'Learning_Development_Coordinator', N'hr@brightpath.example',
     N'<p><strong>About the role</strong></p><p>Design training programs and coordinate instructor-led sessions.</p><p><strong>Requirements</strong></p><ul><li>Instructional design basics</li><li>LMS administration</li><li>Program coordination</li></ul>'),

    (N'Corporate English Trainer', N'BrightPath Education', @idRabat, @idEducation, N'Corporate_English_Trainer', N'hr@brightpath.example',
     N'<p><strong>About the role</strong></p><p>Deliver business English courses for corporate clients.</p><p><strong>Requirements</strong></p><ul><li>TEFL/CELTA or equivalent</li><li>Corporate training experience</li><li>Dynamic facilitation style</li></ul>'),

    (N'Project Manager - Construction', N'CoreBuild Construction', @idCasablanca, @idConstruction, N'Project_Manager_Construction', N'careers@corebuild.example',
     N'<p><strong>About the role</strong></p><p>Lead commercial construction projects from planning to handover.</p><p><strong>Requirements</strong></p><ul><li>PMP or equivalent</li><li>Contractor coordination</li><li>Budget and timeline control</li></ul>'),

    (N'Site Civil Engineer', N'CoreBuild Construction', @idMarrakech, @idConstruction, N'Site_Civil_Engineer', N'careers@corebuild.example',
     N'<p><strong>About the role</strong></p><p>Supervise structural works, safety compliance, and technical drawings.</p><p><strong>Requirements</strong></p><ul><li>Civil engineering degree</li><li>Site supervision experience</li><li>AutoCAD proficiency</li></ul>'),

    (N'Business Development Manager', N'Pacific Retail Group', @idCasablanca, @idRetail, N'Business_Development_Manager', N'careers@pacificretail.example',
     N'<p><strong>About the role</strong></p><p>Identify partnership opportunities and expand market presence.</p><p><strong>Requirements</strong></p><ul><li>B2B sales track record</li><li>Negotiation and contract skills</li><li>Market research capability</li></ul>'),

    (N'Customer Success Manager', N'Nexus Technology Partners', @idRabat, @idIT, N'Customer_Success_Manager', N'talent@nexustech.example',
     N'<p><strong>About the role</strong></p><p>Ensure client adoption, retention, and upsell opportunities.</p><p><strong>Requirements</strong></p><ul><li>SaaS customer success experience</li><li>Account management skills</li><li>Data-driven mindset</li></ul>'),

    (N'Procurement Officer', N'Horizon Logistics', @idCasablanca, @idLogistics, N'Procurement_Officer', N'jobs@horizonlogistics.example',
     N'<p><strong>About the role</strong></p><p>Manage vendor contracts, RFQs, and cost optimization programs.</p><p><strong>Requirements</strong></p><ul><li>Procurement processes knowledge</li><li>Supplier relationship management</li><li>Cost analysis</li></ul>'),

    (N'Internal Audit Specialist', N'Meridian Bank', @idCasablanca, @idFinance, N'Internal_Audit_Specialist', N'hr@meridianbank.example',
     N'<p><strong>About the role</strong></p><p>Execute audit plans and recommend control improvements.</p><p><strong>Requirements</strong></p><ul><li>Audit or compliance background</li><li>Risk assessment methods</li><li>Report writing skills</li></ul>'),

    (N'Executive Assistant', N'Sterling Finance Partners', @idRabat, @idHR, N'Executive_Assistant', N'apply@sterlingfinance.example',
     N'<p><strong>About the role</strong></p><p>Support senior executives with scheduling, travel, and board materials.</p><p><strong>Requirements</strong></p><ul><li>Executive support experience</li><li>Discretion and organization</li><li>Advanced Office suite skills</li></ul>');

    /* Skip if already seeded */
    IF NOT EXISTS (SELECT 1 FROM dbo.Offre WHERE titreOffre = N'Senior .NET Developer')
    BEGIN
        DECLARE @i INT = 1;
        DECLARE @max INT = (SELECT MAX(RowNum) FROM @Jobs);
        DECLARE @codId INT;
        DECLARE @offreNum INT;
        DECLARE @titre NVARCHAR(200);
        DECLARE @recruiter NVARCHAR(200);
        DECLARE @villeId INT;
        DECLARE @secteurId INT;
        DECLARE @urlSlug NVARCHAR(200);
        DECLARE @email NVARCHAR(100);
        DECLARE @message NVARCHAR(MAX);
        DECLARE @recruteurId INT;
        DECLARE @fullUrl NVARCHAR(400);

        SELECT @offreNum = ISNULL(MAX(offre_number), 0) FROM dbo.Codification WHERE annee = @annee;

        WHILE @i <= @max
        BEGIN
            SELECT
                @titre = titre,
                @recruiter = recruiter,
                @villeId = villeId,
                @secteurId = secteurId,
                @urlSlug = urlSlug,
                @email = email,
                @message = message
            FROM @Jobs WHERE RowNum = @i;

            SET @offreNum = @offreNum + 1;

            INSERT INTO dbo.Codification (annee, offre_number) VALUES (@annee, @offreNum);
            SET @codId = SCOPE_IDENTITY();

            SET @recruteurId = @idRecruteurDemo;
            SET @fullUrl = @urlSlug + N'_' + CAST(@offreNum AS NVARCHAR(10)) + N'-' + @annee;

            INSERT INTO dbo.Offre (message, _idRecruteur, _idVille, isPublie, titreOffre, _idSecteurActivite, url, isConfidentiel, adresseMailAPostuler, nomImage, numberOffre)
            VALUES (@message, @recruteurId, @villeId, 1, @titre, @secteurId, @fullUrl, 0, @email, NULL, @codId);

            SET @i = @i + 1;
        END

        PRINT N'40 professional English job listings inserted.';
    END
    ELSE
    BEGIN
        PRINT N'English job listings already present — skipped insert.';
    END

    COMMIT TRANSACTION;
    PRINT N'English jobs migration completed successfully.';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH
GO
