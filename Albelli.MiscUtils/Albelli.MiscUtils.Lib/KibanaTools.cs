using Albelli.MiscUtils.Lib.Kibana;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public static string FormatAsOSQueryDSL(Dictionary<string, List<string>> expressions)
        {
            Kibana.DSLQuery dslQu = new();
            string rslt = JsonConvert.SerializeObject(dslQu, Formatting.Indented);
            StringBuilder sb = new();
            int currFldIdx = 0;
            foreach (string fldName in expressions.Keys)
            {
                if (false == expressions[fldName]?.Count > 0)
                    continue;
                List<Kibana.OSShouldDef> currShoulds = new();
                foreach (string value in expressions[fldName])
                {
                    currShoulds.Add(new Kibana.OSShouldDef() { match_phrase = new Kibana.OSMatchPhraseDef() { AVeryUniquePropertyName_7DDC54B3_0CB6_44CF_89A8_86E384E6D766 = value } });
                }
                if (currFldIdx > 0) sb.Append(",\r\n");

                var currJson = JsonConvert.SerializeObject(currShoulds, Formatting.None).Replace($"{DSLQuery.FieldNamePlaceholder}", $"{fldName}");
                sb.Append(currJson.Substring(1,currJson.Length-2));
                currFldIdx++;
            }
            rslt = rslt.Replace("\"should\": []", string.Format("\"should\": [{0}]", nameof(OSMatchPhraseDef.AVeryUniquePropertyName_7DDC54B3_0CB6_44CF_89A8_86E384E6D766)));
            rslt = rslt.Replace(nameof(OSMatchPhraseDef.AVeryUniquePropertyName_7DDC54B3_0CB6_44CF_89A8_86E384E6D766), sb.ToString());
            //Console.WriteLine(rslt);
            //Console.WriteLine(new string('-',33));
            dynamic rsltObj = JsonConvert.DeserializeObject(rslt);
            return JsonConvert.SerializeObject (rsltObj, Formatting.Indented);

        }

    }
}
