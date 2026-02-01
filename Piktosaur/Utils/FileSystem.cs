using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piktosaur.Utils
{
    public class FileSystem
    {
        public static string GetPicturesFolder()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        }

        public static string GetDownloadsFolder()
        {
            var userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(userFolder, "Downloads");
        }

        public static string GetFormattedFolderName(string path)
        {
            string userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            string relativePath = Path.GetRelativePath(userProfilePath, path);

            if (relativePath.StartsWith("..")) return path;

            if (relativePath == ".") return Environment.UserName;

            return relativePath;
        }
    }
}
