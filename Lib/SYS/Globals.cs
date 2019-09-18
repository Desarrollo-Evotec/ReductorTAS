using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Evotec.KRATA.ReductorTAS.Lib.SYS
{
    public class Globals
    {
        public static bool ThreadOK;

        /// <summary>
        /// Reduce una imagen en caso de ser mayor que el tamaño constante definido
        /// </summary>
        public static Image ResizeImage(Image img)
        {
            Image returnImage = img;
            float xDpi = img.HorizontalResolution;
            float yDpi = img.VerticalResolution;
            PixelFormat pFormat = img.PixelFormat;
            if (pFormat.ToString().ToLower().Contains("index"))
            {
                pFormat = PixelFormat.Format24bppRgb;
            }

            int newImageMaxWidth = Const.maxImageWidth;
            int newImageMaxHeight = Const.maxImageHeight;

            //newImageMaxWidth = img.Width < newImageMaxWidth ? img.Width : newImageMaxWidth;
            //newImageMaxHeight = img.Height < newImageMaxHeight ? img.Height : newImageMaxHeight;

            if (img.Width > newImageMaxWidth || img.Height > newImageMaxHeight)
            {
                var ratioX = (double)newImageMaxWidth / img.Width;
                var ratioY = (double)newImageMaxHeight / img.Height;
                var ratio = Math.Min(ratioX, ratioY);

                var newWidth = (int)(img.Width * ratio);
                var newHeight = (int)(img.Height * ratio);

                var newImage = new Bitmap(newWidth, newHeight, pFormat);


                using (var graphics = Graphics.FromImage(newImage))
                {
                    graphics.DrawImage(img, 0, 0, newWidth, newHeight);
                }

                if (xDpi > Const.maxDpi)
                {
                    xDpi = Const.maxDpi;
                }
                if (yDpi > Const.maxDpi)
                {
                    yDpi = Const.maxDpi;
                }

                newImage.SetResolution(xDpi, yDpi);
                returnImage = newImage;
            }

            return returnImage;

        }

        public static string GetExecutePath()
        {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        public static string GetTempPath
        {
            get
            {
                string TempPath = Path.Combine(GetExecutePath(), Const.TempDirName);
                if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.TempDir))
                {
                    TempPath = Properties.Settings.Default.TempDir;
                }
                if (!Directory.Exists(TempPath))
                {
                    Directory.CreateDirectory(TempPath);
                }
                return TempPath;
            }
        }

        public static string GetSourceDirectory
        {
            get
            {
                return Properties.Settings.Default.SourceDir;
            }
        }

        public static string GetDestinationDirectory
        {
            get
            {
                return Properties.Settings.Default.DestinationDir;
            }
        }

        public static string GetArchiveDirectory
        {
            get
            {
                return Properties.Settings.Default.ArchiveDir;
            }
        }

        public static string GetOversizeDirectory
        {
            get
            {
                return Properties.Settings.Default.OversizeDir;
            }
        }

        public static bool CheckForOversize
        {
            get
            {
                if (MaxMBSize > 0)
                {
                    return true;
                }
                return false;
            }
        }
        public static int MaxMBSize
        {
            get
            {
                return Properties.Settings.Default.MaxMBSize;
            }
        }

        /// <summary>
        /// Obtiene la ruta donde guardar un fichero asegurandose de que en caso de que exista lo guarda añadiendo un sufijo
        /// </summary>
        public static string EnsureFileNameInPath(string FilePath)
        {
            if (File.Exists(FilePath))
            {
                string DirPath = Path.GetDirectoryName(FilePath);
                string FileName = Path.GetFileNameWithoutExtension(FilePath);
                string Extension = Path.GetExtension(FilePath);

                FilePath = Path.Combine(DirPath, FileName + DateTime.Now.ToString("_yyyyMMdd_HHmmss") + Extension);
            }
            return FilePath;
        }

        public static void DeleteFileIfExist(string FilePath)
        {
            if (File.Exists(FilePath))
            {
                try
                {
                    File.Delete(FilePath);
                }
                catch(Exception ex)
                {
                    Log.LogServer.WriteLog(string.Format("Se ha producido una excepción al borrar el fichero '{0}'. Más información:", FilePath));
                    Log.LogServer.WriteLog(ex);
                    throw ex;
                }
                
            }
        }
        public static void DeleteDirectoryIfExist(string DirPath)
        {
            if (Directory.Exists(DirPath))
            {
                try
                {
                    Directory.Delete(DirPath, true);
                }
                catch(Exception ex)
                {
                    Log.LogServer.WriteLog(string.Format("Se ha producido una excepción al borrar el directorio '{0}'. Más información:", DirPath));
                    Log.LogServer.WriteLog(ex);
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Duerme profundamente durante X segundos
        /// </summary>
        /// <param name="Seconds"></param>
        public static void HardSleep(int Seconds)
        {
            DateTime LimitDttm = DateTime.Now.AddSeconds(Seconds);
            while (ThreadOK && DateTime.Now < LimitDttm)
            {
                SoftSleep();
            }
        }
        /// <summary>
        /// Duerme un tiempo estandar mínimo de milisegundos
        /// </summary>
        public static void SoftSleep()
        {
            Thread.Sleep(1000);
        }

        public static void CheckSmaller(string tempFilePath, string originalFilePath, string destinationfilePath)
        {
            FileInfo i_temp = new FileInfo(tempFilePath);
            FileInfo i_ori = new FileInfo(originalFilePath);



            if (i_temp.Length < i_ori.Length)
            {
                //El nuevo fichero es más pequeño
                File.Copy(tempFilePath, destinationfilePath, true);
                Stats.FicherosReducidos++;
                Stats.EspacioGanadoBytes += (i_ori.Length - i_temp.Length);
            }
            else
            {
                //El original es más pequeño
                if (originalFilePath == destinationfilePath)
                {
                    //No hacer nada
                }
                else
                {
                    File.Copy(originalFilePath, destinationfilePath, true);
                }
            }
            File.Delete(tempFilePath);
        }
    }
}
