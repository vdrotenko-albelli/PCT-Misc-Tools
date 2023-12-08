using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.PCT9944
{
    public class PackageV2
    {
        public Dimensions Dimensions { get; set; }
        public Weight Weight { get; set; }
        public string Type { get; set; }
    }

    public class Dimensions
    {
        public string Unit { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }
    public class Weight
    {
        public string Unit { get; set; }
        public double Value { get; set; }
    }

}
