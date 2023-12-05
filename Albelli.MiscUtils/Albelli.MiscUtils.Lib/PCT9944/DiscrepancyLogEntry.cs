using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.PCT9944
{
    public class DiscrepancyLogEntry
    {
        public string Input { get; set; }
        public string CentiroV1 { get; set; }
        public string CentiroV2 { get; set; }
        public string DiffStr 
        {
            get => $"{CentiroV1} vs {CentiroV2}";
        }
        public string Missing { get; set; }
        public string Excessive { get; set; }
        public string XCorrelationId { get; set; }
        public string timestamp_cw { get; set; }
        public string Plant { get; set; }
        public static DiscrepancyLogEntry Parse(string logMsg)
        {
            string[] currMsgLns = logMsg.Split('\n');
            for (int i = 0; i < currMsgLns.Length; i++)
            {
                currMsgLns[i] = currMsgLns[i].Replace("\r", "");
            }
            var rslt = new DiscrepancyLogEntry()
            {
                Input = ExtractPCT9944Input(currMsgLns),
                CentiroV1 = ExtractV1(currMsgLns),
                CentiroV2 = ExtractV2(currMsgLns),
                Missing = ExtractPCT9944Missing(currMsgLns),
                Excessive = ExtractPCT9944Excessive(currMsgLns)
                
            };
            rslt.Plant = ParsePlantCode(rslt.Input);
            return rslt;
        }

        private static string ParsePlantCode(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;
            int pos0 = input.IndexOf('/');
            if (pos0 == -1) return string.Empty;
            return input.Substring(0, pos0);
        }
        private static string ExtractPCT9944Input(string[] currMsg)
        {
            var rslt = ExtractSmcDelimValue(currMsg, 0).Trim().Substring(1);
            return rslt.Substring(0,rslt.Length - 1);
        }

        private static string ExtractSmcDelimValue(string[] currMsg, int lnIdx)
        {
            if (currMsg == null || currMsg.Length < lnIdx + 1) return string.Empty;
            int pos0 = currMsg[lnIdx].IndexOf(':');
            if (pos0 == -1) return string.Empty;
            return currMsg[lnIdx].Substring(pos0 + 1).Trim();
        }
        private static string ExtractV1(string[] currMsg)
        {
            return ExtractSmcDelimValue(currMsg, 2);
        }
        private static string ExtractV2(string[] currMsg)
        {
            return ExtractSmcDelimValue(currMsg, 3);
        }

        private static string ExtractPCT9944Missing(string[] currMsg)
        {
            return ExtractSmcDelimValue(currMsg, 4);
        }

        private static string ExtractPCT9944Excessive(string[] currMsg)
        {
            return ExtractSmcDelimValue(currMsg, 5);
        }

    }
}
