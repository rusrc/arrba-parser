using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Arrba.Parser.Exceptions;
using Arrba.Parser.Provider.Attributes;
using Arrba.Parser.Services;

namespace Arrba.Parser.Provider
{
    public class BaseSiteProvider
    {
        protected readonly Lazy<Task<DataDictionaries>> LazyDataDictionaries;
        public BaseSiteProvider()
        {
            LazyDataDictionaries = new Lazy<Task<DataDictionaries>>(async () => await new DataDictionaries().SeedAsync());
        }

        public string ProviderName => GetType().Name;
        
        protected virtual string GetRootHost()
        {
            var descriptionAttribute = (ProviderDescriptionAttribute)this.GetType()
                .GetCustomAttribute(typeof(ProviderDescriptionAttribute), true);


            var hostName = Regex.Match(descriptionAttribute?.Host ?? "", @"(https?://[\w\.\d\W]+)",
                RegexOptions.IgnoreCase | RegexOptions.Multiline).Groups[1].Value;

            if (string.IsNullOrEmpty(hostName))
            {
                throw new ProviderException(
                    ProviderName, $@"Can't get the value from DescriptionAttribute provided on siteProvider {ProviderName}. 
                       Please provide the {ProviderName} with attribute [Description(""host:https://www.example.ru"")]");
            }

            return hostName;
        }

        protected double GetNumber(string str)
        {
            var number = Regex.Matches(str, "\\d+").Select(m => m.Value).Aggregate((prev, next) => prev + next);
            return Convert.ToDouble(number);
        }

        /// <summary>
        /// Get price from text
        /// </summary>
        /// <param name="priceText">Text contains price. For example Цена: от 38500 руб.</param>
        /// <returns></returns>
        protected virtual double GetPriceFromText(string priceText)
        {
            if (priceText != null && Regex.IsMatch(priceText, "\\d+"))
            {
                return GetNumber(priceText);
            }

            throw new PriceNotFoundException(ProviderName);
        }
    }
}
