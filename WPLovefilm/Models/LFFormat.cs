using System.Xml.Serialization;

namespace WPLovefilm.Models
{
    public class LFFormat
    {
        public LFFormat() { }

        public LFFormat(string name, bool active, LFFormatType type)
        {
            Name = name;
            Active = active;
            Type = type;
        }

        [XmlAttribute()]
        public string Name { get; set; }

        [XmlAttribute()]
        public bool Active { get; set; }

        [XmlAttribute()]
        public LFFormatType Type { get; set; }
    }
}
