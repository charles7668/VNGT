using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;

namespace Helper
{
    public static class FileHelper
    {
        /// <summary>
        /// copy all file in source directory to destination directory
        /// </summary>
        /// <param name="sourceDir"></param>
        /// <param name="destinationDir"></param>
        public static void CopyDirectory(string sourceDir, string destinationDir)
        {
            Directory.CreateDirectory(destinationDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destinationDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (string directory in Directory.GetDirectories(sourceDir))
            {
                string destDir = Path.Combine(destinationDir, Path.GetFileName(directory));
                CopyDirectory(directory, destDir);
            }
        }
    }
}