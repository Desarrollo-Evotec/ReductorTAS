using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.IO;
using Evotec.KRATA.ReductorTAS.Lib.SYS;
//using ICSharpCode.SharpZipLib.Zip;

namespace Evotec.KRATA.ReductorTAS.Lib.COR
{
    public class ZipHelper
    {
        private static Encoding GetActualEncoding()
        {
            return Encoding.GetEncoding("CP850");
        }
        public static void Unzip(string souceFilePath, string destPath)
        {
            ZipFile.ExtractToDirectory(souceFilePath, destPath, GetActualEncoding());
        }

        public static void Zip(string sourcePath, string destFilePath)
        {
            ZipFile.CreateFromDirectory(sourcePath, destFilePath, CompressionLevel.Optimal, false, GetActualEncoding());
        }

        public static string Resize(string sourceFilePath)
        {
            string DirPath = Path.GetDirectoryName(sourceFilePath);
            string FileName = Path.GetFileNameWithoutExtension(sourceFilePath);

            string tempDestPath = Path.Combine(DirPath, FileName);
            Unzip(sourceFilePath, tempDestPath);
            TasProcess.Resize(tempDestPath);

            string destinationFileName = System.IO.Path.Combine(Globals.GetTempPath, Guid.NewGuid() + System.IO.Path.GetExtension(sourceFilePath));
            /*
            if (File.Exists(destFilePath))
            {
                File.Delete(destFilePath);
            }
            */
            Zip(tempDestPath, destinationFileName);
            Directory.Delete(tempDestPath, true);
            return destinationFileName;
        }
    }
}
