using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Arrba.Parser.DbContext.Entities;
using Arrba.Parser.Dto;
using Arrba.Parser.Provider.Attributes;
using Arrba.Parser.Services;

namespace Arrba.Parser.Provider.Realization
{
    [ProviderDescription(Host = "https://pitbikeclub.ru")]
    public class PitbikeclubRuProvider : BaseSiteProvider, ISiteProvider
    {
        public PitbikeclubRuProvider(IHttpClient httpClient)
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
            var pageNumber = 0;
            var isNextPage = false;

            do
            {
                var html = await _httpClient.GetAsync($"{rootHost}/catalog/pitbayki/new/start{pageNumber}");
                var htmlParser = new HtmlParser();
                var document = htmlParser.ParseDocument(html);
                var selector = ".products_container .products_slide_box";
                var cells = document.QuerySelectorAll(selector);
                var linksFromHtml = cells
                    .Where(m =>
                    {
                        var stockStatus = m.QuerySelector(".products_slide_box_btn")?.TextContent;
                        // var price = m.QuerySelector(".card__price").TextContent;
                        var sold = !Regex.IsMatch(stockStatus, "В наличии", RegexOptions.IgnoreCase);

                        return !sold;
                    })
                    .Select(m => m.QuerySelector("a"))
                    .Select(m => m.GetAttribute("href"))
                    .ToList();


                urls.AddRange(linksFromHtml);

                isNextPage = linksFromHtml.Any();
                pageNumber++;

                Debug.WriteLine("Page number: " + pageNumber);
            } while (isNextPage);


            return urls.Distinct().Select(value => new Url
            {
                Value = $"{rootHost}/{value}"
            });
        }

        private string[] GetImageSrcs(IHtmlDocument doc)
        {
            var imgs = doc.QuerySelectorAll(".products_container .products_slide_box")
               .Select(i => GetRootHost() + i.GetAttribute("src"))
               .ToArray();

            return imgs;
        }

        private string GetDescription(IHtmlDocument doc)
        {
            var posts = doc.QuerySelectorAll(".post-text p").ToArray();
            bool takeNext = false;
            string desc = "";
            foreach (var post in posts)
            {
                var t = post.InnerHtml;
                if (takeNext)
                {
                    desc = post.TextContent;
                    break;
                }
                if (t.Contains("<!-- ОПИСАНИЕ  -->"))
                    takeNext = true;
            }
            var cells = doc.QuerySelectorAll(".haracteristiki tr").ToArray();
            var features = cells
                .Select(c =>
                {
                    var key = Regex.Replace(c.QuerySelectorAll("td")[0]?.TextContent ?? "", @"\t|\n|\r", "");
                    var value = Regex.Replace(c.QuerySelectorAll("td")[1]?.TextContent ?? "", @"\t|\n|\r", "");

                    return $"{key}: {value}";
                })
                .Aggregate((prev, next) => $"{prev}, {next}");

            return desc + " " + features;
        }

        private string GetComment(IHtmlDocument doc) => null;

        private string GetCurrencyName() => "RUR";

        private string GetYear(IHtmlDocument doc) => null;

        private double? GetMinimalPrice(IHtmlDocument doc) => null;

        private double GetPrice(IHtmlDocument doc)
        {
            var priceText = doc
                .QuerySelector("#rezultat")
                ?.TextContent;

            return GetPriceFromText(priceText);
        }

        private string GetCityName(IHtmlDocument doc) => "Воронеж";

        private string GetModelName(IHtmlDocument doc) => null;

        private string GetBrandName(IHtmlDocument doc)
        {
            return null;
        }
        private string GetCategoryName(IHtmlDocument doc) => "Pricepy";

        private string GetDealershipNumberPhone(IHtmlDocument doc) => "74732582318";

        private string GetDealershipAddress(IHtmlDocument doc) => "г. Воронеж, ул. Монтажный проезд, д. 12Д";

        private string GetDealershipName(IHtmlDocument doc) => "Прицеп36";

        private string GetTypeName(object doc) => "Разные";

        private readonly IHttpClient _httpClient;

    }
}
