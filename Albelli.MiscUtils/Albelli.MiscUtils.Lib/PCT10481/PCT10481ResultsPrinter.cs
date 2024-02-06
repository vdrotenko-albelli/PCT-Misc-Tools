using Albelli.MiscUtils.Lib.ESLogs;
using Albelli.MiscUtils.Lib.PCT9944;
using Albelli.MiscUtils.Lib.PCT9944.v1;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static Albelli.MiscUtils.Lib.PCT9944.Constants;

namespace Albelli.MiscUtils.Lib.PCT10481
{
    public static class PCT10481ResultsPrinter
    {
        public static void FillInResults(List<PCT10481ReplayResultRecord> target, ESLogEntryEx payloadLogEntry, string newXCorr, bool isV1, ESLogEntryEx centralLogEntry, Tuple<HttpStatusCode, string> uatResp, Tuple<HttpStatusCode, string> prodResp)
        {
            int[] carrCnts = new int[2] { 0, 0 };
            List<AvailableCarriersResponse> carriersUAT = uatResp.Item1 == HttpStatusCode.OK ? JsonConvert.DeserializeObject<List<AvailableCarriersResponse>>(uatResp.Item2) : null;
            List<AvailableCarriersResponse> carriersPROD = prodResp.Item1 == HttpStatusCode.OK ? JsonConvert.DeserializeObject<List<AvailableCarriersResponse>>(prodResp.Item2) : null;
            carrCnts[0] = carriersUAT?.Count ?? 0;
            carrCnts[1] = carriersPROD?.Count ?? 0;
            int carriersCnt = Math.Max(carrCnts[0], carrCnts[1]);
            AvailableCarriersRequest reqV1 = isV1 ? JsonConvert.DeserializeObject<AvailableCarriersRequest>(payloadLogEntry.JSContent) : null;
            AvailableCarriersRequestV2 reqV2 = isV1 ? null : JsonConvert.DeserializeObject<AvailableCarriersRequestV2>(payloadLogEntry.JSContent);
            for (int i = 0; i < carriersCnt; i++)
            {
                AvailableCarriersResponse carrUat = i < carriersUAT?.Count ? carriersUAT[i] : null;
                AvailableCarriersResponse carrProd = i < carriersPROD?.Count ? carriersPROD[i] : null;

                var currRslt = new PCT10481ReplayResultRecord()
                {
                    OriginalErrorMsg = centralLogEntry.Message,
                    PlantCode = isV1 ? reqV1.PlantCode : reqV2.PlantCode,
                    UATvsPROD = Compare(carrUat, carrProd),
                    RequestEstimatedDeliveryDate = isV1 ? reqV1.EstimatedDeliveryDate?.ToString("s") : reqV2.EstimatedDeliveryDate?.ToString("s"),
                    RequestEstimatedShippingDate= isV1 ? reqV1.EstimatedShippingDate?.ToString("s") : reqV2.EstimatedShippingDate?.ToString("s"),
                    carrierName = carrUat?.CarrierName,
                    carrierServiceId = carrUat?.CarrierServiceId,
                    carrierServiceKey = carrUat?.CarrierServiceKey,
                    deliveryMethod = carrUat?.DeliveryMethod,
                    deliveryType = carrUat?.DeliveryType,
                    deliveryDate = carrUat?.DeliveryDate?.ToString("s"),
                    shippingDate = carrUat?.ShippingDate?.ToString("s"),
                    carrierNameProd = carrProd?.CarrierName,
                    carrierServiceIdProd = carrProd?.CarrierServiceId,
                    carrierServiceKeyProd = carrProd?.CarrierServiceKey,
                    deliveryMethodProd = carrProd?.DeliveryMethod,
                    deliveryTypeProd = carrProd?.DeliveryType,
                    deliveryDateProd = carrProd?.DeliveryDate?.ToString("s"),
                    shippingDateProd = carrProd?.ShippingDate?.ToString("s"),
                    OriginalXCorrelationId = centralLogEntry.XCorrelationId,
                    ReplayXCorrelationId = newXCorr,
                    ApiVersion = isV1 ? "v1" : "v2",
                    RequestJSON = payloadLogEntry?.JSContent,
                    ResponseJSON = uatResp.Item2,
                    ResponseJSONProd = prodResp.Item2,
                    StatusUAT = uatResp.Item1,
                    StatusPROD = prodResp.Item1,
                    ErrorUAT = uatResp.Item1 != HttpStatusCode.OK ? uatResp.Item2 : string.Empty,
                    ErrorPROD = prodResp.Item1 != HttpStatusCode.OK ? prodResp.Item2 : string.Empty
                };
                target.Add(currRslt);
            }
        }

        private static string Compare(AvailableCarriersResponse? carrUat, AvailableCarriersResponse? carrProd)
        {
            if (carrUat == null && carrProd == null) return 0.ToString();
            if (carrUat != null && carrProd == null) return "not null vs null";
            if (carrUat == null && carrProd != null) return "null vs not null";
            if (carrUat.CarrierName == carrProd.CarrierName && carrUat.CarrierServiceKey == carrProd.CarrierServiceKey
                && carrUat.CarrierServiceId == carrProd.CarrierServiceId && carrUat.DeliveryMethod == carrUat.DeliveryMethod
                && carrUat.DeliveryType == carrProd.DeliveryType)
            {
                return carrUat.DeliveryDate == carrProd.DeliveryDate ? 0.ToString() : "diff date";
            }
            else
                return "diff carrier specs/method/type";
        }
    }
}
