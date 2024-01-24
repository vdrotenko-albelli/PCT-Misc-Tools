using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.PCT9944.v1
{
    public class Package
    {
        [Required]
        public int LengthInMm { get; set; }
        [Required]
        public int WidthInMm { get; set; }
        [Required]
        public int HeightInMm { get; set; }
        [Required]
        public int WeightInGrams { get; set; }
        [Required]
        public string Type { get; set; }
    }
}
