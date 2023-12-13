using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.PCT9944
{
    public static class DiscrepancyLogEntryParser
    {
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
            Tuple<int, int> counts = ExtractV1V2Counts(currMsgLns[1]);
            rslt.V1Count = counts.Item1;
            rslt.V2Count = counts.Item2;
            return rslt;
        }

        private static Tuple<int, int> ExtractV1V2Counts(string msgLn)
        {
            int v1 = 0;
            int v2 = 0;
            Regex rgx = new Regex("Centiro\\(V1\\) ([0-9]+) options vs ([0-9]+) in Centiro\\(V2\\)");
            var match = rgx.Match(msgLn);
            if (match.Success && match.Groups.Count == 3)
            {
                v1 = int.Parse(match.Groups[1].ToString());
                v2 = int.Parse(match.Groups[2].ToString());
            }
            return new Tuple<int, int>(v1, v2);
        }

        private static string ExtractPCT9944Input(string[] currMsg)
        {
            var rslt = ExtractSmcDelimValue(currMsg, 0).Trim().Substring(1);
            return rslt.Substring(0, rslt.Length - 1);
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
