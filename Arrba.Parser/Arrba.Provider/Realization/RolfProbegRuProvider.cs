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
using Arrba.Parser.Exceptions;
using Arrba.Parser.Provider.Attributes;
using Arrba.Parser.Provider.Extension;
using Arrba.Parser.Services;


namespace Arrba.Parser.Provider.Realization
{
    [ProviderDescription(Host = "https://rolf-probeg.ru")]
    public class RolfProbegRuProvider : BaseSiteProvider, ISiteProvider
    {
        readonly IHttpClient _httpClient;

        public RolfProbegRuProvider(IHttpClient httpClient)
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
                Condition = (int)VehicleDto.ItemCondition.Used
            };
        }

        public async Task<IEnumerable<Url>> GetUrlsAsync()
        {
            var rootHost = GetRootHost();
            var links = new List<string>();
            var pageNumber = 1;
            var isNextPage = false;

            do
            {
                var html = await _httpClient.GetAsync($"{rootHost}/cars/page/{pageNumber}/");
                var htmlParser = new HtmlParser();
                var document = htmlParser.ParseDocument(html);
                var selector = ".card-list__item a";
                var cells = document.QuerySelectorAll(selector);
                var linksFromHtml = cells
                    .Where(m =>
                        {
                            var price = m.QuerySelector(".card__price").TextContent;
                            var sold = Regex.IsMatch(price, "продан", RegexOptions.IgnoreCase);

                            return !sold;
                        }
                    )
                    .Select(m => m.GetAttribute("href"))
                    .ToList();


                links.AddRange(linksFromHtml);

                isNextPage = linksFromHtml.Any();
                pageNumber++;

                Debug.WriteLine("Page number: " + pageNumber);
            } while (isNextPage);


            return links.Distinct().Select(value => new Url
            {
                Value = value
            });
        }

        #region helpers
        string GetCurrencyName()
        {
            return "RUR";
        }

        string GetDealershipAddress(IHtmlDocument doc)
        {
            return GetDealershipName(doc);
        }

        string GetDescription(IHtmlDocument doc)
        {
            var cells = doc.QuerySelectorAll(".info-table__row");
            var features = cells
                .Select(c =>
                {
                    var key = c.QuerySelector(".info-table__data-name").TextContent;
                    var value = c.QuerySelector(".info-table__data").TextContent;

                    return $"{key}:{value}";
                })
                .Aggregate((prev, next) => $"{prev}, {next}");

            return features;
        }

        string GetComment(IHtmlDocument doc)
        {
            var cells = doc.QuerySelectorAll(".feature-list__item");
            var features = cells
                .Select(c => c.TextContent)
                .Aggregate((prev, next) => $"{prev}, {next}");

            return features;
        }

        string GetDealershipName(IHtmlDocument doc)
        {
            return doc.QuerySelector(".map-link__caption").TextContent.Trim();
        }

        string GetTypeName(IHtmlDocument doc)
        {
            return GetCategoryName(doc);
        }
        string GetBrandName(IHtmlDocument doc)
        {
            var cells = doc.QuerySelectorAll(".breadcrumbs__item");
            return cells[2].TextContent.RemoveSpaces();
        }

        string GetModelName(IHtmlDocument doc)
        {
            var cells = doc.QuerySelectorAll(".breadcrumbs__item");
            return cells[3].TextContent.RemoveSpaces();
        }

        string GetCityName(IHtmlDocument doc)
        {
            var cells = doc.QuerySelectorAll(".breadcrumbs__item .breadcrumbs__link");

            var spb = cells.Any(c => Regex.IsMatch(c.TextContent, "Санкт-Петербург", RegexOptions.IgnoreCase));
            var msk = cells.Any(c => Regex.IsMatch(c.TextContent, "Москв", RegexOptions.IgnoreCase));

            if (spb) return "Санкт-Петербург";
            if (msk) return "Москва";

            throw new NotFoundException(ProviderName, "Can't gat city name");
        }

        double GetPrice(IHtmlDocument doc)
        {
            var price = GetNumber(doc.QuerySelector(".price .price__current").TextContent.RemoveSpaces());
            var oldPrice = GetNumber(doc.QuerySelector(".price .price__old").TextContent.RemoveSpaces());

            if (oldPrice == 0)
            {
                return price;
            }

            if (oldPrice > price)
            {
                return oldPrice;
            }

            return price;
        }

        double? GetMinimalPrice(IHtmlDocument doc)
        {
            var price = GetNumber(doc.QuerySelector(".price .price__current").TextContent.RemoveSpaces());
            var oldPrice = GetNumber(doc.QuerySelector(".price .price__old").TextContent.RemoveSpaces());

            if (oldPrice == 0)
            {
                return price;
            }

            if (oldPrice > price)
            {
                return price;
            }

            return price;
        }

        string GetYear(IHtmlDocument doc)
        {
            var cells = doc.QuerySelectorAll(".product-info__item .info-table__row");
            var typeCell = cells.FirstOrDefault(c =>
                Regex.IsMatch(c.QuerySelector(".info-table__data-name").TextContent, "год выпуска", RegexOptions.IgnoreCase));

            return typeCell?.QuerySelector(".info-table__data").TextContent;
        }

        string[] GetImageSrcs(IHtmlDocument doc)
        {
            var imgLinks = doc
                .QuerySelectorAll(".gallery__wrapper .gallery__slide")
                .Select(l => l.GetAttribute("href"))
                .ToArray();

            return imgLinks;
        }

        string GetCategoryName(IHtmlDocument doc)
        {
            var cells = doc.QuerySelectorAll(".product-info__item .info-table__row");
            var typeCell = cells.FirstOrDefault(c =>
                Regex.IsMatch(c.QuerySelector(".info-table__data-name").TextContent, "кузов", RegexOptions.IgnoreCase));

            if (typeCell == null)
            {
                throw new NotFoundException(ProviderName, "Can't get the category or type");
            }

            var
            siteValue = typeCell.QuerySelector(".info-table__data").TextContent;
            siteValue = Regex.Match(siteValue, "([а-яА-Я\\s]+)(?![\\d\\w]+)").Groups[1].Value;

            return siteValue;
        }

        string GetDealershipNumberPhone(IHtmlDocument doc)
        {
            var phoneNumber = doc.QuerySelector(".sub-header__right [class*='call_phone_']").TextContent.RemoveSpaces();
            var clearPhoneNumber =
                Regex.Matches(phoneNumber, "\\d+")
                    .Select(m => m.Value)
                    .Aggregate((prev, next) => prev + next);

            return clearPhoneNumber;
        }
        #endregion
    }
}
