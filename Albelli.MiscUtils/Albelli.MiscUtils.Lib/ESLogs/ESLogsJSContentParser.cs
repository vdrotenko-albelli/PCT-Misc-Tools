using Albelli.MiscUtils.Lib.PCT9944;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.ESLogs
{
    public static class ESLogsJSContentParser
    {
        const string MESSAGE_COL_NM_OLD = "Message";
        const string MESSAGE_COL_NM_NEW = "_source.Message";
        const string XCORRID_COL_NM_OLD = "X-CorrelationId";
        const string XCORRID_COL_NM_NEW = "_source.X-CorrelationId";
        const string REQUEST_PATH_COL_NM = "_source.RequestPath";
        private static readonly char[] CRLF = new char[] { '\r', '\n' };
        public static List<ESLogEntryEx> ReadOut(string srcCsv)
        {
            List<ESLogEntryEx> rslt = new();
            DataTable dt = Tools.Csv2DataTable(srcCsv, ',', null, 102400);
            string MsgColNm = dt.Columns.Contains(MESSAGE_COL_NM_OLD) ? MESSAGE_COL_NM_OLD : MESSAGE_COL_NM_NEW;
            string XCorrIdColNm = dt.Columns.Contains(XCORRID_COL_NM_OLD) ? XCORRID_COL_NM_OLD : XCORRID_COL_NM_NEW;
            foreach (DataRow dr in dt.Rows)
            {
                ESLogEntryEx curr = new();
                curr.Message = dr[MsgColNm] as string;
                curr.JSContent = ParseJSContent(curr.Message);
                if (dt.Columns.Contains(XCorrIdColNm))
                    curr.XCorrelationId = dr[XCorrIdColNm] as string;
                else
                    curr.XCorrelationId = ParseXCorrId(curr.Message);

                if (dt.Columns.Contains($"@{nameof(curr.timestamp_cw)}"))
                    curr.timestamp_cw = dr[$"@{nameof(curr.timestamp_cw)}"] as string;
                if (dt.Columns.Contains(nameof(curr.Level)))
                    curr.Level = dr[nameof(curr.Level)] as string;
                if (dt.Columns.Contains(REQUEST_PATH_COL_NM))
                    curr.RequestPath = dr[REQUEST_PATH_COL_NM] as string;
                rslt.Add(curr);
            }
            return rslt;
        }
        public static string ParseJSContent(string srcMsg) 
        {
            const string Token = "Content:";
            if (string .IsNullOrWhiteSpace(srcMsg))
                return string.Empty;
            int pos0 = srcMsg.IndexOf(Token);
            if (pos0 == -1)
                return string.Empty;
            return srcMsg.Substring(pos0 + Token.Length).Trim();
        }
        public static string ParseXCorrId(string srcMsg)
        {
            const string Token = "- X-CorrelationId: ";
            
            if (string.IsNullOrWhiteSpace(srcMsg))
                return string.Empty;
            int pos0 = srcMsg.IndexOf(Token);
            if (pos0 == -1)
                return string.Empty;
            int pos1 = srcMsg.IndexOfAny(CRLF, pos0 + Token.Length);
            if (pos1 == -1)
                return string.Empty;

            return srcMsg.Substring(pos0 + Token.Length, pos1 - (pos0 + Token.Length)).Replace("\n", "").Replace("\r", "").Trim();
        }

    }
}
