using Evotec.KRATA.ReductorTAS.Lib.SYS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Evotec.KRATA.ReductorTAS.Lib.COR
{
    public class TasProcess
    {
        /// <summary>
        /// Comprueba que todos los directorios se encuentren y sean accesibles
        /// </summary>
        public static string CheckDirectories()
        {
            StringBuilder sb = new StringBuilder();
            if (!Directory.Exists(Globals.GetSourceDirectory))
            {
                sb.AppendLine("El directorio de entrada no existe");
            }

            if (!string.IsNullOrWhiteSpace(Globals.GetDestinationDirectory) && !Directory.Exists(Globals.GetDestinationDirectory))
            {
                sb.AppendLine("El directorio de salida no existe");
            }
            if (!string.IsNullOrWhiteSpace(Globals.GetArchiveDirectory) && !Directory.Exists(Globals.GetArchiveDirectory))
            {
                sb.AppendLine("El directorio de archivo no existe");
            }
            if (!string.IsNullOrWhiteSpace(Globals.GetOversizeDirectory) && !Directory.Exists(Globals.GetOversizeDirectory))
            {
                sb.AppendLine("El directorio para tasaciones sobredimensionadas no existe");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Procesa un directorio donde buscará los ficheros .tas
        /// </summary>
        public static void ProcessSourceDir()
        {
            string sourcePath = Globals.GetSourceDirectory;

            int ProcesadosOK = 0;
            int ProcesadosKO = 0;

            List<string> FicherosTAS = Directory.GetFiles(sourcePath, "*.tas").ToList();
            if (FicherosTAS.Count == 0)
            {
                Log.ConsoleDebugWriteLine("No se han encontrado ficheros .tas en el directorio");
                return;
            }
            //Para no pillar un fichero siendo escrito
            Globals.HardSleep(10);
            foreach (string tasFileName in FicherosTAS)
            {
                try
                {
                    string tasDestFileName = Path.Combine(Globals.GetDestinationDirectory, Path.GetFileNameWithoutExtension(tasFileName) + Const.FinalTasFileExtension);
                    if (Config.ByPass)
                    {
                        File.Copy(tasFileName, tasDestFileName);
                    }
                    else
                    {
                        ProcessIndividualTas(tasFileName, tasDestFileName);
                    }
                    ArchiveOrDeleteOriginalFile(tasFileName);
                    ProcesadosOK++;
                }
                catch(Exception ex)
                {
                    Log.LogConsoleDebugWriteLine("Se ha producido un error procesando el fichero " + tasFileName);
                    Log.LogServer.WriteLog(ex);
                    ProcesadosKO++;
                }
            }

            Log.ConsoleDebugWriteLine("Ficheros procesados correctamente: " + ProcesadosOK);
            Log.ConsoleDebugWriteLine("Ficheros procesados con error: " + ProcesadosKO);

            #region Limpiar temporal
            //Limpiamos el temporal
            try
            {
                Directory.Delete(Globals.GetTempPath, true);
            }
            catch(Exception ex)
            {
                Log.LogServer.WriteLog("No se ha podido borrar el directorio temporal. Más información:");
                Log.LogServer.WriteLog(ex);
            }
            #endregion

            CheckArchiveTimeout();
        }

        /// <summary>
        /// Punto de entrada con gestión de excepciones
        /// </summary>
        public static void ProcessIndividualTas(string sourceFileName, string destFileName)
        {
            try
            {
                _ProcessIndividualTas(sourceFileName, destFileName);
            }
            catch (PathTooLongException)
            {
                Log.LogServer.WriteLog("Fichero con ByPass por ruta demasiado larga: " + Path.GetFileName(destFileName) + ". Más información:");
                File.Copy(sourceFileName, destFileName);
            }
            catch (ByPassException ex)
            {
                Log.LogServer.WriteLog("Fichero con ByPass por error: " + Path.GetFileName(destFileName) + ". Más información:");
                Log.LogServer.WriteLog(ex);
                File.Copy(sourceFileName, destFileName);
            }
        }

        /// <summary>
        /// Procesa un fichero .tas individual
        /// </summary>
        private  static void _ProcessIndividualTas(string sourceFileName, string destFileName)
        {
            string tempPath = Path.Combine(Globals.GetTempPath, Path.GetFileNameWithoutExtension(sourceFileName));

            Globals.DeleteDirectoryIfExist(tempPath);

            try
            {
                ZipHelper.Unzip(sourceFileName, tempPath);
            }
            catch (Exception ex)
            {
                throw new ByPassException("Fichero con ByPass por error al descomprimir", ex);
            }
            #region Comprimir ficheros

            Resize(tempPath);

            #endregion
            
            string tempZipFilePath = Path.Combine(Globals.GetTempPath, Path.GetFileName(destFileName));
            Globals.DeleteFileIfExist(tempZipFilePath);
            ZipHelper.Zip(tempPath, tempZipFilePath);

            
            string realFinalDestFilePath = CheckOversizeTas(tempZipFilePath, destFileName);
            
            Globals.DeleteFileIfExist(realFinalDestFilePath);
            File.Move(tempZipFilePath, realFinalDestFilePath);

            try
            {
                Globals.DeleteDirectoryIfExist(tempPath);
            }
            catch(Exception ex)
            {
                //No nos afecta para el correcto funcionamiento
            }
        }

        /// <summary>
        /// Archiva el fichero original
        /// </summary>
        public static void ArchiveOrDeleteOriginalFile(string sourceFilePath)
        {
            string sourceFileName = Path.GetFileName(sourceFilePath);

            string archiveFilePath = Path.Combine(Globals.GetArchiveDirectory, sourceFileName);
            archiveFilePath = Globals.EnsureFileNameInPath(archiveFilePath);

            bool canDelete = false;
            if (Properties.Settings.Default.DaysToKeepOriginal <= 0)
            {
                canDelete = true;
            }
            else
            {
                try
                {
                    File.Copy(sourceFilePath, archiveFilePath);
                    canDelete = true;
                }
                catch (Exception ex)
                {
                    Log.ConsoleDebugWriteLine("No se ha podido archivar el fichero " + sourceFileName);
                    Log.LogServer.WriteLog(ex);
                }
            }

            if (canDelete)
            {
                try
                {
                    File.Delete(sourceFilePath);
                }
                catch (Exception ex)
                {
                    Log.ConsoleDebugWriteLine("No se ha podido borrar el fichero " + sourceFileName);
                    Log.LogServer.WriteLog(ex);
                }
            }
        }

        public static string CheckOversizeTas(string tempFilePath, string NormalDestFilePath)
        {
            string realFinalFilePath = NormalDestFilePath;
            if (Globals.CheckForOversize)
            {
                FileInfo f = new FileInfo(tempFilePath);
                int FileMB = (int)Math.Ceiling(((decimal)f.Length / 1024) / 1024);
                if (Globals.MaxMBSize < FileMB)
                {
                    realFinalFilePath = Path.Combine(Globals.GetOversizeDirectory, Path.GetFileName(NormalDestFilePath));
                    Email.SendOverSizedEmail(Path.GetFileName(tempFilePath), FileMB);
                }
            }
            return realFinalFilePath;
        }

        /// <summary>
        /// Comprobar y borros ficheros que se hayan pasado el margen de tiempo para archivar (no genera excepción en caso de fallo)
        /// </summary>
        public static void CheckArchiveTimeout_NoException()
        {
            try
            {
                CheckArchiveTimeout();
            }
            catch(Exception ex)
            {
                Log.LogServer.WriteLog("Excepción al limpiar el archivo. Más información: ");
                Log.LogServer.WriteLog(ex);
            }
        }

        /// <summary>
        /// Comprobar y borros ficheros que se hayan pasado el margen de tiempo para archivar
        /// </summary>
        private static void CheckArchiveTimeout()
        {
            int _OldDeleteFiles = 0;
            DirectoryInfo Dir = new DirectoryInfo(Globals.GetArchiveDirectory);

            //Hemos detectado que a veces la fecha de creación es posterior a la de modificación, no me fio
            var TimeoutFiles = Dir.GetFiles().Where(x => 
                x.CreationTime.AddDays(Properties.Settings.Default.DaysToKeepOriginal) <= DateTime.Now
                && x.LastWriteTime.AddDays(Properties.Settings.Default.DaysToKeepOriginal) <= DateTime.Now
                && x.LastAccessTime.AddDays(Properties.Settings.Default.DaysToKeepOriginal) <= DateTime.Now
                );

            foreach (FileInfo _file in TimeoutFiles)
            {
                try
                {
                    _file.Delete();
                    _OldDeleteFiles++;
                }
                catch(Exception ex)
                {
                    Log.LogServer.WriteLog(string.Format("No se ha podido borrar el fichero {0}. Más información:", _file.FullName));
                    Log.LogServer.WriteLog(ex);
                }
            }
            if (_OldDeleteFiles > 0)
            {
                Log.ConsoleDebugWriteLine(string.Format("Se han limpiado {0} ficheros antiguos", _OldDeleteFiles));
            }
        }

        /// <summary>
        /// Comprime los ficheros de un directorio en particular
        /// </summary>
        public static void Resize(string sourcePath, string destinationPath = null)
        {
            if (string.IsNullOrWhiteSpace(destinationPath))
            {
                destinationPath = sourcePath;
            }
            var Ficheros = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
            int TotalCount = Ficheros.Count();
            int Index = 0;
            DateTime FlushDttm = DateTime.MinValue;
            int FlushCount = 0;
            foreach (string file in Ficheros)
            {
                Index++;
                
                
                //System.Diagnostics.Debug.WriteLine(string.Format("Procesando ficheros en tasación: {0}/{1}", Index, TotalCount));
                //string destinationFile = Path.Combine(destinationPath, Path.GetFileName(file));

                //Para no saturar la consola... La está saturando el itextsharp
                if (FlushDttm < DateTime.Now)
                {
                    FlushCount++;
                    FlushDttm = DateTime.Now.AddSeconds(10);
                    Log.LogConsoleDebugWriteLine(string.Format("Procesando ficheros: {0}/{1}. {2}", Index, TotalCount, file));
                    if (FlushCount > (6 * 5))//Cada 5 minutos
                    {
                        Log.LogConsoleDebugWriteLine(string.Format("Hasta el momento se han reducido {0} ficheros ahorrando {1} MB", Stats.FicherosReducidos, Stats.GetEspacioGanadoMB()));
                        FlushCount = 0;
                    }
                }
                

                string destinationFile = Path.Combine(destinationPath, file.Replace(sourcePath, string.Empty).TrimStart('\\'));

                try
                {
                    string extension = Path.GetExtension(file).ToLower();
                    switch (extension)
                    {
                        case ".pdf":
                            {
                                Globals.CheckSmaller(PdfHelper.Resize(file), file, destinationFile);
                                break;
                            }
                        case ".jpg":
                        case ".jpeg":
                        case ".png":
                        case ".tiff":
                            {
                                Globals.CheckSmaller(ImageHelper.Resize(file), file, destinationFile);
                                break;
                            }
                        case ".zip":
                            {
                                Globals.CheckSmaller(ZipHelper.Resize(file), file, destinationFile);
                                break;
                            }
                        default:
                            {
                                if (sourcePath != destinationPath)
                                {
                                    File.Copy(file, destinationFile, true);
                                }
                                break;
                            }
                    }
                }
                catch(Exception ex)
                {
                    if (sourcePath != destinationPath)
                    {
                        File.Copy(file, destinationFile, true);
                    }
                }
            }
        }

        /// <summary>
        /// Procesa un directorio particular
        /// </summary>
        public static void ProcessSimpleDir()
        {
            string DestinationDirectory = Globals.GetDestinationDirectory;
            if (string.IsNullOrWhiteSpace(DestinationDirectory))
            {
                DestinationDirectory = null;
            }
            Resize(Globals.GetSourceDirectory, DestinationDirectory);
            Log.LogConsoleDebugWriteLine(string.Format("Se ha terminado el proceso. Se han reducido {0} ficheros ahorrando {1} MB", Stats.FicherosReducidos, Stats.GetEspacioGanadoMB()));
        }
    }
}
