using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.PCT9944
{
    public class PCT9247ReplayCompareV1vsV2Entry
    {
        public string CalculateCarrierModel { get; set; }
        public string CalculateCarrierModelShort { get; set; }
        public string v1ReqJson { get; set; }
        public string v2ReqJson { get; set; }
        public string v1Err { get; set; }
        public string v2Err { get; set; }
        public string XCorrelationId { get; set; }
        public string v1Status { get; set; }
        public string v2Status { get; set; }
        public string v1Resp { get; set; }
        public string v2Resp { get; set; }
        public int? v1Count { get; set; }
        public int? v2Count { get; set; }
        public string v1PKs { get; set; }
        public string v2PKs { get; set; }
        public string Diff { get; set; }
        public string Error { get; set; }

    }
}
