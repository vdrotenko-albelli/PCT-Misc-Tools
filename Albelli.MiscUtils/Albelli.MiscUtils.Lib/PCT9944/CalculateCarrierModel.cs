﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.PCT9944
{
    public class CalculateCarrierModel
    {
        public CalculateCarrierModel() {
            ArticleTypes = new string[] { };
            DeliveryMethods = new string[] { };
            DeliveryTypes = new string[] { };

        }

        public string PlantCode { get; set; }
        public int DealerId { get; set; }
        public string Brand { get; set; }
        public string CountryId { get; set; }
        public string ZipCode { get; set; }
        public Package Package { get; set; }
        public IEnumerable<string> ArticleTypes { get; set; }
        public bool InEu { get; set; }
        public IEnumerable<string> DeliveryTypes { get; set; }
        public IEnumerable<string> DeliveryMethods { get; set; }
        public bool? ExcludeInternalOnlyCarriers { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? EstimatedShippingDate { get; set; }

        // Forward calculation: DeliveryDate = shipdate + shippingLeadTime
        public bool IsDeliveryDateCalculation => EstimatedShippingDate.HasValue;
        // Backward calculation: ShipDate = DeliveryDate - ShippingLeadTime 
        public bool IsShipDateCalculation => EstimatedDeliveryDate.HasValue;
        public bool FilterOutPudoForLetterBox { get; set; }

        public static explicit operator AvailableCarriersRequestV2(CalculateCarrierModel model)
        {
            if (model == null) { return null; }
            AvailableCarriersRequestV2 rslt = new AvailableCarriersRequestV2();
            rslt.Brand = model.Brand;
            rslt.CountryId = model.CountryId;
            rslt.ZipCode = model.ZipCode;
            rslt.EstimatedDeliveryDate = model.EstimatedDeliveryDate;
            rslt.EstimatedShippingDate = model.EstimatedShippingDate;
            rslt.Package = new PackageV2()
            {
                Dimensions = new Dimensions()
                {
                    Length = model.Package.LengthInMm,
                    Width = model.Package.WidthInMm,
                    Height = model.Package.HeightInMm,
                    Unit = "mm"
                },
                Weight = new Weight()
                {
                    Unit = "g",
                    Value = model.Package.WeightInGrams
                },
                Type = model.Package.Type
            };
            rslt.PlantCode = model.PlantCode;
            return rslt;
        }

        public static explicit operator CalculateCarrierModel(AvailableCarriersRequestV2 model)
        {
            if (model == null) { return null; }
            CalculateCarrierModel rslt = new CalculateCarrierModel();
            rslt.Brand = model.Brand;
            rslt.CountryId = model.CountryId;
            rslt.ZipCode = model.ZipCode;
            rslt.EstimatedDeliveryDate = model.EstimatedDeliveryDate;
            rslt.EstimatedShippingDate = model.EstimatedShippingDate;
            rslt.Package = new Package()
            {
                LengthInMm = (int)model.Package.Dimensions.Length,
                WidthInMm = (int)model.Package.Dimensions.Width,
                HeightInMm = (int)model.Package.Dimensions.Height,
                WeightInGrams = (int)model.Package.Weight.Value,
                Type = model.Package.Type
            };
            rslt.PlantCode = model.PlantCode;
            return rslt;
        }
        private string IEnumerableToString(IEnumerable<string> values)
        {
            return true != values?.Any() ? "empty" : string.Join(",", values);
        }
        public override string ToString()
        {
            return $"{PlantCode}/{Brand} => {CountryId}/'{ZipCode}', {Package.Type} LxWxH: {Package.LengthInMm} x {Package.WidthInMm} x {Package.HeightInMm}mm / {Package.WeightInGrams}g, {nameof(ArticleTypes)}: {IEnumerableToString(ArticleTypes)}, {nameof(DeliveryTypes)}: {IEnumerableToString(DeliveryTypes)}";
        }

    }


}
