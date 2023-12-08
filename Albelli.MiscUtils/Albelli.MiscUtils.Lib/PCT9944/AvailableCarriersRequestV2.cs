using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.PCT9944
{
    public class AvailableCarriersRequestV2// : IRequest<IEnumerable<AvailableCarriersResponse>>
    {
        public string PlantCode { get; set; }
        public string Brand { get; set; }
        public string CountryId { get; set; }
        public string ZipCode { get; set; }
        /// <summary>
        /// Optional parameter specifying if customer has chosen PUDO delivery
        /// </summary>
        public string PreferredDeliveryMethod { get; set; }
        public PackageV2 Package { get; set; }
        /// <summary>
        /// Is used for ShippingDate backward calculation in Centiro
        /// </summary>
        public DateTime? EstimatedDeliveryDate { get; set; }
        /// <summary>
        /// Is used for DeliveryDate forward calculation in Centiro
        /// </summary>
        public DateTime? EstimatedShippingDate { get; set; }
    }
}
