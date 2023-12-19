using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.PCT9944
{
    public static class NoDiscrepancyLogEntryPrinter
    {
        private const string JSONColNm = "JSON";
        private static readonly string[] Columns = new string[] {"timestamp_cw"
, "XCorrelationId"
, "Centiro"
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
        public static string Print(List<NoDiscrepancyLogEntry> dles, bool withJson = true)
        {
            StringBuilder sb = new StringBuilder();
            PrintHeader(sb, withJson);
            foreach (var dle in dles)
            {
                PrintLine(dle, sb, withJson);
            }
            return sb.ToString();
        }

        private static void PrintLine(NoDiscrepancyLogEntry dle, StringBuilder sb, bool withJson)
        {
            if (dle == null)
                return;
            List<string> currLine = new List<string>(Columns.Length);
            lColumns.ForEach(col => { currLine.Add(string.Empty); });
            InsertValue(dle.timestamp_cw, nameof(dle.timestamp_cw), currLine);
            InsertValue(dle.XCorrelationId, nameof(dle.XCorrelationId), currLine);
            InsertValue(dle.Centiro, nameof(dle.Centiro), currLine);
            InsertValue(dle.Input, nameof(dle.Input), currLine);
            if (dle?.ParsedInput != null)
            {
                InsertValue(dle?.ParsedInput?.PlantCode, nameof(dle.ParsedInput.PlantCode), currLine);
                InsertValue(dle?.ParsedInput?.Brand, nameof(dle.ParsedInput.Brand), currLine);
                InsertValue(dle?.ParsedInput?.CountryId, nameof(dle.ParsedInput.CountryId), currLine);
                InsertValue(dle?.ParsedInput?.ZipCode, nameof(dle.ParsedInput.ZipCode), currLine);
                InsertValue(dle?.ParsedInput?.Package?.LengthInMm.ToString(), $"{nameof(dle.ParsedInput.Package)}.{nameof(dle.ParsedInput.Package.LengthInMm)}", currLine);
                InsertValue(dle?.ParsedInput?.Package?.WidthInMm.ToString(), $"{nameof(dle.ParsedInput.Package)}.{nameof(dle.ParsedInput.Package.WidthInMm)}", currLine);
                InsertValue(dle?.ParsedInput?.Package?.HeightInMm.ToString(), $"{nameof(dle.ParsedInput.Package)}.{nameof(dle.ParsedInput.Package.HeightInMm)}", currLine);
                InsertValue(dle?.ParsedInput?.Package?.WeightInGrams.ToString(), $"{nameof(dle.ParsedInput.Package)}.{nameof(dle.ParsedInput.Package.WeightInGrams)}", currLine);
                InsertValue(dle?.ParsedInput?.Package?.Type, $"{nameof(dle.ParsedInput.Package)}.{nameof(dle.ParsedInput.Package.Type)}", currLine);
                InsertValue(string.Join(", ", dle?.ParsedInput?.ArticleTypes), nameof(dle.ParsedInput.ArticleTypes), currLine);
                InsertValue(string.Join(", ", dle?.ParsedInput?.DeliveryTypes), nameof(dle.ParsedInput.DeliveryTypes), currLine);
                if (withJson)
                {
                    try { InsertValue(JsonConvert.SerializeObject(dle?.ParsedInput, Formatting.None), JSONColNm, currLine); } catch (Exception ex) { InsertValue($"Error:{ex.Message}", JSONColNm, currLine); }
                }
            }
            sb.AppendLine(string.Join('\t', currLine.ToArray()));
        }

        private static void InsertValue(string val, string heading, List<string> target)
        {
            target[lColumns.IndexOf(heading)] = val;
        }

        private static void PrintHeader(StringBuilder sb, bool withJson)
        {
            List<string> actualCols = new List<string>(Columns);
            if (!withJson)
                actualCols.Remove(JSONColNm);

            sb.AppendLine(string.Join('\t', actualCols.ToArray()));
        }
    }
}
