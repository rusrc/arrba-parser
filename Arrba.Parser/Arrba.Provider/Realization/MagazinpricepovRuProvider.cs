using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using Arrba.Parser.DbContext.Entities;
using Arrba.Parser.Dto;
using Arrba.Parser.Provider.Attributes;
using Arrba.Parser.Services;

namespace Arrba.Parser.Provider.Realization
{
    [ProviderDescription(Host = "https://magazinpricepov.ru")]
    public class MagazinpricepovRuProvider : BaseSiteProvider, ISiteProvider
    {
        readonly IHttpClient _httpClient;

        public MagazinpricepovRuProvider(IHttpClient httpClient)
        {
            this._httpClient = httpClient;
        }

        public async Task<VehicleRawDto> GetItemAsync(Url url)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Url>> GetUrlsAsync()
        {
            var rootHost = GetRootHost();
            var urls = new List<string>();
            var pageNumber = 1;
            var isNextPage = false;

            do
            {
                var html = await _httpClient.GetAsync($"{rootHost}/pricepi/?page={pageNumber}");
                var htmlParser = new HtmlParser();
                var document = htmlParser.ParseDocument(html);
                var selector = ".product-layout";
                var cells = document.QuerySelectorAll(selector);
                var linksFromHtml = cells
                    .Where(m =>
                    {
                        var stockStatus = m.QuerySelector(".stock_status")?.TextContent;
                        // var price = m.QuerySelector(".card__price").TextContent;
                        var sold = !Regex.IsMatch(stockStatus, "В наличии", RegexOptions.IgnoreCase);

                        return !sold;
                    })
                    .Select(m => m.QuerySelector(".image a"))
                    .Select(m => m.GetAttribute("href"))
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
    }
}
