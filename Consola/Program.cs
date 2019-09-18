using Evotec.KRATA.ReductorTAS.Lib.COR;
using Evotec.KRATA.ReductorTAS.Lib.SYS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Consola
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Count() == 0)
            {
                Log.ConsoleDebugWriteLine("Hace falta un parametro");
                Opciones();
                IntroducirOpcion();
            }
            else if (args.Count() >= 2)
            {
                Log.ConsoleDebugWriteLine("No se permite mas de un parametro");
                Opciones();
                IntroducirOpcion();
            }
            else
            {
                ComprobarOpcion(args.ElementAt(0));
            }


            Log.LogConsoleDebugWriteLine("Proceso finalizado, pulsa una tecla para cerrar.");
            Console.ReadKey();
        }

        static void Opciones()
        {
            Console.WriteLine("\nParametros:");
            Console.WriteLine("-t (ficheros comprimidos .tas)");
            Console.WriteLine("-s (ficheros simples)");
        }

        static void ComprobarDirectorios()
        {
            string err = TasProcess.CheckDirectories();
            if (!string.IsNullOrWhiteSpace(err))
            {
                Log.ConsoleDebugWriteLine(err);
                throw new Exception(err);
            }
        }

        static void IntroducirOpcion()
        {
            Console.WriteLine("\nIntroduzca una opcion: ");
            ComprobarOpcion(Console.ReadLine());
        }

        static void ComprobarOpcion(string opcion)
        {
            switch (opcion)
            {
                case "-t":
                    {
                        ComprobarDirectorios();
                        Log.LogConsoleDebugWriteLine("Se inicia el proceso -t");
                        TasProcess.ProcessSourceDir();
                        break;
                    }
                case "-s":
                    {
                        ComprobarDirectorios();
                        Log.LogConsoleDebugWriteLine("Se inicia el proceso -s");
                        TasProcess.ProcessSimpleDir();
                        break;
                    }
                default:
                    {
                        Console.WriteLine("No se reconoce el parametro");
                        Opciones();
                        break;
                    }
            }
        }
    }
}
