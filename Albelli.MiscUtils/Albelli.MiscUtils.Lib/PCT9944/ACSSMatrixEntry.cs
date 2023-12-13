using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.PCT9944
{
    public class ACSSMatrixEntry
    {
        public bool IsActive { get; set; }
        public string ZoneCode { get; set; }
        public string PackageType { get; set; }
        public string Vendor { get; set; }
        public string CalculationType { get; set; }
        public int MaxWeight { get; set; }
        public int MaxLength { get; set; }
        public int MaxWidth { get; set; }
        public int MaxHeight { get; set; }
        public int MinWeight { get; set; }
        public int CalculationMaxLimit { get; set; }
        public int Priority { get; set; }
        public string CarrierCode { get; set; }
        public string CarrierServiceCode { get; set; }
        public string ModeofTransport { get; set; }
        public string CarrierName { get; set; }
        public string DeliveryMethod { get; set; }
        public string CarrierServiceType { get; set; }
        public string NetworkID { get; set; }
        public string CarrierCodeLetter { get; set; }
        public string CarrierServiceKey { get; set; }
        public bool ShipmentPriceIncludeVat { get; set; }
        public bool ShipmentPriceIncludesShippingCost { get; set; }
        public string PackageLabel { get; set; }
        public string IsLetterbox { get; set; }
        public string Currency { get; set; }
        public double ImportTaxRatio { get; set; }

        public ACSSMatrixEntry() 
        { }
        public ACSSMatrixEntry(DataRow dr) 
        {
            IsActive = bool.Parse(dr["Is Active"] as string);
            ZoneCode = dr["Zone Code"] as string;
            PackageType = dr["Package Type"] as string;
            Vendor = dr["Vendor"] as string;
            CalculationType = dr["Calculation Type"] as string;
            MaxWeight = int.Parse(dr["Max Weight"] as string);
            MaxLength = int.Parse(dr["Max Length"] as string);
            MaxWidth = int.Parse(dr["Max Width"] as string);
            MaxHeight = int.Parse(dr["Max Height"] as string);
            MinWeight = int.Parse(dr["Min Weight"] as string);
            CalculationMaxLimit = int.Parse(dr["Calculation Max Limit"] as string);
            Priority = int.Parse(dr["Priority"] as string);
            CarrierCode = dr["Carrier Code"] as string;
            CarrierServiceCode = dr["Carrier Service Code"] as string;
            ModeofTransport = dr["Mode of Transport"] as string;
            CarrierName = dr["Carrier Name"] as string;
            DeliveryMethod = dr["Delivery Method"] as string;
            CarrierServiceType = dr["Carrier Service Type"] as string;
            NetworkID = dr["Network ID"] as string;
            CarrierCodeLetter = dr["Carrier Code Letter"] as string;
            CarrierServiceKey = dr["Carrier Service Key"] as string;
            ShipmentPriceIncludeVat = TryParseVariousBool(dr["Shipment Price Include Vat"] as string, false);
            ShipmentPriceIncludesShippingCost = TryParseVariousBool(dr["Shipment Price Includes Shipping Cost"] as string, false);
            PackageLabel = dr["Package Label"] as string;
            IsLetterbox = dr["IsLetterbox"] as string;
            Currency = dr["Currency"] as string;
            ImportTaxRatio = double.Parse(dr["Import Tax Ratio"] as string);
        }

        private bool TryParseVariousBool(string? val, bool defaultTo)
        {
            if (string.IsNullOrWhiteSpace(val)) return defaultTo;
            bool rslt;
            if (bool.TryParse(val.ToLower(), out rslt)) return rslt;
            int irslt;
            if (int.TryParse(val, out irslt)) return irslt != 0;
            return false;
        }

        public string PK()
        {
            var rslt = $"{PackageLabel}/{CarrierServiceKey}-{DeliveryMethod}/{CarrierServiceType}";
            if (!string.IsNullOrWhiteSpace(CarrierCodeLetter))
                rslt = $"{rslt}-'{CarrierCodeLetter}'";
            return rslt;
        }
    }
}
