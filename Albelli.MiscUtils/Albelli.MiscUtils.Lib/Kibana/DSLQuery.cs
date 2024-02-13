using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Albelli.MiscUtils.Lib.Kibana
{
    public class DSLQuery
    {
        public const string FieldNamePlaceholder = nameof(OSMatchPhraseDef.AVeryUniquePropertyName_7DDC54B3_0CB6_44CF_89A8_86E384E6D766);
        public OSQueryDef query { get; set; }

        public DSLQuery() { query = new(); }
    }

    public class OSQueryDef
    {
        public OSQueryDef()
        {
            BoolDef = new();
        }
        [Newtonsoft.Json.JsonProperty(PropertyName = "bool")]
        public OSBoolDef BoolDef { get; set; }
    }

    public class OSBoolDef
    {
        public OSBoolDef()
        {
            should = new();
            minimum_should_match = 1;
        }
        public List<OSShouldDef> should { get; set; }
        public int minimum_should_match { get; set; }
    }

    public class OSShouldDef
    {
        public OSMatchPhraseDef match_phrase { get; set; }
    }
    public class OSMatchPhraseDef
    {
        public string AVeryUniquePropertyName_7DDC54B3_0CB6_44CF_89A8_86E384E6D766 { get; set; }
    }
}
