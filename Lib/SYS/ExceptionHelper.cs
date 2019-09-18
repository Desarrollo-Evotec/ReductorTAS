using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evotec.KRATA.ReductorTAS.Lib.SYS
{
    class ByPassException: Exception
    {
        public ByPassException(string msg, Exception innerEx): base(msg, innerEx)
        {

        }
    }
}
