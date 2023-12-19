using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.PCT9944
{
    public static class Constants
    {
        public static class TrackingUrl
        {
            public const string TrackingId = "TRACKING_ID";
            public const string ZipCode = "ZIP_CODE";
        }

        public static class TemplateKeys
        {
            public const string OrderCode = "ordercode";
            public const string AmountText = "amounttext";
            public const string PackageItemsQuantitySum = "shipmentitemcount";
            public const string CarrierCode = "carriercode";
            public const string FlyerNumber = "insert";
            public const string CarrierName = "carriername";
            public const string ShipmentId = "shipmentid";
            public const string LabelId = "labelid";
            public const string CompanyName = "companyname";
            public const string LastName = "lastname";
            public const string Street = "street";
            public const string StreetNumber = "streetnumber";
            public const string Address2 = "address2";
            public const string ZipCode = "zipcode";
            public const string City = "city";
            public const string FullCountry = "fullcountry";
        }

        public static class MissingFileErrorMessages
        {
            private const string HintMessage = "Hint: either file is missing or case sensitivity is wrong.";
            public const string MissingTemplateFile = $"Template file should be part of LabelTemplates folder. {HintMessage}";
            public const string MissingResourcesFile = $@"Resources file should be part of LabelTemplates\Resources folder. {HintMessage}";
            public const string Missing2dBarcodesFile = $@"Barcode file should be part of LabelTemplates\2dBarcodes folder. {HintMessage}";
        }

        public static class CentiroConstants
        {
            public const string PrinterType = "Zebra300";
            public const string ShipmentType = "Outbound";
            public const string SortingCodeKey = "SORTINGCODE";
            public const string FlyperNumberKey = "FLYERNUMBER";
            public const string InvoiceNumberKey = "InvoiceNr";
            public const string ShipmentPrice = "SHIPMENTPRICE";
            public const string AddressTypeSender = "Sender";
            public const string AddressTypeReturnTo = "ReturnTo";
            public const string AddressTypeReceiver = "Receiver";
            public const string AddressTypeCollectionpoint = "Collectionpoint";
            public const string CentiroProviderPrefix = "tms:";
            public const string CN23SearchCriteria = "CN23";

            public static class SenderCodes
            {
                public const string NLH = "FR@albelli.com";
                public const string YPB = "NL@albelli.com";
                public const string RRD = "CZ@albelli.com";
                public const string ELA = "DE@albelli.com";
                public const string KHS = "KHS@albelli.com";
                public const string ORWO = "ORWO@albelli.com";
                public const string PAT = "PAT@albelli.com";
                public const string RAV = "RAV@albelli.com";
                public const string YPBVP = "YPB-VP@albelli.com";
                public const string EKT = "EKT@albelli.com";
                public const string PP = "PP@albelli.com";
                public const string WFR = "WFR@albelli.com";
            }
        }

        public static class CarriersRecsKeys
        {
            public const string ColisPrive = "recs:colis-prive";
            public const string GlsNl = "recs:gls-netherlands";
            public const string GlsDe = "recs:gls-germany-economy";
        }

        public static class CarrierNames
        {
            public const string Over20kg = nameof(Over20kg);
            public const string Calberson = nameof(Calberson);
        }

        public static class ShippingLabelMediaTypes
        {
            public const string PngImage = "image/png";
            public const string JpegImage = "image/jpeg";
            public const string Zpl = "zpl";
        }

        public static class DeliveryMethods
        {
            public const string Pudo = "PUDO";
            public const string Home = "Home";
        }

        public static class DeliveryTypes
        {
            public const string Standard = "Standard";
            public const string Express = "Express";
        }

        public static class PackageLabel
        {
            public const decimal DefaultLabelWidthInches = 4;
            public const decimal DefaultLabelHeightInches = 6;
        }

        public static class PlantCode
        {
            public const string NLH = nameof(NLH);
            public const string YPB = nameof(YPB);
            public const string RRD = nameof(RRD);
            public const string ELA = nameof(ELA);
            public const string KHS = nameof(KHS);
            public const string ORWO = nameof(ORWO);
            public const string PAT = nameof(PAT);
            public const string RAV = nameof(RAV);
            public const string EKT = nameof(EKT);  // Exakta
            public const string PP = nameof(PP);
            public const string WFR = nameof(WFR);
        }

        public static class Customs
        {
            public const string ExportReason = "Sales of goods";
            public const string ExportReasonCode = "11";

            /// <summary>
            /// EORI number - Economic Operators Registration and Identification number
            /// Important identification number used for access within the EU countries in terms of imports and exports. 
            /// This number is formed by the country code where the company is registered, and a unique code number, followed by the VAT number.
            /// </summary>
            public static class EORINumber
            {
                public const string YPB = "NL815430188";
                public const string NLH = "FR47843883100057";
                public const string Unknown = "unknown";
            }
        }

        public static class PrintModes
        {
            // TearOff mode
            public const string DefaultMode = "^MMT";
            public const string PeelOffMode = "^MMP";
            public const string CutterMode = "^MMC";
        }

        public static readonly Dictionary<string, string> TimeZoneIdPerPlantCode = new()
    {
        { PlantCode.YPB, "W. Europe Standard Time" },
        { PlantCode.NLH, "W. Europe Standard Time" },
        { PlantCode.RRD, "Central Europe Standard Time"},
        { PlantCode.ELA, "W. Europe Standard Time" },
        { PlantCode.KHS, "W. Europe Standard Time" },
        { PlantCode.ORWO,"W. Europe Standard Time" },
        { PlantCode.PAT, "Romance Standard Time" },
        { PlantCode.RAV, "W. Europe Standard Time" },
        { PlantCode.EKT, "Central Europe Standard Time" },
        { PlantCode.PP, "GMT Standard Time" },
        { PlantCode.WFR, "GMT Standard Time" }
    };
    }
}
