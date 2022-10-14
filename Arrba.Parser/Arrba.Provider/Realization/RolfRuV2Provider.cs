using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Arrba.Parser.Dto;
using Arrba.Parser.Exceptions;
using Arrba.Parser.Provider.Attributes;
using Arrba.Parser.Services;
using Newtonsoft.Json;

namespace Arrba.Parser.Provider.Realization
{
    [ProviderDescription(Host = "https://www.rolf.ru")]
    public class RolfRuV2Provider : BaseSiteProvider, ISiteProvider
    {
        private readonly Dictionary<string, string> _normalizedTypes = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _normalizedCategories = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _normalizedBrands = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _normalizedColors = new Dictionary<string, string>();

        public RolfRuV2Provider()
        {
            _normalizedTypes.Add("ЛЕГКОВОЙ ХЭТЧБЕК", "automobile");
            _normalizedTypes.Add("Хэтчбек", "automobile");
            _normalizedTypes.Add("Седан", "automobile");
            _normalizedTypes.Add("Универсал", "automobile");
            _normalizedTypes.Add("Кабриолет", "automobile");
            _normalizedTypes.Add("Родстер", "automobile");
            _normalizedTypes.Add("Лифтбек", "automobile");
            _normalizedTypes.Add("Тарта", "automobile");
            _normalizedTypes.Add("Автобус", "Minivans and minibuses");
            _normalizedTypes.Add("Фургон", "Minivans and minibuses");
            _normalizedTypes.Add("Минивэн", "Minivans and minibuses");
            _normalizedTypes.Add("Кроссовер", "SUVs and pickups");
            _normalizedTypes.Add("Внедорожник", "SUVs and pickups");
            _normalizedTypes.Add("Пикап", "SUVs and pickups");
            _normalizedTypes.Add("Шасси", "SUVs and pickups");

            _normalizedCategories.Add("ЛЕГКОВОЙ ХЭТЧБЕК", "legkovye-avtomobili");
            _normalizedCategories.Add("Хэтчбек", "legkovye-avtomobili");
            _normalizedCategories.Add("Седан", "legkovye-avtomobili");
            _normalizedCategories.Add("Универсал", "legkovye-avtomobili");
            _normalizedCategories.Add("Кабриолет", "legkovye-avtomobili");
            _normalizedCategories.Add("Родстер", "legkovye-avtomobili");
            _normalizedCategories.Add("Лифтбек", "legkovye-avtomobili");
            _normalizedCategories.Add("Тарта", "legkovye-avtomobili");
            _normalizedCategories.Add("Автобус", "legkovye-avtomobili");
            _normalizedCategories.Add("Фургон", "legkovye-avtomobili");
            _normalizedCategories.Add("Минивэн", "legkovye-avtomobili");
            _normalizedCategories.Add("Кроссовер", "legkovye-avtomobili");
            _normalizedCategories.Add("Внедорожник", "legkovye-avtomobili");
            _normalizedCategories.Add("Пикап", "legkovye-avtomobili");
            _normalizedCategories.Add("Шасси", "legkovye-avtomobili");

            //_normalizedCategories.Add("test2", "Motocikly");
            //_normalizedCategories.Add("test3", "Mopedy-i-skutery");

            _normalizedBrands.Add("ŠKODA", "Skoda");

            _normalizedColors.Add("Слоновая кость с крышей коричневый каштан", "белый");
            _normalizedColors.Add("Белый с черной крышей", "белый");
            _normalizedColors.Add("Белый лед", "белый");
            _normalizedColors.Add("Коричневый с черной крышей", "коричневый");
            _normalizedColors.Add("Коричневый с крышей цвета слоновой кости", "коричневый");
            _normalizedColors.Add("Темный каштан", "коричневый");
            _normalizedColors.Add("Красный с черной крышей", "красный");
            _normalizedColors.Add("Красный", "красный");
            _normalizedColors.Add("Красный с крышей цвета слоновой кости", "красный");
            _normalizedColors.Add("Оранжевый с черной крышей", "оранжевый");
            _normalizedColors.Add("Оранжевый с крышей цвета слоновой кости", "оранжевый");
            _normalizedColors.Add("Оранжевая Аризона", "оранжевый");
            _normalizedColors.Add("ТЕМНО-СЕРЫЙ", "серый");
            _normalizedColors.Add("Темно-серый с черной крышей", "серый");
            _normalizedColors.Add("Серая платина", "серый");
            _normalizedColors.Add("Серый с черной крышей", "черный");
            _normalizedColors.Add("Черная жемчужина", "черный");
            _normalizedColors.Add("Черный с крышей цвета слоновой кости", "черный");
            _normalizedColors.Add("Crystal White", "белый");
            _normalizedColors.Add("CRYSTAL WHITE", "белый");
            _normalizedColors.Add("DEEP IMPACT BLUE", "синий");
            _normalizedColors.Add("Fiery Red", "красный");
            _normalizedColors.Add("FIERY RED", "красный");
            _normalizedColors.Add("FROZEN WHITE", "белый");
            _normalizedColors.Add("Ice Wine", "красный");
            _normalizedColors.Add("LUNAR SKY", "синий");
            _normalizedColors.Add("MAGNETIC", "серый");
            _normalizedColors.Add("MAGNETIC GREY", "серый");
            _normalizedColors.Add("MARINA BLUE", "синий");
            _normalizedColors.Add("MOONDUST SILVER", "серый");
            _normalizedColors.Add("PHANTOM BLACK", "черный");
            _normalizedColors.Add("RACE RED", "красный");
            _normalizedColors.Add("SHADOW BLACK", "черный");
            _normalizedColors.Add("SIENA BROWN", "коричневый");
            _normalizedColors.Add("Sleek Silver", "серый");
            _normalizedColors.Add("Sunset Orange", "оранжевый");
            _normalizedColors.Add("SUNSET ORANGE", "оранжевый");
            _normalizedColors.Add("URBAN GRAY", "серый");
            _normalizedColors.Add("Orange Fusion", "оранжевый");
            _normalizedColors.Add("Blue Flame", "синий");
            _normalizedColors.Add("Copper Stone", "коричневый");
            _normalizedColors.Add("SANTORINI BLACK", "Черный");
            _normalizedColors.Add("Rossello Red", "красный");
            _normalizedColors.Add("Carpathian Grey", "серый");
        }

