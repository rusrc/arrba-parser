using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Arrba.Parser.DbContext.Entities;
using Arrba.Parser.Dto;
using Arrba.Parser.Exceptions;
using Arrba.Parser.Provider.Attributes;
using Arrba.Parser.Provider.Extension;
using Arrba.Parser.Services;

namespace Arrba.Parser.Provider.Realization
{
    [ProviderDescription(Host = "https://trakholding.ru")]
    public class TrakholdingRuProvider : BaseSiteProvider, ISiteProvider
    {
        readonly IHttpClient _httpClient;

        public TrakholdingRuProvider(IHttpClient httpClient)
        {
            this._httpClient = httpClient;
        }

        public async Task<VehicleRawDto> GetItemAsync(Url url)
        {
            var html = await _httpClient.GetAsync(url.Value);
            var data = await LazyDataDictionaries.Value;
            var htmlParser = new HtmlParser();
            var doc = htmlParser.ParseDocument(html);

            return new VehicleRawDto
            {
                DealershipName = this.GetDealershipName(doc),
                DealershipAddress = this.GetDealershipAddress(doc),
                DealershipNumberPhone = this.GetDealershipNumberPhone(doc),
                CategoryName = this.GetCategoryName(doc),
                BrandName = this.GetBrandName(doc, data),
                TypeName = this.GetTypeName(doc),
                ModelName = this.GetModelName(doc),
                CityName = this.GetCityName(doc),
                Price = this.GetPrice(doc),
                MinimalPrice = this.GetMinimalPrice(doc),
                Year = this.GetYear(doc),
                CurrencyName = this.GetCurrencyName(),
                Comment = this.GetComment(doc),
                Description = this.GetDescription(doc),
                ImageSrcs = this.GetImageSrcs(doc),
                Condition = (int)VehicleDto.ItemCondition.Used,
            };

        }

        public async Task<IEnumerable<Url>> GetUrlsAsync()
        {
            var host = this.GetRootHost();
            // https://trakholding.ru/catalog/traktora/Belorussia/catmann/
            // https://trakholding.ru/catalog/traktora/Belorussia/belarus/
            // https://trakholding.ru/catalog/traktora/japan/iseki/
            // https://trakholding.ru/catalog/traktora/japan/mitsubishi/
            // https://trakholding.ru/catalog/traktora/russia/rustrak/
            // https://trakholding.ru/catalog/traktora/japan/solis/
            // https://trakholding.ru/catalog/traktora/russia/chuvashpiller/
            // https://trakholding.ru/catalog/traktora/other/scout/


            // https://trakholding.ru/catalog/traktora/belorussia/kentavr/
            // https://trakholding.ru/catalog/traktora/japan/kubota/
            // https://trakholding.ru/catalog/traktora/japan/shibaura/
            // https://trakholding.ru/catalog/traktora/japan/yanmar/


            var urls = new List<string>();
            var pageNumber = 1;
            var isNextPage = false;

            do
            {
                var html = await _httpClient.GetAsync($"{host}/catalog/traktora/?PAGEN_1={pageNumber}&SIZEN_1=32");
                var htmlParser = new HtmlParser();
                var document = htmlParser.ParseDocument(html);
                var selector = ".catalog_product";
                var cells = document.QuerySelectorAll(selector);
                var linksFromHtml = cells
                    .Where(m =>
                    {
                        var stockStatus = m.QuerySelector(".quantity_in_catalog")?.TextContent ?? string.Empty;
                        var inStock = Regex.IsMatch(stockStatus, "В наличии", RegexOptions.IgnoreCase);
                        return inStock;
                    })
                    .Where(m =>
                    {
                        var price = m.QuerySelector(".catalog_product__price").TextContent;
                        var hasPrice = Regex.IsMatch(price, "\\d+");
                        return hasPrice;
                    })
                    .Select(m => m.QuerySelector("a"))
                    .Select(m => m.GetAttribute("href"))
                    .Select(url => host + url)
                    .ToList();


                urls.AddRange(linksFromHtml);

                isNextPage = linksFromHtml.Any();
                pageNumber++;

                Debug.WriteLine("Page number: " + pageNumber);
            } while (isNextPage);


            return urls.Distinct().Select(value => new Url
            {
                Value = value
            });
        }

