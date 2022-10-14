using System;
using System.Collections.Generic;
using System.Linq;
using Arrba.Parser.Provider.Realization;

namespace Arrba.Parser
{
    public class ParserConfiguration
    {
        private static readonly Dictionary<string, string> userNames =
            new Dictionary<string, string>
            {
                {$"userName:{nameof(RolfProbegRuProvider)}", "info@rolf.ru"},
                {$"userName:{nameof(PricepiVoronezhRuProvider)}", "mr.prisep@mail.ru"},
                {$"userName:{nameof(Pricep36RfProvider)}", "pricep36@mail.ru"},
                {$"userName:{nameof(PitbikeclubRuProvider)}", "info@pitbikeclub.ru"},
                {$"userName:{nameof(TrakholdingRuProvider)}", "info@trakholding.ru" }
            };

        private static readonly Dictionary<string, string> userPasswords =
            new Dictionary<string, string>
            {
                {$"userPassword:{nameof(RolfProbegRuProvider)}", "UqPp2sLn1JD8zVgke6sv"},
                {$"userPassword:{nameof(PricepiVoronezhRuProvider)}", "pEZbHz9Nl"},
                {$"userPassword:{nameof(Pricep36RfProvider)}", "pEZbHz9Nl23"},
                {$"userPassword:{nameof(PitbikeclubRuProvider)}", "9GG!3Kws&K^j"},
                {$"userPassword:{nameof(TrakholdingRuProvider)}", "0fCJDFcJVpd^jUr2"}
            };

        public static string GetUserName(string providerName)
        {
            try
            {
                return userNames.SingleOrDefault(d => d.Key == $"userName:{providerName}").Value;
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException("Please provide the providerName for userName", ex);
            }
        }

        public static string GetUserPassword(string providerName)
        {
            try
            {
                return userPasswords.SingleOrDefault(d => d.Key == $"userPassword:{providerName}").Value;
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException("Please provide the providerName for password", ex);
            }
        }
    }
}
