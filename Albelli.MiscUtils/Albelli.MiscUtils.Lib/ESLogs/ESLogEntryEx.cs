using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.ESLogs
{
    public class ESLogEntryEx
    {
        public string Message { get; set; }
        public string XCorrelationId { get; set; }
        public string timestamp_cw { get; set; }
        public string Level { get; set; }
        public string JSContent { get; set; }
    }
}
