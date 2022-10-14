using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arrba.Parser.Dto;
using Arrba.Parser.Logger;
using Arrba.Parser.Services;

namespace Arrba.Parser.Mapper
{
    public class DictionaryMapper : BaseMapper
    {
        private readonly Lazy<Task<DataDictionaries>> _lazyDataDictionaries;

        public DictionaryMapper(ILogService logService)
        {
            _lazyDataDictionaries = new Lazy<Task<DataDictionaries>>(async () =>
            {
                var dataDictionaries = new DataDictionaries();
                await dataDictionaries.SeedAsync();

                logService.Info($"The data of '{nameof(DataDictionaries)}' has been seeded. TreadName: {Thread.CurrentThread.Name}");
                return dataDictionaries;
            });
        }

        public override async Task<VehicleDto> MapAsync(VehicleRawDto source)
        {
            var data = await _lazyDataDictionaries.Value;

            var categoryId = GetCategoryId(source.CategoryName, data, categoryName => GetNormalizedValue(categoryName, NormalizedCategories));
            var superCategoryId = GetSuperCategoryId(source.CategoryName, data, categoryName => GetNormalizedValue(categoryName, NormalizedCategories));
            var typeId = GetTypeId(source.TypeName, categoryId, data, typeName => GetNormalizedValue(typeName, NormalizedTypes));
            var brandId = GetBrandId(source.BrandName, data, brandName => GetNormalizedValue(brandName, NormalizedBrands));
            var modelId = GetModelId(source.ModelName, brandId, data);
            var cityId = GetCityId(source.CityName, data);
            var countryId = 1; // TODO hard coded
            var currencyId = 4; // TODO hard coded

            return new VehicleDto
            {
                SuperCategoryId = superCategoryId,
                CategoryId = categoryId,
                BrandId = brandId,
                ModelId = modelId,
                TypeId = typeId,
                CountryId = countryId,
                CityId = cityId,

                Price = source.Price,
                MinimalPrice = source.MinimalPrice,
                Year = source.Year,
                CurrencyId = currencyId,
                ImageSrcs = source.ImageSrcs,
                AdditionalComment = source.Comment,
                Description = source.Description,
                //MapJsonCoord = GetGoogleCoords("Санкт-Петербург")
                DealershipName = source.DealershipName,
                DealershipAddress = source.DealershipAddress,
                DealershipNumberPhone = source.DealershipNumberPhone,
                Condition = (VehicleDto.ItemCondition)source.Condition
            };
        }

        private int GetCategoryId(string name, DataDictionaries dictionary, Func<string, string> tryToNormolize = null)
        {
            if (TryGetValue(name, dictionary.CategoryAlias_CategoryId, tryToNormolize, out int result))
            {
                return result;
            }

            throw new KeyNotFoundException($"Key '{name}' not found");
        }

        private int GetSuperCategoryId(string name, DataDictionaries dictionary, Func<string, string> tryToNormolize = null)
        {
            if (TryGetValue(name, dictionary.CategoryAlias_SuperCategoryId, tryToNormolize, out int result))
            {
                return result;
            }

            throw new KeyNotFoundException($"Key '{name}' not found");
        }

        private int GetBrandId(string name, DataDictionaries dictionary, Func<string, string> tryToNormolize = null)
        {
            if (TryGetValue(name, dictionary.BrandName_BrandId, tryToNormolize, out int result))
            {
                return result;
            }

            throw new KeyNotFoundException($"Key '{name}' not found");
        }

        private int GetCityId(string name, DataDictionaries dictionary, Func<string, string> tryToNormolize = null)
        {
            if (TryGetValue(name, dictionary.CityName_CityId, tryToNormolize, out int result))
            {
                return result;
            }

            throw new KeyNotFoundException($"Key '{name}' not found");
        }

        private int? GetModelId(string name, int brandId, DataDictionaries dictionary)
        {
            var key = $"{name}:{brandId}";

            if (TryGetValue(key, dictionary.ModelNameBrandId_ModelId, null, out int result))
            {
                return result;
            }

            return null;
        }

        private int GetTypeId(string name, int categoryId, DataDictionaries dictionary, Func<string, string> tryToNormolize = null)
        {
            var key = $"{name}:{categoryId}";

            if (TryGetValue(key, dictionary.TypeNameCategoryId_TypeId, tryToNormolize, out int result))
            {
                return result;
            }

            throw new KeyNotFoundException($"Key '{key}' not found");
        }

        private bool TryGetValue(string key, IReadOnlyDictionary<string, int> dictionary, Func<string, string> tryToNormolize, out int result)
        {
            result = 0;

            // 1. Try get as is
            if (dictionary.TryGetValue(key, out result))
                return true;

            // 2. Try get approximately or matched value
            if ((result = dictionary.SingleOrDefault(d => key.Contains(d.Key)).Value) > 0)
                return true;

            // 3. Try to normolize
            key = tryToNormolize?.Invoke(key);
            return key != null && dictionary.TryGetValue(key, out result);
        }
    }
}
