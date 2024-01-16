using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.PCT10679
{
    public class FlattenedLeadTime
    {
        public string PlantCode { get; set; }
        public string ArticleCode { get; set; }
        public string ProductOptionName { get; set; }
        public string ProductOptionValue { get; set; }
        public int BaseLeadTime { get; set; }
        public int? ExtraLeadTime { get; set; }

    }
}
