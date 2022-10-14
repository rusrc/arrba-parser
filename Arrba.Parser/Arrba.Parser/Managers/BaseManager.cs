using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Arrba.Parser.DbContext;
using Arrba.Parser.DbContext.Entities;
using Arrba.Parser.Dto;
using Arrba.Parser.Exceptions;
using Arrba.Parser.Logger;
using Arrba.Parser.Services;

namespace Arrba.Parser.Managers
{
    public class BaseManager
    {
        private readonly ILogService _logger;
        private readonly ParserDbContext _context;

        public BaseManager(ILogService logger, ParserDbContext context)
        {
            this._logger = logger;
            this._context = context;
        }

        public async Task<DealershipDto> GetDealershipByName(string name)
        {
            try
            {
                return await ArrbaApiService.GetDealershipAsync(name);
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse r && r.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new DealershipNotFoundException(GetType().Name, $"can't get dealership by name: {name}", ex);
                }

                throw new ProviderException(GetType().Name, ex);
            }
        }

        protected UrlInfo GetUrlInfo(string providerName)
        {
            var urlInfo = _context.UrlInfos
                .SingleOrDefault(r => r.ProviderName == providerName) ?? new UrlInfo
                {
                    ProviderName = providerName,
                    CreateDate = DateTime.Now
                };

            return urlInfo;
        }

        protected string GetToken(string providerName)
        {
            string token = string.Empty;

            try
            {
                string userName = ParserConfiguration.GetUserName(providerName);
                string password = ParserConfiguration.GetUserPassword(providerName);
                token = ArrbaApiService.GetToken(userName, password);

            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }

            return token;
        }
    }
}
