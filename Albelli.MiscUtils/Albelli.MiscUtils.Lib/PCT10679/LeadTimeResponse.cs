using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.PCT10679
{
    public class LeadTimeResponse
    {
        public string PlantCode { get; set; }

        public string ArticleCode { get; set; }

        public LeadTime LeadTime { get; set; }

        public List<ProductOptionLeadTime> ProductOptionLeadTimes { get; set; }

    }

}
