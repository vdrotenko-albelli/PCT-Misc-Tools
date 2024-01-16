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
        const string MESSAGE_COL_NM_OLD = "Message";
        const string MESSAGE_COL_NM_NEW = "_source.Message";
        const string XCORRID_COL_NM_OLD = "X-CorrelationId";
        const string XCORRID_COL_NM_NEW = "_source.X-CorrelationId";


        public static List<DiscrepancyLogEntry> Read(string csvLogPath)
        {
            List<DiscrepancyLogEntry> rslt = new();
            var dt = Tools.Csv2DataTable(csvLogPath, ',', maxBufferSize: 102400);
            string MsgColNm = dt.Columns.Contains(MESSAGE_COL_NM_OLD) ? MESSAGE_COL_NM_OLD : MESSAGE_COL_NM_NEW;
            string XCorrIdColNm = dt.Columns.Contains(XCORRID_COL_NM_OLD) ? XCORRID_COL_NM_OLD : XCORRID_COL_NM_NEW;
            foreach (DataRow dr in dt.Rows)
            {
                string currMsg = dr[MsgColNm] as string;
                string currCorrId = dr[XCorrIdColNm] as string;
                var dle = DiscrepancyLogEntryParser.Parse(currMsg);
                dle.XCorrelationId = currCorrId;
                if (dt.Columns.Contains($"@{nameof(dle.timestamp_cw)}"))
                    dle.timestamp_cw = dr[$"@{nameof(dle.timestamp_cw)}"] as string;
                rslt.Add(dle);
            }
            return rslt;

        }

        public static List<NoDiscrepancyLogEntry> ReadNoDiscrepancies(string csvLogPath)
        {
            List<NoDiscrepancyLogEntry> rslt = new();
            var dt = Tools.Csv2DataTable(csvLogPath, ',', maxBufferSize: 102400);
            string MsgColNm = dt.Columns.Contains(MESSAGE_COL_NM_OLD) ? MESSAGE_COL_NM_OLD : MESSAGE_COL_NM_NEW;
            string XCorrIdColNm = dt.Columns.Contains(XCORRID_COL_NM_OLD) ? XCORRID_COL_NM_OLD : XCORRID_COL_NM_NEW;
            foreach (DataRow dr in dt.Rows)
            {
                string currMsg = dr[MsgColNm] as string;
                string currCorrId = dr[XCorrIdColNm] as string;
                var dle = NoDiscrepancyLogEntryParser.Parse(currMsg);
                dle.XCorrelationId = currCorrId;
                if (dt.Columns.Contains($"@{nameof(dle.timestamp_cw)}"))
                    dle.timestamp_cw = dr[$"@{nameof(dle.timestamp_cw)}"] as string;
                rslt.Add(dle);
            }
            return rslt;

        }
    }
}
