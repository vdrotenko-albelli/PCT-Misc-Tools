using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Albelli.MiscUtils.Lib.ImageArchive
{
    [XmlType("picture")]
    [XmlRoot("picture")]
    public class PictureInfo
    {
        [XmlElement("name")]
        public string name { get; set; }
        [XmlElement("type")]
        public string type { get; set; }
        [XmlElement("filename")]
        public string filename { get; set; }
        [XmlElement("quantity")]
        public int quantity { get; set; }
        [XmlElement("indexPosition")]
        public int indexPosition { get; set; }
        [XmlElement("backprint")]
        public string backprint { get; set; }
        [XmlElement("takenDate")]
        public string takenDate { get; set; }
        
    }
}
