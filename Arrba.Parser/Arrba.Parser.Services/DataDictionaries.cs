using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arrba.Parser.Dto;

namespace Arrba.Parser.Services
{
    /// <summary>
    /// Used to reduce calls to server
    /// </summary>
    public class DataDictionaries
    {
        public Dictionary<string, int> CategoryAlias_CategoryId { get; set; }
        public Dictionary<string, int> CategoryAlias_SuperCategoryId { get; set; }
        public Dictionary<string, int> BrandName_BrandId { get; set; }
        public Dictionary<string, int> ModelNameBrandId_ModelId { get; set; }
        public Dictionary<string, int> TypeNameCategoryId_TypeId { get; set; }
        public Dictionary<string, int> CityName_CityId { get; set; }

        public async Task<DataDictionaries> SeedAsync()
        {
            var comparer = StringComparer.InvariantCultureIgnoreCase;

            if (CategoryAlias_CategoryId == null || CategoryAlias_SuperCategoryId == null)
            {
                var categories = await ArrbaApiService.GetCategorisAsync();
                CategoryAlias_CategoryId
                    = new Dictionary<string, int>(categories.ToDictionary(c => c.Alias, c => c.ID), comparer);

                CategoryAlias_SuperCategoryId
                    = new Dictionary<string, int>(categories.ToDictionary(c => c.Alias, c => c.SuperCategID), comparer);
            }

            if (BrandName_BrandId == null)
            {
                var brands = await ArrbaApiService.GetBrandsAsync();
                BrandName_BrandId
                    = new Dictionary<string, int>(brands.ToDictionary(b => b.Name, b => b.ID), comparer);
            }

            if (TypeNameCategoryId_TypeId == null)
            {
                var typeCategories = await ArrbaApiService.GetTypesAsync();
                TypeNameCategoryId_TypeId
                    = new Dictionary<string, int>
                        (typeCategories.ToDictionary(tc => $"{tc.TypeName}:{tc.CategoryID}", tc => tc.ID), comparer);
            }

            if (CityName_CityId == null)
            {
                var cities = await ArrbaApiService.GetCitiesAsync();
                CityName_CityId
                    = new Dictionary<string, int>(cities.ToDictionary(c => c.Name, c => c.Id), comparer);
            }

            if (ModelNameBrandId_ModelId == null)
            {
                var models = await ArrbaApiService.GetModelsAsync();
                ModelNameBrandId_ModelId
                    = new Dictionary<string, int>(models.ToDictionary(m => $"{m.Name}:{m.BrandID}", m => m.ID), comparer);
            }

            return this;
        }
    }
}
