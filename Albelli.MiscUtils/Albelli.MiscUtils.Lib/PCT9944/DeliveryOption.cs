using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.PCT9944
{
    public class DeliveryOption
    {
        public bool IsReverseCalculation { get; set; }
        public DateTime DispatchDate { get; set; }
        public string SenderCode { get; set; }
        public Address SenderAddress { get; set; }
        public Address ReceiverAddress { get; set; }
        public string Vendor { get; set; }
        public IEnumerable<string> DeliveryTypes { get; set; }
        public IEnumerable<string> DeliveryMethods { get; set; }
    }
}
