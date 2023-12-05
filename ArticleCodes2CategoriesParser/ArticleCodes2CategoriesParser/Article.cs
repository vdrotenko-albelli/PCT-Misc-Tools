using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArticleCodes2CategoriesParser
{
    public partial class Article
    {
        // Foreign keys
        public virtual ArticleType ArticleType { get; set; } // FK_Articles_ArticleTypes
        public virtual PackagingMethod PackagingMethod { get; set; } // FK_Articles_PackagingMethods
    }
    public partial class PackagingMethod
    {
        public byte Id { get; set; } // ID (Primary key)
        public string Name { get; set; } // Name (length: 20)

    }

    public partial class ArticleType
    {
        [JsonProperty(PropertyName = "ArticleTypeId")]
        public int Id { get; set; } // ID (Primary key)
        public string Code { get; set; } // Code (length: 20)
        public string Name { get; set; } // Name (length: 50)
        public short LeadTime { get; set; } // LeadTime
        public string WorkingDays { get; set; } // WorkingDays (length: 7)
        public bool PickLabels { get; set; } // PickLabels
        public int? BinAreaId { get; set; } // BinAreaID
        public string HsCode { get; set; } // HSCode (length: 20)
        public bool FastLane { get; set; } // FastLane
        public bool IsCoverContentMatchRequired { get; set; } // IsCoverContentMatchRequired

    }
}
