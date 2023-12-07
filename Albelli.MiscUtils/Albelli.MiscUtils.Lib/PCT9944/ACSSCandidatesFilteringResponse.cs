using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.PCT9944
{
    public class ACSSCandidatesFilteringResponse
    {
        public ACSSCandidatesFilteringResponse() {
            Candidates = new();
        }
        public string PreFilterQuery { get; set; }
        public List<ACSSCandidatesFilteringResponseEntry> Candidates { get; set; }
    }
}
