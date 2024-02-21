using Albelli.MiscUtils.Lib.Kibana;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib
{
    public static class KibanaTools
    {
        public static string FormatAsOSQueryDSL(string fieldName, IEnumerable<string> values)
        {
            Kibana.DSLQuery dslQu = new();
            foreach (string value in values)
            {
                dslQu.query.BoolDef.should.Add(new Kibana.OSShouldDef { match_phrase = new Kibana.OSMatchPhraseDef() { AVeryUniquePropertyName_7DDC54B3_0CB6_44CF_89A8_86E384E6D766 = value } });
            }

            string rslt = JsonConvert.SerializeObject(dslQu, Formatting.Indented);
            return rslt.Replace($"{DSLQuery.FieldNamePlaceholder}", $"{fieldName}");
        }
    }
}
