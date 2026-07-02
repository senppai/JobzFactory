/*
Post-Deployment Script Template
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.
 Use SQLCMD syntax to include a file in the post-deployment script.
 Example:      :r .\myfile.sql
--------------------------------------------------------------------------------------
*/

-- Reference / lookup data (countries, cities, sectors, action types) — always loaded.
:r ..\..\jobzFactory_SeedData.sql

-- Demo / sample data (test recruiter accounts and sample offers) is NOT loaded here.
-- Production deployments must start clean. For a dev/staging dataset, run
-- ..\jobzFactory_SampleData.sql manually (see Database\README.md), or uncomment the
-- line below for local DACPAC publishes only.
-- :r ..\..\jobzFactory_SampleData.sql
