using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Albelli.MiscUtils.Lib.ImageArchive
{
    [XmlType("pictures")]
    [XmlRoot("pictures")]
    public class PicturesCollection : List<PictureInfo>
    {
        public PicturesCollection()
        {
        }

        public PicturesCollection(IEnumerable<PictureInfo> pictures) : base(pictures)
        {
        }
    }
}
