using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evotec.KRATA.ReductorTAS.Lib.SYS
{
    public static class Stats
    {
        public static int FicherosReducidos = 0;
        public static long EspacioGanadoBytes = 0;
        public static decimal GetEspacioGanadoMB()
        {
            return (((decimal)EspacioGanadoBytes / 1024) / 1024);
        }
    }
}
