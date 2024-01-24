using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.PCT9944
{
    public class DeliveryMethod
    {
        private static readonly IList<string> _deliveryMethods = new List<string> { Constants.DeliveryMethods.Home, Constants.DeliveryMethods.Pudo };

        public static IList<string> DeliveryMethods => _deliveryMethods;
        public static string HomeDeliveryMethod => Constants.DeliveryMethods.Home;
        public static string PudoDeliveryMethod => Constants.DeliveryMethods.Pudo;
    }
}
