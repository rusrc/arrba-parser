using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Arrba.Parser.Dto
{
    public class VehicleDto
    {
        public enum ItemCondition
        {
            New = 1,
            Used = 2,
            Crashed = 3
            //Refobished
        }

        [Required]
        [JsonProperty(PropertyName = "superCategoryId")]
        public long SuperCategoryId { get; set; }
        [Required]
        [JsonProperty(PropertyName = "categoryId")]
        public long CategoryId { get; set; }

        [Required]
        [JsonProperty(PropertyName = "TypeId")]
        public long TypeId { get; set; }

        [Required]
        [JsonProperty(PropertyName = "BrandId")]
        public long BrandId { get; set; }

        [JsonProperty(PropertyName = "ModelId")]
        public long? ModelId { get; set; }

        [JsonProperty(PropertyName = "modelValue")]
        public string ModelValue { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [Required]
        [JsonProperty(PropertyName = "CurrencyId")]
        public long CurrencyId { get; set; }

        [Required]
        [JsonProperty(PropertyName = "CountryId")]
        public long CountryId { get; set; }

        [Required]
        [JsonProperty(PropertyName = "CityId")]
        public long CityId { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "additionalComment")]
        public string AdditionalComment { get; set; }

        [JsonProperty(PropertyName = "commentMode")]
        public string CommentMode { get; set; }

        [Required]
        [JsonProperty(PropertyName = "price")]
        public double Price { get; set; }

        [Required]
        [JsonProperty(PropertyName = "minimalPrice")]
        public double? MinimalPrice { get; set; }

        [Required]
        [JsonProperty(PropertyName = "year")]
        public string Year { get; set; }

        [JsonProperty(PropertyName = "phoneNumbers")]
        public string[] PhoneNumbers { get; set; }

        [JsonProperty(PropertyName = "temporaryImageFolder")]
        public string TemporaryImageFolder { get; set; }

        [JsonProperty(PropertyName = "dealershipId")]
        public long? DealershipId { get; set; }

        [JsonProperty(PropertyName = "mapJsonCoord")]
        public string MapJsonCoord { get; set; }

        [JsonIgnore]
        public string DealershipName { get; set; }
        [JsonIgnore]
        public string DealershipAddress { get; set; }
        [JsonIgnore]
        public string DealershipNumberPhone { get; set; }
        [JsonIgnore]
        public string[] ImageSrcs { get; set; }

        [JsonProperty(PropertyName = "properties")]
        public Dictionary<string, object> Properties { get; set; }

        public ItemCondition Condition { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as VehicleDto;

            if (other == null)
            {
                return false;
            }

            if (Math.Abs(Price - other.Price) > 0)
            {
                return false;
            }

            if (MinimalPrice != other.MinimalPrice)
            {
                return false;
            }

            return true;
        }
    }
}
