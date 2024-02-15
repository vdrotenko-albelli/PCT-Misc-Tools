using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.PCT9944
{
    public class AvailableCarriersResponse
    {
        public string PlantCode { get; set; }
        public string CarrierName { get; set; }
        /// <summary>
        /// Key to create shipment and print shipping label with SAPI or Centiro
        /// Prefix of this value drives the decistion:
        ///   starting with "recs:" (for example recs:postnl-smo) means that the shipment is created with SAPI
        ///   starting with "tms:" (for example tms:postnl-smo) means that the shipment is created with Centiro
        /// </summary>
        /// 
        // TODO: Should this be removed?
        // Is not used in Plant API and DO API
        public string CarrierServiceKey { get; set; }
        public string CarrierServiceId { get; set; }
        /// <summary>
        /// Standard or Express
        /// </summary>
        public string DeliveryType { get; set; }
        /// <summary>
        /// Home or PUDO (pickup point)
        /// </summary>
        public string DeliveryMethod { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? ShippingDate { get; set; }
        /// <summary>
        /// Carrier network id (applicable only to DeliveryMethod = PUDO)
        /// </summary>
        public string NetworkId { get; set; }

        public object PK()
        {
            return $"{CarrierServiceId}/{CarrierServiceKey}-{DeliveryMethod}/{DeliveryType}-'?'";
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            AvailableCarriersResponse second = obj as AvailableCarriersResponse;
            if (second == null) return false;
            return PlantCode == second.PlantCode
                && CarrierName == second.CarrierName
                && CarrierServiceKey == second.CarrierServiceKey
                && CarrierServiceId == second.CarrierServiceId
                && DeliveryType == second.DeliveryType
                && DeliveryMethod == second.DeliveryMethod
                && DeliveryDate == second.DeliveryDate
                && ShippingDate == second.ShippingDate
                && NetworkId == second.NetworkId;
        }
    }
}
