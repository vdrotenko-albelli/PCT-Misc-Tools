using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.PCT9944
{
    public class PickupPointsRequest //: IRequest<PickupPointsResponse>
    {
        [JsonIgnore]
        public string NetworkId { get; set; }
        public string PostalCode { get; set; }
        public string CountryCode { get; set; }
        public double? MaxDistanceInKm { get; set; }
        public int? MaxCollectionPoints { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
