using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.PCT9944
{
    public class PromiseEngineDeliveryRequest
    {
        public string DeliveryId { get; set; }  // we can hide this from clients/users of this endpoint and handle deliveryId internally
        public List<DeliveryOption> Options { get; set; }
        public List<PackageInfo> Packages { get; set; }
        public List<Metadata> AdditionalInfo { get; set; }
    }
}
