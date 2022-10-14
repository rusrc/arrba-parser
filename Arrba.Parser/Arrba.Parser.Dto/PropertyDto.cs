using System.Collections.Generic;

namespace Arrba.Parser.Dto
{
    public class PropertyDto
    {
        public long PropertyID { get; set; }
        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }
        public string Description { get; set; }
        public string UnitMeasure { get; set; }
        public string PropertyType { get; set; }
        public IEnumerable<SelectOptionDto> SelectOptions { get; set; }
    }
}
