using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Arrba.Parser.Dto;
using Newtonsoft.Json;

namespace Arrba.Parser.Services
{
    public class ArrbaApiService
    {
#if DEBUG
        private const string host = "https://localhost:44306";
#else
        private const string host = "https://api.arrba.ru";
#endif

#region Plural
        public static async Task<IEnumerable<CategoryDto>> GetCategorisAsync()
        {
            return await GetAsync<IEnumerable<CategoryDto>>($"{host}/api/Category/all");
        }

        public static async Task<IEnumerable<BrandDto>> GetBrandsAsync()
        {
            return await GetAsync<IEnumerable<BrandDto>>($"{host}/api/brand/all");
        }

        public static async Task<IEnumerable<TypeDto>> GetTypesAsync()
        {
            return await GetAsync<IEnumerable<TypeDto>>($"{host}/api/ItemType/allWithCategory");
        }

        public static async Task<IEnumerable<CityDto>> GetCitiesAsync(int countryId = 1)
        {
            return await GetAsync<IEnumerable<CityDto>>($"{host}/api/City/{countryId}/all");
        }

        public static async Task<PropertyDto> GetPropertyAsync(string name)
        {
            return await GetAsync<PropertyDto>($"{host}/api/property/{name}/like");
        }

        public static async Task<IEnumerable<ModelDto>> GetModelsAsync()
        {
            return await GetAsync<IEnumerable<ModelDto>>($"{host}/api/model/all");
        }
#endregion

        public static async Task<BrandDto> GetBrandAsync(string name)
        {
            return await GetAsync<BrandDto>($"{host}/api/brand/name/{name}");
        }

        public static async Task<ModelDto> GetModelAsync(int brandId, string name)
        {
            return await GetAsync<ModelDto>($"{host}/api/model/brand/{brandId}/model/{name}");
        }

        public static async Task<TypeDto> GetTypeAsync(int categoryId, string name)
        {
            return await GetAsync<TypeDto>($"{host}/api/itemType/categoryId/{categoryId}/typeName/{name}");
        }

        public static async Task<CityDto> GetCityAsync(string name)
        {
            return await GetAsync<CityDto>($"{host}/api/city/name/{name}");
        }

        public static async Task<CityDto> GetCityByAliasAsync(string alias)
        {
            return await GetAsync<CityDto>($"{host}/api/city/{alias}");
        }

        public static async Task<CurrencyDto> GetCurrencyAsync(string name = "RUR")
        {
            var currencies = await GetAsync<IEnumerable<CurrencyDto>>($"{host}/api/currency/all");
            return currencies.SingleOrDefault(c => c.Name == name);
        }

        public static async Task<CategoryDto> GetCategoryAsync(string alias)
        {
            return await GetAsync<CategoryDto>($"{host}/api/category/{alias}");
        }

        public static async Task<DealershipDto> GetDealershipAsync(string name)
        {
            return await GetAsync<DealershipDto>($"{host}/api/dealership/{name}");
        }

        public static async Task<byte[]> GetImageBytesAsync(string src)
        {
            if (!string.IsNullOrEmpty(src))
            {
                using (var client = new HttpClient())
                {
                    return await client.GetByteArrayAsync(src);
                }
            }

            return new byte[] { };
        }

        public static async Task<VehicleDto> GetItemAsync(int externalId)
        {
            return await GetAsync<VehicleDto>($"{host}/api/vehicle/{externalId}");
        }

        public static async Task<bool> EditItemAsync(VehicleDto item)
        {
            throw new NotImplementedException();
        }

        public static async Task<bool> UploadImageAsync(byte[] bytes, string fileName, string uniqueFolderName, string token)
        {
            var url = $"{host}/api/upload/image/{uniqueFolderName}";
            using (Stream image = new MemoryStream(bytes))
            {
                HttpContent fileStreamContent = new StreamContent(image);
                fileStreamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") { Name = "file", FileName = fileName };
                fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg"); // application/octet-stream
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    using (var formData = new MultipartFormDataContent())
                    {
                        formData.Add(fileStreamContent);
                        var response = await client.PostAsync(url, formData);
                        return response.IsSuccessStatusCode;
                    }
                }
            }
        }

        public static async Task DeleteImagesAsync(string uniqueItemFolder)
        {
            var url = $"{host}/api/upload/{uniqueItemFolder}/delete";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                await client.DeleteAsync(url);
            }
        }

        public static async Task<int> AddItemAsync(VehicleDto item, string token)
        {
            var url = $"{host}/api/vehicle/add";
            return await PostAsync<int>(url, item, token); ;
        }

        // TODO make async
        public static int DeactualizeItem(int id, string jwtToken)
        {
            var url = $"{host}/api/vehicle/deactualize";
            using (var client = new WebClient())
            {
                client.Headers = new WebHeaderCollection
                {
                    { "Content-Type", "application/json" },
                    { "Authorization", $"Bearer {jwtToken}" }
                };

                var response = client.UploadString(url, id.ToString());

                return Convert.ToInt32(response);
            }
        }

        public static string GetToken(string userName, string password)
        {
            var url = $"{host}/api/token";
            using (var client = new WebClient())
            {
                client.Headers = new WebHeaderCollection
                {
                    { "Content-Type", "application/json" }
                };

                var response = client.UploadString(url, JsonConvert.SerializeObject(new
                {
                    Email = userName,
                    Password = password
                }));

                return response.Trim('"');
            }
        }

        public static async Task<string> GenereateUniqueItemFolderAsync(string token)
        {
            var url = $"{host}/api/upload/getGuid";
            var response = await GetAsync<UniqueItemFolderDto>(url, token);
            return response.UniqueItemFolder;
        }

        private static async Task<TDto> GetAsync<TDto>(string url, string token = null)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    if (!string.IsNullOrEmpty(token))
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }

                    var response = await client.GetStringAsync(url);
                    var result = JsonConvert.DeserializeObject<TDto>(response);

                    return result;
                }
            }
            catch (HttpRequestException ex)
            {
                const string statusCodePattern = @"[123450]{3}";
                if (Regex.IsMatch(ex.Message, statusCodePattern))
                {
                    var statusCode = Regex.Match(ex.Message, statusCodePattern).Value;
                    var httpStatusCode = (HttpStatusCode)Convert.ToInt32(statusCode);

                    throw new HttpWebException(httpStatusCode, ex.Message, ex);
                }

                throw new HttpWebException(HttpStatusCode.InternalServerError, ex.Message, ex);
            }
        }

        private static async Task<TDto> PostAsync<TDto>(string url, object item, string token = null)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    if (!string.IsNullOrEmpty(token))
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }
                    var itemJson = JsonConvert.SerializeObject(item);
                    var content = new StringContent(itemJson, Encoding.UTF8, "application/json");
                    var httpResponseMessage = await client.PostAsync(url, content);
                    var response = await httpResponseMessage.Content.ReadAsStringAsync();

                    if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
                    {
                        throw new NotImplementedException(response);
                    }

                    var result = JsonConvert.DeserializeObject<TDto>(response);

                    return result;
                }
            }
            catch (HttpRequestException ex)
            {
                const string statusCodePattern = @"[123450]{3}";
                if (Regex.IsMatch(ex.Message, statusCodePattern))
                {
                    var statusCode = Regex.Match(ex.Message, statusCodePattern).Value;
                    var httpStatusCode = (HttpStatusCode)Convert.ToInt32(statusCode);

                    throw new HttpWebException(httpStatusCode, ex.Message, ex);
                }

                throw new HttpWebException(HttpStatusCode.InternalServerError, ex.Message, ex);
            }
        }
    }
}
