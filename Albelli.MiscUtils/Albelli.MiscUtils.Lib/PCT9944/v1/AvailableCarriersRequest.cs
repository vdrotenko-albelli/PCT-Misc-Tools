using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.PCT9944.v1
{
    public class AvailableCarriersRequest// : IRequest<IEnumerable<AvailableCarriersResponse>>
    {
        [Required]
        public string PlantCode { get; set; }
        [Required]
        public string Brand { get; set; }
        [Required]
        public string CountryId { get; set; }
        public string ZipCode { get; set; }
        /// <summary>
        /// Optional parameter specifying if customer has chosen PUDO delivery
        /// </summary>
        public string PreferredDeliveryMethod { get; set; }
        [Required]
        public Package Package { get; set; }
        [Required]
        [MinLength(1)]
        public IEnumerable<string> ArticleTypes { get; set; }
        public bool? ExcludeInternalOnlyCarriers { get; set; }
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