        public VehicleDto GetItem(string link)
        {
            // var url = $@"{GetRootHost()}/cars/new/mitsubishi/outlander_iii/stock_car35547/";
            var url = GetRootHost() + link;
            var html = Get(url);
            var htmlParser = new HtmlParser();
            var doc = htmlParser.ParseDocument(html);

            var dealershipName = GetAddress(doc);
            var dealershipAddress = GetAddress(doc);
            var dealershipPhoneNumber = this.GetDealershipNumberPhone(doc);
            var category = this.GetCategoryByName(GetCategoryName(doc), categoryName => GetNormalizedValue(categoryName, _normalizedCategories));
            var brand = this.GetBrandByName(GetBrandName(doc), brandName => GetNormalizedValue(brandName, _normalizedBrands));
            var type = this.GetTypeByName(GetTypeName(doc), category.ID, typeName => GetNormalizedValue(typeName, _normalizedTypes));
            var model = this.GetModelByName(GetModelName(doc), brand.ID);
            var city = this.GetCityByNameOrAlias(GetCityName(doc));
            var price = this.GetPrice(doc);
            var minimalPrice = this.GetMinimalPrice(doc);
            var year = this.GetYear(doc);
            var currency = ArrbaApiService.GetCurrency();
            var imagesSrcs = new[] { GetImageSrc(doc) };
            var comment = this.GetComment(doc);


            // Add additional properties
            //var properties = new Dictionary<string, object>();

            return new VehicleDto
            {
                SuperCategoryId = category.SuperCategID,
                CategoryId = category.ID,
                BrandId = brand.ID,
                ModelId = model?.ID,
                TypeId = type.ID,
                CountryId = city.CountryId,
                CityId = city.Id,
                Price = price,
                MinimalPrice = minimalPrice,
                Year = year,
                CurrencyId = currency.ID,
                ImageSrcs = imagesSrcs,
                AdditionalComment = comment,
                //MapJsonCoord = GetGoogleCoords("Санкт-Петербург")
                DealershipName = dealershipName,
                DealershipAddress = dealershipAddress,
                DealershipNumberPhone = dealershipPhoneNumber,
                // Properties = properties
            };
        }

        private CityDto GetCityByNameOrAlias(string cityNameOrAlias)
        {
            CityDto city = null;

            try
            {
                city = this.GetCityByName(cityNameOrAlias);
            }
            catch (NotFoundException ex)
            {
                city = this.GetCityByAlias(cityNameOrAlias);
            }
            return city;
        }

        public IEnumerable<string> GetLinks()
        {
            var url = $@"{GetRootHost()}/ajax/stock/load_more/";
            var links = new List<string>();
            var pageNumber = 1;
            var isNextPage = false;

            do
            {
                var response = Post<NextPageResponse>(url, pageNumber);
                var htmlParser = new HtmlParser();
                var document = htmlParser.ParseDocument(response?.list);
                var selector = ".car-list__item";
                var cells = document.QuerySelectorAll(selector);
                var linksFromHtml = cells
                    .Select(m => m.GetAttribute("href"))
                    .ToList();

                links.AddRange(linksFromHtml);

                isNextPage = response.nextPage;
                pageNumber++;
                Debug.WriteLine("Page number: " + pageNumber);
            } while (isNextPage);


            return links.Distinct();
        }

        #region Private
        private string GetComment(IHtmlDocument doc)
        {
            var elements = doc.QuerySelectorAll(".tabs-content__list-item");

            var result = elements
                .Select(e => e.FirstElementChild.TextContent + ":" + e.LastElementChild.TextContent)
                .Aggregate((a, b) => $"{a}, {b}");

            return result;
        }

        private string GetImageSrc(IHtmlDocument doc)
        {
            var element = doc.QuerySelector(".car-page__item-photo-item img");
            var src = element.GetAttribute("src");

            return GetRootHost() + src;
        }

