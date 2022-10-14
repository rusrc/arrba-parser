using System.Collections.Generic;

namespace Arrba.Parser.Dto
{
    public class VehicleRawDto
    {
        public string DealershipName { get; set; }
        public string DealershipAddress { get; set; }
        public string DealershipNumberPhone { get; set; }
        public string CategoryName { get; set; }
        public string BrandName { get; set; }
        public string TypeName { get; set; }
        public string ModelName { get; set; }
        public string CityName { get; set; }
        public double Price { get; set; }
        public double? MinimalPrice { get; set; }
        public string Year { get; set; }
        public string CurrencyName { get; set; }
        public string Comment { get; set; }
        public string Description { get; set; }
        public string[] ImageSrcs { get; set; }
        public int Condition { get; set; }
        public string[] Properties { get; set; }
    }
}
