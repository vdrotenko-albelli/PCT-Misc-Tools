using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.CodeMetrics
{
    public class ProjectQualityMetrics
    {
        public string Sln { get; set; }
        public string Proj { get; set; }
        public int MaintainabilityIndex { get; set; }
        public int CyclomaticComplexity { get; set; }
        public int ClassCoupling { get; set; }
        public int DepthOfInheritance { get; set; }
        public int SourceLines { get; set; }
        public int ExecutableLines { get; set; }
    }
}