        private string GetYear(IHtmlDocument doc)
        {
            var elements = doc.QuerySelectorAll(".tabs-content .tabs-content__list-item");
            var element = elements
                .SingleOrDefault(e => e.FirstElementChild.TextContent.Contains("Год выпуска"))
                ?.LastElementChild;

            return element?.TextContent;
        }

        private double? GetMinimalPrice(IHtmlDocument doc)
        {
            try
            {
                var elements = doc.QuerySelectorAll(".price-block-item p");
                var price2 = Regex.Matches(elements[1].TextContent, "\\d+").Select(e => e.Value).Aggregate((a, b) => a + b);

                if (price2 != null)
                {
                    return Convert.ToDouble(price2);
                }
            }
            catch (Exception ex)
            {
                //ignore 
            }
            return null;
        }

        private double GetPrice(IHtmlDocument doc)
        {
            var elements = doc.QuerySelectorAll(".price-block-item p");

            var value = elements[0].TextContent.Trim();

            if (value.Contains("уточняйте цену", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new PriceNotFoundException(ProviderName, "Returns 'yточняте цену'");
            }

            var numbers = Regex.Matches(elements[0].TextContent, "\\d+").Select(e => e.Value).ToArray();

            if (numbers.Any())
            {
                var price1 = numbers.Aggregate((a, b) => a + b);
                return Convert.ToDouble(price1);
            }

            throw new PriceNotFoundException(ProviderName);
        }

        private string GetCityName(IHtmlDocument doc)
        {
            string cityName;
            string address = GetAddress(doc);
            if (Regex.IsMatch("", "^г\\."))
            {
                cityName = Regex.Match(address, @"[^г\.\s]([а-яА-Я\-]+)[^\,\s][а-яА-Я]", RegexOptions.IgnoreCase).Value;
            }
            else
            {
                cityName = new GoogleMapService().GetCityNameByAddress(address);
            }

            return cityName;
        }

        private string GetModelName(IHtmlDocument doc)
        {
            var elements = doc.QuerySelectorAll(".breadcrumbs a");
            var element = elements[3];
            var name = Regex.Match(element.TextContent, "^продажа\\s+новых\\s+([\\w\\s\\-]+)", RegexOptions.IgnoreCase)
                .Groups[1].Value;

            name = name.Replace(GetBrandName(doc), string.Empty);

            return name;
        }

        private string GetTypeName(IHtmlDocument doc)
        {
            return GetCategoryName(doc);
        }

        private string GetBrandName(IHtmlDocument doc)
        {
            var elements = doc.QuerySelectorAll(".breadcrumbs a");
            var element = elements[2];
            var name = Regex.Match(element.TextContent, "^продажа\\s+новых\\s+([\\w\\s\\-]+)", RegexOptions.IgnoreCase)
                .Groups[1].Value;

            return name;
        }

        private string GetCategoryName(IHtmlDocument doc)
        {
            var elements = doc.QuerySelectorAll(".tabs-content .tabs-content__list-item");
            var element = elements
                .SingleOrDefault(e => e.FirstElementChild.TextContent.Contains("Кузов"))
                ?.LastElementChild;

            return element?.TextContent;
        }

        private string GetDealershipNumberPhone(IHtmlDocument doc)
        {
            var element = doc.QuerySelector(".salon-phone a");
            return element.TextContent;
        }

        private string GetDealershipName(IHtmlDocument doc)
        {
            return null;
        }

        private string GetAddress(IHtmlDocument doc)
        {
            var element = doc.QuerySelector(".salon-info p:last-child");
            return element.TextContent;
        }

        private string Get(string url)
        {
            using (var client = new WebClient())
            {
                client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3626.96 Safari/537.36");
                client.Headers.Add("Accept-Language", "en-US,en;q=0.9,ru;q=0.8,he;q=0.7");

                var htmlPage = client.DownloadString(url);

                return htmlPage;
            }
        }

        private TResponse Post<TResponse>(string url, int pageNumber)
        {
            string result;
            using (var client = new WebClient())
            {
                client.Headers = new WebHeaderCollection()
                {
                    { "user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3626.96 Safari/537.36" },
                    { "X-Requested-With", "XMLHttpRequest" },
                    { "XMLHttpRequest", "application/x-www-form-urlencoded; charset=UTF-8" }
                };
                var reqparm = new NameValueCollection
                {
                    {"brand[]", ""},
                    {"model[]", ""},
                    {"price_min", ""},
                    {"price_max", ""},
                    {"body[]", ""},
                    {"fuel[]", ""},
                    {"gear_type[]", ""},
                    {"kpp[]", ""},
                    {"volume_min", ""},
                    {"volume_max", ""},
                    {"power_min", ""},
                    {"power_max", ""},
                    {"dealer[]", ""},
                    {"stock", "cars"},
                    {"page", pageNumber.ToString()},
                };

                var bytes = client.UploadValues(url, reqparm);
                result = Encoding.UTF8.GetString(bytes);
            }

            return JsonConvert.DeserializeObject<TResponse>(result);
        }

        private class NextPageResponse
        {
            public string list { get; set; }
            public bool nextPage { get; set; }
        }
        #endregion
    }

}
