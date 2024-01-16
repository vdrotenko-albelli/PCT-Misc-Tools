using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.PCT9944
{
    public class NoOrDiscrepancyLogsAnalyzeRequest
    {
        public string LogsDir { get; set; }
        public List<string> Logs { get; set; }
        public string OutputTablePath { get; set; }
        public bool PrintPayloadJson { get; set; }
        public string[] FilterByPlants { get; set; }
        public Dictionary<string, MatrixZonesPerEnvConfig> MatrixZonesByPlantPerEnv { get; set; }
        public List<string> AllPlantCodes { get; set; }
    }
    public class MatrixZonesPerEnvConfig
    {
        public Dictionary<string,MatrixZoneConfig> MatrixZonesPerPlant { get; set; }
    }

    public class MatrixZoneConfig
    {
        public string MatrixXlsPath { get; set; }
        public string ZonesXlsPath { get; set; }

    }
}
