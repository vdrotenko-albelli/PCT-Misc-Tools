using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.PCT9944
{
    public static class NoDiscrepancyLogEntryParser
    {
        private const string ReturnedClause = ", both Centiro(V1) and Centiro(V2) returned:";
        public static NoDiscrepancyLogEntry Parse(string logMsg)
        {
            var rslt = new NoDiscrepancyLogEntry()
            {
                Input = ExtractPCT9944Input(logMsg),
                Centiro = ExtractCentiroResponse(logMsg),
            };
            //rslt.Plant = CalculateCarrierModelParser.ParsePlantCode(rslt.Input);
            return rslt;
        }
        private static string ExtractPCT9944Input(string msg)
        {
            const string prefix = "No discrepancy for";
            int pos0 = msg.IndexOf(prefix);
            if (pos0 == -1) { return null; }
            int pos1 = msg.IndexOf(ReturnedClause, pos0 + prefix.Length);
            if (pos1 == -1) {  pos1 = msg.Length-1; }
            return msg.Substring(pos0 + prefix.Length, pos1 - (pos0 + prefix.Length)).Trim();
        }

        private static string ExtractSmcDelimValue(string[] currMsg, int lnIdx)
        {
            if (currMsg == null || currMsg.Length < lnIdx + 1) return string.Empty;
            int pos0 = currMsg[lnIdx].IndexOf(':');
            if (pos0 == -1) return string.Empty;
            return currMsg[lnIdx].Substring(pos0 + 1).Trim();
        }
        private static string ExtractCentiroResponse(string msg)
        {
            int pos0 = msg.IndexOf(ReturnedClause);
            if (pos0 == -1) return null;
            return msg.Substring(pos0 + ReturnedClause.Length, msg.Length - (pos0 + ReturnedClause.Length)).Trim();
        }
    }
}
