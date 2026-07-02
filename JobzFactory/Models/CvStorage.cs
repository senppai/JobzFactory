using System;
using System.IO;
using System.Web;

namespace JobzFactory.Models
{
    public static class CvStorage
    {
        public static string GetApplicationFolder(HttpServerUtilityBase server, int applicationId, bool create = false)
        {
            var path = server.MapPath("~/App_Data/CV/OffrePostule/" + applicationId);
            if (create && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        public static string GetLegacyFolder(HttpServerUtilityBase server, int applicationId)
        {
            return server.MapPath("~/Ressource/CV/OffrePostule/" + applicationId);
        }

        // Folder where a candidate's saved CV library lives: App_Data/CV/Profil/{profilId}.
        public static string GetProfilCvFolder(HttpServerUtilityBase server, int profilId, bool create = false)
        {
            var path = server.MapPath("~/App_Data/CV/Profil/" + profilId);
            if (create && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public static string GetProfilCvPath(HttpServerUtilityBase server, int profilId, string storedFileName)
        {
            var safeName = Path.GetFileName(storedFileName);
            if (string.IsNullOrEmpty(safeName)) return null;

            var folder = GetProfilCvFolder(server, profilId);
            var full = Path.Combine(folder, safeName);
            if (!IsInsideDirectory(full, folder) || !File.Exists(full)) return null;
            return full;
        }

        public static string ResolveApplicationCvPath(HttpServerUtilityBase server, int applicationId, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            // Defense against path traversal: keep only the leaf file name and
            // confirm the resolved full path stays inside the application folder.
            var safeName = Path.GetFileName(fileName);
            if (string.IsNullOrEmpty(safeName) || safeName != fileName.Trim())
            {
                return null;
            }

            var appFolder = GetApplicationFolder(server, applicationId);
            var appDataPath = Path.Combine(appFolder, safeName);
            if (!IsInsideDirectory(appDataPath, appFolder) || !File.Exists(appDataPath))
            {
                appDataPath = null;
            }

            if (appDataPath != null)
            {
                return appDataPath;
            }

            var legacyFolder = GetLegacyFolder(server, applicationId);
            var legacyPath = Path.Combine(legacyFolder, safeName);
            if (!IsInsideDirectory(legacyPath, legacyFolder) || !File.Exists(legacyPath))
            {
                return null;
            }

            Directory.CreateDirectory(GetApplicationFolder(server, applicationId, create: true));
            File.Copy(legacyPath, Path.Combine(appFolder, safeName), overwrite: true);
            return Path.Combine(appFolder, safeName);
        }

        private static bool IsInsideDirectory(string fullPath, string directoryPath)
        {
            string normalizedFull = Path.GetFullPath(fullPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string normalizedDir = Path.GetFullPath(directoryPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return normalizedFull.StartsWith(normalizedDir + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        }
    }
}
