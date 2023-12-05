using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.PCT9944
{
    public class PCT9944FullDiffInfo
    {
        public PCT9944FullDiffInfo()
        {
            XCorrelationIds = new();
            Inputs = new();
            Count = 0;
        }
        public List<string> XCorrelationIds { get; set; }
        public List<string> Inputs { get; set; }
        public int Count { get; set; }
    }
}
