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
    public class DirHelper
    {
        private static FileSystemWatcher _watcher;

        /// <summary>
        /// Inicia el watcher sobre el directorio de entradas
        /// </summary>
        public static void StartWatcher()
        {
            string DirPath = Globals.GetSourceDirectory;
            if (_watcher == null && Directory.Exists(DirPath))
            {
                _watcher = new FileSystemWatcher();
                _watcher.Path = DirPath;
                _watcher.NotifyFilter = NotifyFilters.FileName;
                _watcher.Filter = "*.tas";
                _watcher.Created += new FileSystemEventHandler(Watcher_NewFile);
                //_watcher.Changed += new FileSystemEventHandler(Watcher_NewFile);
                _watcher.EnableRaisingEvents = true;
            }
        }

        /// <summary>
        /// Para el watcher
        /// </summary>
        public static void StopWatcher()
        {
            if (_watcher != null)
            {
                _watcher.Dispose();
                _watcher = null;
            }
        }

        /// <summary>
        /// Llamado cada vez que se detecta un nuevo fichero
        /// </summary>
        private static void Watcher_NewFile(object sender, FileSystemEventArgs e)
        {
            string TASFullPath = string.Empty;
            try
            {
                TASFullPath = e.FullPath;
                FileInfo _info = new FileInfo(TASFullPath);
                if (_info == null)
                {
                    return;
                }
                int retryCount = 0;
                int MaxCount = 10;
                while (IsFileLocked(_info, retryCount == MaxCount) && retryCount <= MaxCount)
                {
                    Globals.HardSleep(2);
                }
                Globals.SoftSleep();

                string tasDestFileName = Path.Combine(Globals.GetDestinationDirectory, Path.GetFileNameWithoutExtension(TASFullPath) + Const.FinalTasFileExtension);

                try
                {
                    TasProcess.ProcessIndividualTas(TASFullPath, tasDestFileName);
                }
                catch (PathTooLongException)
                {
                    Log.LogServer.WriteLog("Fichero con ByPass por ruta demasiado larga: " + Path.GetFileName(tasDestFileName));
                    File.Copy(TASFullPath, tasDestFileName);
                }
                catch (ByPassException ex)
                {
                    Log.LogServer.WriteLog("Fichero con ByPass por error: " + Path.GetFileName(tasDestFileName));
                    Log.LogServer.WriteLog(ex);
                    File.Copy(TASFullPath, tasDestFileName);
                }
                TasProcess.ArchiveOrDeleteOriginalFile(TASFullPath);
            }
            catch(Exception ex)
            {
                Log.LogServer.WriteLog(string.Format("Error procesando la tasación {0}. Más información: ", TASFullPath));
                Log.LogServer.WriteLog(ex);
            }
        }

        /// <summary>
        /// Nos dice si el fichero está bloqueado (o aún se está escribiendo)
        /// </summary>
        private static bool IsFileLocked(FileInfo file, bool logError = false)
        {
            FileStream stream = null;
            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (Exception ex)
            {
                if (logError)
                {
                    Log.LogServer.WriteLog(ex);
                }
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            //file is not locked
            return false;
        }

    }
}
