﻿using Albelli.MiscUtils.Lib.Excel;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.PCT9944
{
    public class ACSSCandidatesFilterer
    {
        private DataTable _matrixTable;
        private DataTable _zonesTable;
        public bool Debug { get; set; }
        private ACSSCandidatesFilterer() 
        {
            Debug = false;
        }
        public ACSSCandidatesFilterer(DataTable matrixTable, DataTable zonesTable) :this()
        {
            _matrixTable = matrixTable;
            _zonesTable = zonesTable;
        }

        public List<string> ResolveZones(CalculateCarrierModel request)
        {
            List<string> rslt = new List<string>();
            DataRow[] candidates = _zonesTable.Select($"Country = '{request.CountryId}'");
            if (candidates == null || candidates.Length == 0 ) return rslt;
            foreach(DataRow row in candidates)
            {
                string startZip = (row["From"] as string)?.Trim();
                string endZip = (row["To"] as string)?.Trim();
                string zoneCode = (row["ZoneCode"] as string)?.Trim();
                if (IsAnyOrNoZip(startZip) || IsAnyOrNoZip(endZip))
                    rslt.Add(zoneCode);
                else if (IsZipBetween(request.ZipCode, startZip, endZip))
                    rslt.Add(zoneCode);
            }
            return rslt.Distinct().ToList();
        }

        public static bool IsZipBetween(string zipCode, string startZip, string endZip)
        {
            return string.Compare(startZip, zipCode) <=0 && string.Compare(endZip, zipCode) >=0;
        }

        private bool IsAnyOrNoZip(string zipColVal)
        {
            return string.IsNullOrWhiteSpace(zipColVal) || zipColVal == "*";
        }
        private string DbgPfx(string methodName)
        {
            return $"{nameof(ACSSCandidatesFilterer)}.{methodName}";
        }
        public ACSSCandidatesFilteringResponse Filter(CalculateCarrierModel request)
        {
            if (Debug) Console.WriteLine($"{DbgPfx(nameof(Filter))}({request})");
            ACSSCandidatesFilteringResponse rslt = new();
            rslt.PreFilterQuery = GeneratePreFilterQuery(request);
            if (Debug) Console.WriteLine($"{DbgPfx(nameof(Filter))}::{nameof(rslt.PreFilterQuery)}: '{rslt.PreFilterQuery}'");
            DataRow[] candidates = PreFilter(rslt.PreFilterQuery);
            if (Debug) { Console.WriteLine($"{DbgPfx(nameof(Filter))}::{nameof(candidates)}:"); ExcelUtils.PrintDataRows(candidates, _matrixTable, Console.Out); }
            if (candidates == null || candidates.Length == 0) return rslt;
            List<ACSSMatrixEntry> matrixEntries = new();
            foreach(DataRow dr in candidates)
            {
                matrixEntries.Add(new ACSSMatrixEntry(dr));
            }
            foreach (var me in matrixEntries)
            {
                var evalRslt = EvaluateCandidate(me, request);
                if (Debug) { Console.WriteLine($"{DbgPfx(nameof(Filter))}::{nameof(evalRslt)}({me.PK()}):{evalRslt}"); }
                rslt.Candidates.Add(evalRslt);
            }
            return rslt;
        }

        private ACSSCandidatesFilteringResponseEntry EvaluateCandidate(ACSSMatrixEntry me, CalculateCarrierModel request)
        {
            ACSSCandidatesFilteringResponseEntry rslt = new();
            List<string> reasons = new();
            if (!ValidateDimension(me.MaxLength, request.Package.LengthInMm)) reasons.Add(nameof(me.MaxLength));
            if (!ValidateDimension(me.MaxWidth, request.Package.WidthInMm)) reasons.Add(nameof(me.MaxWidth));
            if (!ValidateDimension(me.MaxHeight, request.Package.HeightInMm)) reasons.Add(nameof(me.MaxHeight));
            if (!ValidateDimension(me.MinWeight, me.MaxWeight, request.Package.WeightInGrams)) reasons.Add(nameof(request.Package.WeightInGrams));
            string calcReason = ValidateCalculation(me.CalculationType, me.CalculationMaxLimit, request.Package);
            if (!string.IsNullOrWhiteSpace(calcReason)) reasons.Add(calcReason);
            rslt.Verdict = reasons.Any() ? ACSSFilteringVerdict.Rejected: ACSSFilteringVerdict.Served;
            rslt.NonMatchingFields = reasons.ToArray();
            rslt.MatrixRow = me;
            return rslt;
        }

        private string ValidateCalculation(string calculationType, int calculationMaxLimit, Package package)
        {
            if (calculationType == "*" || calculationMaxLimit == 0) return string.Empty;
            int calcDims = 0;
            switch(calculationType) 
            {
                case "L+W+H": calcDims = package.LengthInMm+ package.WidthInMm + package.HeightInMm; break;
                case "L+2D": calcDims = package.LengthInMm + 2*package.WidthInMm; break;
                case "L+2W+2H": calcDims = package.LengthInMm + 2 * package.WidthInMm + 2*package.HeightInMm; break;
                default:
                    return $"Unsupported {nameof(calculationType)}:'{calculationType}'.";
            }
            if (calcDims <= calculationMaxLimit) return string.Empty;
            return $"{calculationType} = {calcDims} vs {calculationMaxLimit}";
        }

        private bool ValidateDimension(int min, int max, int val)
        {
            if (min == 0 && max == 0) return true;
            return val >= min && val <= max;
        }

        private bool ValidateDimension(int max, int val)
        {
            if (max == 0 || val == 0) return true;
            return val <= max;
        }

        public string GeneratePreFilterQuery(CalculateCarrierModel request)
        {
            var zones = ResolveZones(request);
            if (!zones.Any()) return null;

            List<string> clauses = new List<string>();
            clauses.Add(BuildIsActiveClause(true));
            clauses.Add(BuildZonesWhereClause(zones));
            clauses.Add(BuildAnyStringClauseWorker("Package Type", request.Package.Type));
            clauses.Add(BuildAnyStringClauseWorker("Vendor", request.Brand));
            return string.Join(" AND ", clauses.ToArray());
        }

        public DataRow[] PreFilter(string query)
        {
            return _matrixTable.Select(query, "Priority ASC");
        }

        private string BuildAnyStringClauseWorker(string fieldName, string value)
        {
            return $"[{fieldName}] IN('{string.Join("', '", new string[] { value, "*"})}')";
        }

        private string BuildIsActiveClause(bool isActive)
        {
            return $"[Is Active] = '{isActive.ToString().ToLower()}'";
        }

        private string BuildZonesWhereClause(List<string> zones)
        {
            return $"[Zone Code] IN ('{string.Join("', '",zones.ToArray())}')";
        }
    }
}
