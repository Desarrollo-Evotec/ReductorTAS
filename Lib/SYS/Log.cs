using Evotec.KRATA.ReductorTAS.Lib.Properties;
using Evotec.Utils.LogLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evotec.KRATA.ReductorTAS.Lib.SYS
{
    public class Log
    {
        private static LogServer _LogServer;
        public static LogServer LogServer
        {
            get
            {
                if (_LogServer == null)
                {
                    _LogServer = new LogServer(Settings.Default.LogName, Settings.Default.LogPath, Settings.Default.LogDaysToKeep, Settings.Default.LogTimeUTC);
                }
                return _LogServer;
            }
        }

        public static void ConsoleDebugWriteLine(string msg)
        {
            //Log.LogServer.WriteLog(msg);
            System.Diagnostics.Debug.WriteLine(msg);
            System.Console.WriteLine(msg);
        }

        public static void LogWriteLine(string msg)
        {
            Log.LogServer.WriteLog(msg);
        }

        public static void LogConsoleDebugWriteLine(string msg)
        {
            ConsoleDebugWriteLine(msg);
            LogWriteLine(msg);
        }

    }
}
