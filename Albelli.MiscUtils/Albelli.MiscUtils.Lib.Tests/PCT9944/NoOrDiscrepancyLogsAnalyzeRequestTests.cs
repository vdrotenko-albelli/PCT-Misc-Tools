using Albelli.MiscUtils.Lib.PCT9944;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.Tests.PCT9944
{
    
    public class NoOrDiscrepancyLogsAnalyzeRequestTests
    {
        [Test]
        public void RequestJsonSerializationSample()
        {
            string[] envs = { "prod", "uat" };
            NoOrDiscrepancyLogsAnalyzeRequest req = new NoOrDiscrepancyLogsAnalyzeRequest();
            req.MatrixZonesByPlantPerEnv = new();
            foreach (var env in envs)
            {
                req.MatrixZonesByPlantPerEnv.Add(env, new MatrixZonesPerEnvConfig());
                req.MatrixZonesByPlantPerEnv[env].MatrixZonesPerPlant = new();
                req.MatrixZonesByPlantPerEnv[env].MatrixZonesPerPlant.Add("YPB", new MatrixZoneConfig()
                {
                    MatrixXlsPath = $"matrix_nl_{env}.xls",
                    ZonesXlsPath = $"zones_nl_{env}.xls"
                }
                );
                req.MatrixZonesByPlantPerEnv[env].MatrixZonesPerPlant.Add("WFR", new MatrixZoneConfig()
                {
                    MatrixXlsPath = $"matrix_wfr_{env}.xls",
                    ZonesXlsPath = $"zones_wfr_{env}.xls"
                }
                );
            }

            TestContext.Out.WriteLine(JsonConvert.SerializeObject( req, Formatting.Indented ));

        }
    }
}
