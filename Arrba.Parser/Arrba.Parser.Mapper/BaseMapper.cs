using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Arrba.Parser.Dto;
using Arrba.Parser.Exceptions;
using Arrba.Parser.Services;

namespace Arrba.Parser.Mapper
{
    public class BaseMapper: IMapper<VehicleDto, VehicleRawDto>
    {
        protected readonly Dictionary<string, string> NormalizedTypes = new Dictionary<string, string>();
        protected readonly Dictionary<string, string> NormalizedCategories = new Dictionary<string, string>();
        protected readonly Dictionary<string, string> NormalizedBrands = new Dictionary<string, string>();
        protected readonly Dictionary<string, string> NormalizedColors = new Dictionary<string, string>();

        public BaseMapper()
        {
            NormalizedTypes.Add("ЛЕГКОВОЙ ХЭТЧБЕК", "automobile");
            NormalizedTypes.Add("Хэтчбек", "automobile");
            NormalizedTypes.Add("Седан", "automobile");
            NormalizedTypes.Add("Универсал", "automobile");
            NormalizedTypes.Add("Кабриолет", "automobile");
            NormalizedTypes.Add("Родстер", "automobile");
            NormalizedTypes.Add("Лифтбек", "automobile");
            NormalizedTypes.Add("Тарта", "automobile");
            NormalizedTypes.Add("Автобус", "Minivans and minibuses");
            NormalizedTypes.Add("Фургон", "Minivans and minibuses");
            NormalizedTypes.Add("Минивэн", "Minivans and minibuses");
            NormalizedTypes.Add("Кроссовер", "SUVs and pickups");
            NormalizedTypes.Add("Внедорожник", "SUVs and pickups");
            NormalizedTypes.Add("Пикап", "SUVs and pickups");
            NormalizedTypes.Add("Шасси", "SUVs and pickups");

            NormalizedCategories.Add("ЛЕГКОВОЙ ХЭТЧБЕК", "legkovye");
            NormalizedCategories.Add("Хэтчбек", "legkovye");
            NormalizedCategories.Add("Седан", "legkovye");
            NormalizedCategories.Add("Универсал", "legkovye");
            NormalizedCategories.Add("Кабриолет", "legkovye");
            NormalizedCategories.Add("Родстер", "legkovye");
            NormalizedCategories.Add("Лифтбек", "legkovye");
            NormalizedCategories.Add("Тарта", "legkovye");
            NormalizedCategories.Add("Автобус", "legkovye");
            NormalizedCategories.Add("Фургон", "legkovye");
            NormalizedCategories.Add("Минивэн", "legkovye");
            NormalizedCategories.Add("Кроссовер", "legkovye");
            NormalizedCategories.Add("Внедорожник", "legkovye");
            NormalizedCategories.Add("Пикап", "legkovye");
            NormalizedCategories.Add("Шасси", "legkovye");

            NormalizedBrands.Add("Škoda", "Skoda");
        }

        public virtual async Task<VehicleDto> MapAsync(VehicleRawDto source)
        {
            var category = await GetCategoryByName(source.CategoryName, categoryName => GetNormalizedValue(categoryName, NormalizedCategories));
            var brand = await GetBrandByName(source.BrandName, brandName => GetNormalizedValue(brandName, NormalizedBrands));
            var type = await GetTypeByName(source.TypeName, category.ID, typeName => GetNormalizedValue(typeName, NormalizedTypes));
            var model = await GetModelByName(source.ModelName, brand.ID);
            var city = await GetCityByName(source.CityName);
            var currency = await ArrbaApiService.GetCurrencyAsync(source.CurrencyName);

            return new VehicleDto
            {
                SuperCategoryId = category.SuperCategID,
                CategoryId = category.ID,
                BrandId = brand.ID,
                ModelId = model?.ID ?? null,
                TypeId = type.ID,
                CountryId = city.CountryId,
                CityId = city.Id,

                Price = source.Price,
                MinimalPrice = source.MinimalPrice,
                Year = source.Year,
                CurrencyId = currency.ID,
                ImageSrcs = source.ImageSrcs,
                AdditionalComment = source.Comment,
                Description = source.Description,
                //MapJsonCoord = GetGoogleCoords("Санкт-Петербург")
                DealershipName = source.DealershipName,
                DealershipAddress = source.DealershipAddress,
                DealershipNumberPhone = source.DealershipNumberPhone,
            };
        }

        #region Private helpers
        protected async Task<TypeDto> GetTypeByName(string name, int categoryId, Func<string, string> tryToNormolize = null)
        {
            try
            {
                return await ArrbaApiService.GetTypeAsync(categoryId, name);
            }
            catch (HttpWebException ex)
            {
                if (ex.HttpStatusCode == HttpStatusCode.NotFound)
                {
                    if (tryToNormolize != null)
                    {
                        return await GetTypeByName(tryToNormolize(name), categoryId);
                    }
                    throw new NotFoundException(GetType().Name, $"can't get type by typeName: {name}", ex);
                }

                throw new ProviderException(GetType().Name, ex);
            }
        }

