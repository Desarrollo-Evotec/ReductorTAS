using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evotec.KRATA.ReductorTAS.Lib.SYS
{
    public static class Config
    {
        public static bool ByPass { get { return ReadSettings<bool>("ByPass"); } }
        public static string Destinatarios { get { return ReadSettings("Destinatarios"); } }
        public static string DestinatariosCopia { get { return ReadSettings("DestinatariosCopia"); } }
        public static string DestinatariosCopiaOculta { get { return ReadSettings("DestinatariosCopiaOculta"); } }

        #region Métodos Privados
        /// <summary>
        /// Clase generica de lectura
        /// </summary>
        private static T ReadSettings<T>(string SettingsName)
        {
            T returnValue = default(T);
            try
            {
                string stringValue = System.Configuration.ConfigurationManager.AppSettings[SettingsName];
                returnValue = (T)Convert.ChangeType(stringValue, typeof(T));
            }
            catch (Exception) { }
            return returnValue;
        }

        /// <summary>
        /// Clase de lectura de tipo string por defecto
        private static string ReadSettings(string SettingsName)
        {
            return ReadSettings<string>(SettingsName);
        }

        private static string EnsureDir(string DirPath)
        {
            if (!string.IsNullOrWhiteSpace(DirPath) && !Directory.Exists(DirPath))
            {
                Directory.CreateDirectory(DirPath);
            }
            return DirPath;
        }
        #endregion

    }
}
