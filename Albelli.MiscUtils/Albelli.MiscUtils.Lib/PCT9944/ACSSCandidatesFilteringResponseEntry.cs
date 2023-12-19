using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.PCT9944
{
    public class ACSSCandidatesFilteringResponseEntry
    {
        public ACSSFilteringVerdict Verdict { get; set; }
        public string[] NonMatchingFields { get; set; }
        public ACSSMatrixEntry MatrixRow { get; set; }

        public override string ToString()
        {
            if (Verdict == ACSSFilteringVerdict.Served)
                return $"{Verdict}";
            return $"{Verdict}({string.Join(',', NonMatchingFields != null ? NonMatchingFields : new string[] { })})";
        }
    }
}