        protected async Task<BrandDto> GetBrandByName(string name, Func<string, string> tryToNormolize = null)
        {
            try
            {
                return await ArrbaApiService.GetBrandAsync(name);
            }
            catch (HttpWebException ex)
            {
                if (ex.HttpStatusCode == HttpStatusCode.NotFound)
                {
                    if (tryToNormolize != null)
                    {
                        return await GetBrandByName(tryToNormolize(name));
                    }
                    throw new NotFoundException(GetType().Name, $"can't get brand by brandName: {name}", ex);
                }

                throw new ProviderException(GetType().Name, ex);
            }
        }

        protected async Task<ModelDto> GetModelByName(string modelName, int brandId, Func<string, string> tryToNormolize = null)
        {
            try
            {
                return await ArrbaApiService.GetModelAsync(brandId, modelName);
            }
            catch (HttpWebException ex)
            {
                if (ex.HttpStatusCode == HttpStatusCode.NotFound)
                {
                    if (tryToNormolize != null)
                    {
                        return await GetModelByName(tryToNormolize(modelName), brandId);
                    }
                    // Model could be Nullable
                    return null;
                }

                throw new ProviderException(GetType().Name, ex);
            }
        }

        protected async Task<CityDto> GetCityByName(string cityName)
        {
            try
            {
                return await ArrbaApiService.GetCityAsync(cityName);
            }
            catch (HttpWebException ex)
            {
                if (ex.HttpStatusCode == HttpStatusCode.NotFound)
                {
                    throw new NotFoundException(GetType().Name, $"can't get city by cityName: {cityName}", ex);
                }

                throw new ProviderException(GetType().Name, ex);
            }
        }

        protected async Task<CityDto> GetCityByAlias(string cityAlias)
        {
            try
            {
                return await ArrbaApiService.GetCityByAliasAsync(cityAlias);
            }
            catch (HttpWebException ex)
            {
                if (ex.HttpStatusCode == HttpStatusCode.NotFound)
                {
                    throw new NotFoundException(GetType().Name, $"can't get city by cityAlias: {cityAlias}", ex);
                }

                throw new ProviderException(GetType().Name, ex);
            }
        }

        protected async Task<CategoryDto> GetCategoryByName(string alias, Func<string, string> tryToNormolize = null)
        {
            try
            {
                return await ArrbaApiService.GetCategoryAsync(alias);
            }
            catch (HttpWebException ex)
            {
                if (ex.HttpStatusCode == HttpStatusCode.NotFound)
                {
                    if (tryToNormolize != null)
                    {
                        return await GetCategoryByName(tryToNormolize(alias));
                    }
                    throw new NotFoundException(GetType().Name, $"can't get category by category alias: {alias}", ex);
                }

                throw new ProviderException(GetType().Name, ex);
            }
        }

        protected async Task<PropertyDto> GetColorByName(string name, Func<string, string> tryToNormolize = null)
        {
            try
            {
                var property = await ArrbaApiService.GetPropertyAsync("Цвет");
                var option = property
                    .SelectOptions
                    .FirstOrDefault(o => Regex.IsMatch(name, o.Name, RegexOptions.IgnoreCase | RegexOptions.Multiline));

                if (option == null && tryToNormolize != null)
                {
                    return await GetColorByName(tryToNormolize(name));
                }

                property.PropertyValue = option?.ID.ToString();

                return property;

            }
            catch (HttpWebException ex)
            {
                if (ex.HttpStatusCode == HttpStatusCode.NotFound)
                {
                    if (tryToNormolize != null)
                    {
                        return await GetColorByName(tryToNormolize(name));
                    }

                    // could be Nullable
                    return null;
                }

                throw new ProviderException(GetType().Name, ex);
            }
        }

        // TODO Test method
        protected virtual string GetNormalizedValue(string originalName, Dictionary<string, string> normolizedItems)
        {
            if (string.IsNullOrEmpty(originalName))
            {
                throw new NotFoundException(GetType().Name, $"Value from site: '{originalName}' is null or empty");
            }

            // TODO Test method to test the line
            var result = normolizedItems.SingleOrDefault(d => d.Key.Equals(originalName.Trim(), StringComparison.CurrentCultureIgnoreCase));

            if (string.IsNullOrEmpty(originalName) is false && string.IsNullOrEmpty(result.Value))
            {
                throw new NormalizedValueException($"originalName: {originalName}, result: {result}");
            }

            return result.Value ?? originalName;
        }

        #endregion
    }
}
