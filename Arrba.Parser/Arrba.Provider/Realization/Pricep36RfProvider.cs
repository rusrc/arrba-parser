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
    [ProviderDescription(Host = "http://прицеп36.рф")]
    public class Pricep36RfProvider : BaseSiteProvider, ISiteProvider
    {
        private readonly string[] _brands = {
            "кремень",
            "караван",
            "трейлер",
            "сст",
            "avtos",
            "мзса",
            "кремос",
            "титан",
            "прогресс",
            "смарт",
            "гранит",
            "креон",
            "сааз",
            "alaska",
            "норд",
            "оптима"
        };
        public Pricep36RfProvider(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<VehicleRawDto> GetItemAsync(Url url)
        {
            var html = await _httpClient.GetAsync(url.Value);
            var htmlParser = new HtmlParser();
            var doc = htmlParser.ParseDocument(html);
            double? price = this.GetPrice(doc);
            string brand = GetBrandName(url.Value);
            if (price != null && brand != null)
            {
                var vv = new VehicleRawDto
                {
                    DealershipName = this.GetDealershipName(doc),
                    DealershipAddress = this.GetDealershipAddress(doc),
                    DealershipNumberPhone = this.GetDealershipNumberPhone(doc),
                    CategoryName = this.GetCategoryName(doc),
                    BrandName = brand,
                    TypeName = this.GetTypeName(doc),
                    ModelName = this.GetModelName(doc),
                    CityName = this.GetCityName(doc),
                    Price = (double)price,
                    MinimalPrice = this.GetMinimalPrice(doc),
                    Year = this.GetYear(doc),
                    CurrencyName = this.GetCurrencyName(),
                    Comment = this.GetComment(doc),
                    Description = this.GetDescription(doc),
                    ImageSrcs = this.GetImageSrcs(doc),
                    Condition = (int)VehicleDto.ItemCondition.New,
                };
                return vv;
            }
            return null;
        }

        public async Task<IEnumerable<Url>> GetUrlsAsync()
        {
            var rootHost = GetRootHost();
            var urls = new List<string>();
            var pageNumber = 1;

            var html = await _httpClient.GetAsync($"{rootHost}");
            var htmlParser = new HtmlParser();
            var document = await htmlParser.ParseDocumentAsync(html);
            var selector = ".wh-block a";
            var pageLinks = document.QuerySelectorAll(selector)
                .Select(s => s.GetAttribute("href"))
                .Where(v => v.Contains("прицепы") && _brands.Any(v.Contains)) // только прицепы с брендами
                .Distinct()
                .ToArray();

            foreach (var pageLink in pageLinks)
            {
                var htmlPage = await _httpClient.GetAsync(GetRootHost() + pageLink);
                document = await htmlParser.ParseDocumentAsync(htmlPage);
                var _urls = document.QuerySelectorAll(".rt-img-holder a")
                    .Select(s => s.GetAttribute("href"))
                    .Where(aa => _brands.Any(bb => aa.Contains(bb)))
                    .Distinct()
                    .ToArray();

                urls.AddRange(_urls);

                Debug.WriteLine("Page number: " + pageNumber);

                pageNumber++;
            }
            return urls.Distinct().Select(value => new Url
            {
                Value = value
            });
        }

        private string[] GetImageSrcs(IHtmlDocument doc)
        {
            var imgs = doc.QuerySelectorAll(".n2-ss-slide-background-mask img")
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

        private double? GetPrice(IHtmlDocument doc)
        {
            var priceText = doc
                .QuerySelector("#rezultat")
                ?.TextContent;
            if (!string.IsNullOrEmpty(priceText))
                return GetPriceFromText(priceText);
            return null;
        }

        private string GetCityName(IHtmlDocument doc) => "Воронеж";

        private string GetModelName(IHtmlDocument doc) => null;

        private string GetBrandName(string link) => _brands.FirstOrDefault(bb => link.Contains(bb))?.ToUpper();

        private string GetCategoryName(IHtmlDocument doc) => "Pricepy";

        private string GetDealershipNumberPhone(IHtmlDocument doc) => "74732582318";

        private string GetDealershipAddress(IHtmlDocument doc) => "г. Воронеж, ул. Монтажный проезд, д. 12Д";

        private string GetDealershipName(IHtmlDocument doc) => "Прицеп36";

        private string GetTypeName(object doc) => "Разные";

        private readonly IHttpClient _httpClient;

    }
}
