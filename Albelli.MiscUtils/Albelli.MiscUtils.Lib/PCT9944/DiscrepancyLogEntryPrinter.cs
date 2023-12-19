using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.PCT9944
{
    public static class DiscrepancyLogEntryPrinter
    {
        private const string JSONColNm = "JSON";
        private static readonly string[] Columns = new string[] {"timestamp_cw"
, "XCorrelationId"
, "V1Count"
, "V2Count"
, "CentiroV1"
, "CentiroV2"
, "DiffStr"
, "Missing"
, "Excessive"
, "Input"
, "PlantCode"
, "Brand"
, "CountryId"
, "ZipCode"
, "Package.LengthInMm"
, "Package.WidthInMm"
, "Package.HeightInMm"
, "Package.WeightInGrams"
, "Package.Type"
, "ArticleTypes"
, "DeliveryTypes"
, JSONColNm};
        private static readonly List<string> lColumns = new List<string>(Columns);
        public static string Print(List<DiscrepancyLogEntry> dles)
        {
            StringBuilder sb = new StringBuilder();
            PrintHeader(sb);
            foreach (var dle in dles)
            {
                PrintLine(dle, sb);
            }
            return sb.ToString();
        }

        private static void PrintLine(DiscrepancyLogEntry dle, StringBuilder sb)
        {
            List<string> currLine = new List<string>(Columns.Length);
            lColumns.ForEach(col => { currLine.Add(string.Empty); });
            InsertValue(dle.timestamp_cw, nameof(dle.timestamp_cw), currLine);
            InsertValue(dle.XCorrelationId, nameof(dle.XCorrelationId), currLine);
            InsertValue(dle.V1Count, nameof(dle.V1Count), currLine);
            InsertValue(dle.V2Count, nameof(dle.V2Count), currLine);
            InsertValue(dle.CentiroV1, nameof(dle.CentiroV1), currLine);
            InsertValue(dle.CentiroV2, nameof(dle.CentiroV2), currLine);
            InsertValue(dle.DiffStr, nameof(dle.DiffStr), currLine);
            InsertValue(dle.Missing, nameof(dle.Missing), currLine);
            InsertValue(dle.Excessive, nameof(dle.Excessive), currLine);
            InsertValue(dle.Input, nameof(dle.Input), currLine);
            InsertValue(dle.ParsedInput.PlantCode, nameof(dle.ParsedInput.PlantCode), currLine);
            InsertValue(dle.ParsedInput.Brand, nameof(dle.ParsedInput.Brand), currLine);
            InsertValue(dle.ParsedInput.CountryId, nameof(dle.ParsedInput.CountryId), currLine);
            InsertValue(dle.ParsedInput.ZipCode, nameof(dle.ParsedInput.ZipCode), currLine);
            InsertValue(dle.ParsedInput.Package.LengthInMm.ToString(), $"{nameof(dle.ParsedInput.Package)}.{nameof(dle.ParsedInput.Package.LengthInMm)}", currLine);
            InsertValue(dle.ParsedInput.Package.WidthInMm.ToString(), $"{nameof(dle.ParsedInput.Package)}.{nameof(dle.ParsedInput.Package.WidthInMm)}", currLine);
            InsertValue(dle.ParsedInput.Package.HeightInMm.ToString(), $"{nameof(dle.ParsedInput.Package)}.{nameof(dle.ParsedInput.Package.HeightInMm)}", currLine);
            InsertValue(dle.ParsedInput.Package.WeightInGrams.ToString(), $"{nameof(dle.ParsedInput.Package)}.{nameof(dle.ParsedInput.Package.WeightInGrams)}", currLine);
            InsertValue(dle.ParsedInput.Package.Type, $"{nameof(dle.ParsedInput.Package)}.{nameof(dle.ParsedInput.Package.Type)}", currLine);
            InsertValue(string.Join(", ",dle.ParsedInput.ArticleTypes), nameof(dle.ParsedInput.ArticleTypes), currLine);
            InsertValue(string.Join(", ", dle.ParsedInput.DeliveryTypes), nameof(dle.ParsedInput.DeliveryTypes), currLine);
            InsertValue(JsonConvert.SerializeObject(dle, Formatting.None), JSONColNm, currLine);
            sb.AppendLine(string.Join('\t', currLine.ToArray()));
        }

        private static void InsertValue(int val, string heading, List<string> target)
        {
            InsertValue(val.ToString(), heading, target);
        }
        private static void InsertValue(string val, string heading, List<string> target)
        {
            target[lColumns.IndexOf(heading)] = val;
        }

        private static void PrintHeader(StringBuilder sb)
        {
            sb.AppendLine(string.Join('\t', Columns));
        }
    }
}
