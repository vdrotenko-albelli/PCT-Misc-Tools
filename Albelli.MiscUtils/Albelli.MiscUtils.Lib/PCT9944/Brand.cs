using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.PCT9944
{
    public class Brand
    {
        private const int VistaprintDealerId = 2000;
        private const int DefaultDealerId = 0;

        public static bool IsVistaprintBrand(string brand)
        {
            return string.Equals(brand, "vistaprint");
        }

        public static int GetDealerIdByBrand(string brand)
        {
            if (IsVistaprintBrand(brand))
            {
                return VistaprintDealerId;
            }

            return DefaultDealerId;
        }
    }
}
