using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Albelli.MiscUtils.Lib.PCT10481
{
    public class PCT10481ReplayResultRecord
    {
        public string OriginalErrorMsg { get; set; }
        public string PlantCode { get; set; }
        public string Brand { get; set; }
        public string CountryId { get; set; }
        public string ZipCode { get; set; }
        public string PackageType { get; set; }
        public string ModeOfTransport { get; internal set; }
        public string RequestEstimatedDeliveryDate { get; set; }
        public string RequestEstimatedShippingDate { get; set; }
        public string RequestEDD_DoW { get; set; }
        public string UATvsPROD { get; set; }
        public int carriersCount { get; set; }
        public int carriersCountProd { get; set; }
        public string carrierName { get; set; }
        public string carrierServiceId { get; set; }
        public string carrierServiceKey { get; set; }
        public string deliveryMethod { get; set; }
        public string deliveryType { get; set; }
        public string deliveryDate { get; set; }
        public string deliveryDate_DoW { get; internal set; }
        public string shippingDate { get; set; }
        public string shippingDate_DoW { get; set; }
        public string DeliverToShip { get; set; }
        public string ReqRespDeliveryDateEq { get; set; }
        public string carrierNameProd { get; set; }
        public string carrierServiceIdProd { get; set; }
        public string carrierServiceKeyProd { get; set; }
        public string deliveryMethodProd { get; set; }
        public string deliveryTypeProd { get; set; }
        public string deliveryDateProd { get; set; }
        public string shippingDateProd { get; set; }
        public string shippingDateProd_DoW { get; set; }
        public string OriginalXCorrelationId { get; set; }
        public string ReplayXCorrelationId { get; set; }
        public string ApiVersion { get; set; }
        public string RequestJSON { get; set; }
        public string ResponseJSON { get; set; }
        public string ResponseJSONProd { get; set; }
        public HttpStatusCode StatusUAT { get; set; }
        public HttpStatusCode StatusPROD { get; set; }
        public string ErrorUAT { get; set; }
        public string ErrorPROD { get; set; }
    }
}