        private string GetCategoryName(IHtmlDocument doc) => "Minitraktory-i-traktory";

        private string GetBrandName(IHtmlDocument doc, DataDictionaries data)
        {
            var title = doc.QuerySelector("h1 .name-of-prod")?.TextContent ?? String.Empty;
            var brandName = data.BrandName_BrandId.SingleOrDefault(b => title.Contains(b.Key)).Key;

            return brandName;
        }

        private double GetPrice(IHtmlDocument doc)
        {
            var standardPrice = doc.QuerySelector(".flypage__price .price_fly_page")?.TextContent.RemoveSpaces();
            // var oldPrice = doc.QuerySelector(".flypage__price .old_price_fly_page")?.TextContent.RemoveSpaces();
            var newPrice = doc.QuerySelector(".flypage__price .new_price_fly_page")?.TextContent.RemoveSpaces();

            var price = string.Empty;

            if (!string.IsNullOrEmpty(standardPrice))
            {
                price = standardPrice;
            }

            if (!string.IsNullOrEmpty(newPrice))
            {
                price = newPrice;
            }

            return GetPriceFromText(price);
        }

        private double? GetMinimalPrice(IHtmlDocument doc)
        {
            var oldPrice = doc.QuerySelector(".flypage__price .old_price_fly_page")?.TextContent.RemoveSpaces();

            if (!string.IsNullOrEmpty(oldPrice))
            {
                return GetPriceFromText(oldPrice);
            }

            return null;
        }

        private string GetDealershipName(IHtmlDocument doc) => "trakholding";

        private string GetDealershipAddress(IHtmlDocument doc) => "Московская область, г. Ивантеевка, Санаторный проезд, Корпус 1";

        private string GetDealershipNumberPhone(IHtmlDocument doc) => "88007750496";

        private string GetTypeName(IHtmlDocument doc) => "Минитракторы";

        private string GetModelName(IHtmlDocument doc) => null;

        private string GetCityName(IHtmlDocument doc) => "Москва";

        private string GetYear(IHtmlDocument doc) => null;

        private string GetCurrencyName() => "RUR";

        private string GetDescription(IHtmlDocument doc)
        {
           var cells = doc.QuerySelectorAll(".flypage__features_list__item");
            var features = cells
                .Select(c =>
                {
                    var key = Regex.Replace(c.QuerySelector(".flypage__features_list__item__title")?.TextContent ?? "", @"\t|\n|\r", "");
                    var value = Regex.Replace(c.QuerySelector(".flypage__features_list__item__value")?.TextContent ?? "", @"\t|\n|\r", "");

                    return $"{key}:{value}";
                })
                .Aggregate((prev, next) => $"{prev}, {next}");

            return features;
        }

        private string GetComment(IHtmlDocument doc)
        {
            var cells = doc.QuerySelectorAll(".product_prop .properties_catalog");
            var features = cells
                .Select(c =>
                {
                    var key = Regex.Replace(c.QuerySelector(".properties_prod_name")?.TextContent ?? "", @"\t|\n|\r", "");
                    var value = Regex.Replace(c.QuerySelector(".properties_prod_value")?.TextContent ?? "", @"\t|\n|\r", "");

                    return $"{key}:{value}";
                })
                .Aggregate((prev, next) => $"{prev}, {next}");

            return features;
        }

        private string[] GetImageSrcs(IHtmlDocument doc)
        {
            var imgs = doc.QuerySelectorAll(".flypage__top__left_image a")
                .Select(i => GetRootHost() + i.GetAttribute("href"))
                .Distinct()
                // .Select(e => GetRootHost() + e)
                .ToArray();

            if (imgs.Length <= 0)
            {
                throw new NotFoundException(ProviderName, "Can't get image links");
            }

            return imgs;
        }
    }
}
