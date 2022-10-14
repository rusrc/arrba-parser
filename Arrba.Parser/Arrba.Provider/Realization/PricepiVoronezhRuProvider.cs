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
using Arrba.Parser.Services;

namespace Arrba.Parser.Provider.Realization
{
    /// <summary>
    /// http://прицеп-воронеж.рф
    /// </summary>
    [ProviderDescription(Host = "http://прицеп-воронеж.рф")]
    public class PricepiVoronezhRuProvider : BaseSiteProvider, ISiteProvider
    {
        private readonly IHttpClient _httpClient;

        public PricepiVoronezhRuProvider(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<VehicleRawDto> GetItemAsync(Url url)
        {
            var html = await _httpClient.GetAsync(url.Value);
            var htmlParser = new HtmlParser();
            var doc = htmlParser.ParseDocument(html);

            return new VehicleRawDto
            {
                DealershipName = this.GetDealershipName(doc),
                DealershipAddress = this.GetDealershipAddress(doc),
                DealershipNumberPhone = this.GetDealershipNumberPhone(doc),
                CategoryName = this.GetCategoryName(doc),
                BrandName = this.GetBrandName(doc),
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
            var rootHost = GetRootHost();
            var urls = new List<string>();
            var pageNumber = 1;

            var html = await _httpClient.GetAsync($"{rootHost}");
            var htmlParser = new HtmlParser();
            var document = htmlParser.ParseDocument(html);
            var selector = ".left-menu li";
            var cells = document.QuerySelectorAll(selector);
            var pageUrls = cells
                .Select(m => m.QuerySelector("a"))
                .Select(m => m.GetAttribute("href"))
                .ToList();


            foreach (var pageUrl in pageUrls)
            {
                html = await _httpClient.GetAsync($"{rootHost}{pageUrl}");
                document = new HtmlParser().ParseDocument(html);

                selector = ".catalog-section .ff";
                cells = document.QuerySelectorAll(selector);

                var linksFromHtml = cells
                    .Where(m =>
                    {
                        var stockStatus = m.QuerySelector(".PRICE")?.TextContent;
                        var sold = !Regex.IsMatch(stockStatus ?? "", "Цена", RegexOptions.IgnoreCase);

                        return !sold;
                    })
                    .Select(m => m.QuerySelector("a"))
                    .Select(m => m.GetAttribute("href"))
                    .Select(url => rootHost + url)
                    .ToList();

                urls.AddRange(linksFromHtml);

                pageNumber++;
                Debug.WriteLine("Page number: " + pageNumber);
            }

            return urls.Distinct().Select(value => new Url
            {
                Value = value
            });
        }

        private string GetDealershipName(IHtmlDocument doc) => "прицеп-воронеж";

        private string GetDealershipAddress(IHtmlDocument doc) => "г. Воронеж, ул. Димитрова, дом 118";

        private string GetDealershipNumberPhone(IHtmlDocument doc) => "79507635426";

        private string GetCategoryName(IHtmlDocument doc) => "Pricepy";

        private string GetBrandName(IHtmlDocument doc)
        {
            var cells = doc.QuerySelectorAll(".bx-breadcrumb-item [itemprop=\"title\"]");

            var brand = cells[2]?.TextContent;

            if (brand != null)
            {
                if (brand.Equals("Прицепы Воронежского Производства", StringComparison.CurrentCultureIgnoreCase))
                {
                    brand = cells[3]?.TextContent;
                }
                else if (Regex.IsMatch(brand, "Прицепы", RegexOptions.IgnoreCase | RegexOptions.ECMAScript))
                {
                    brand = Regex
                        .Match(brand, "Прицепы\\s+(?<brand>[\\W\\w+]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ECMAScript)
                        .Groups["brand"].Value;
                }
            }

            return brand;
        }

        private string GetTypeName(IHtmlDocument doc) => "Разные";

        private string GetModelName(IHtmlDocument doc) => null;

        private string GetCityName(IHtmlDocument doc) => "Воронеж";

        private double GetPrice(IHtmlDocument doc)
        {
            var priceText = doc
                .QuerySelector(".catalog-detail-desc .PRICE")
                ?.TextContent;

            return GetPriceFromText(priceText);
        }

        private double? GetMinimalPrice(IHtmlDocument doc) => null;

        private string GetYear(IHtmlDocument doc) => null;

        private string GetCurrencyName() => "RUR";

        private string GetComment(IHtmlDocument doc)
        {
            var features = doc.QuerySelectorAll(".catalog-detail-desc ul li")
                .Select(l => l.TextContent)
                .Aggregate((prev, next) => $"{prev}, {next}");

            return features;
        }

        private string GetDescription(IHtmlDocument doc)
        {
            var cells = doc.QuerySelectorAll(".catalog-detail-full-desc table tr");
            var features = cells
                .Select(c =>
                {
                    var key = Regex.Replace(c.QuerySelectorAll("td")[0]?.TextContent ?? "", @"\t|\n|\r", "");
                    var value = Regex.Replace(c.QuerySelectorAll("td")[1]?.TextContent ?? "", @"\t|\n|\r", "");

                    return $"{key}:{value}";
                })
                .Aggregate((prev, next) => $"{prev}, {next}");

            return features;
        }

        private string[] GetImageSrcs(IHtmlDocument doc)
        {
            var imgs = doc.QuerySelectorAll(".catalog-detail-images a")
                .Select(i => GetRootHost() + i.GetAttribute("href"))
                .ToArray();

            if (imgs.Length <= 0)
            {
                throw new NotFoundException(ProviderName, "Can't get image links");
            }

            return imgs;
        }
    }
}
