using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.PCT9944
{
    public static class PCT9944DiscrepanciesLogReader
    {
        public static List<DiscrepancyLogEntry> Read(string csvLogPath)
        {
            List<DiscrepancyLogEntry> rslt = new();
            var dt = Tools.Csv2DataTable(csvLogPath, ',', maxBufferSize: 102400);
            foreach (DataRow dr in dt.Rows)
            {
                string currMsg = dr["Message"] as string;
                string currCorrId = dr["X-CorrelationId"] as string;
                var dle = DiscrepancyLogEntryParser.Parse(currMsg);
                dle.XCorrelationId = currCorrId;
                if (dt.Columns.Contains($"@{nameof(dle.timestamp_cw)}"))
                    dle.timestamp_cw = dr[$"@{nameof(dle.timestamp_cw)}"] as string;
                rslt.Add(dle);
            }
            return rslt;

        }
    }
}
