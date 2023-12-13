using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.PCT9944
{
    public class NoDiscrepancyLogEntry
    {
        public string Input { get; set; }
        private CalculateCarrierModel _parsedInput = null;
        public CalculateCarrierModel ParsedInput
        {
            get
            {
                if (_parsedInput == null)
                {
                    _parsedInput = (new CalculateCarrierModelParser()).Parse(Input);
                }
                return _parsedInput;
            }
        }
        public string Centiro { get; set; }
        public string XCorrelationId { get; set; }
        public string timestamp_cw { get; set; }
    }
}
