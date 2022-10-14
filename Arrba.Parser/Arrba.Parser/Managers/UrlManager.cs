using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arrba.Parser.DbContext;
using Arrba.Parser.DbContext.Entities;
using Arrba.Parser.Exceptions;
using Arrba.Parser.Logger;
using Arrba.Parser.Provider;

namespace Arrba.Parser.Managers
{
    public class UrlManager : BaseManager, IParserManager
    {
        private readonly ISiteProvider _siteProvider;
        private readonly ILogService _logger;
        private readonly ParserDbContext _context;

        public UrlManager(ISiteProvider siteProvider, ILogService logger, ParserDbContext context)
        : base(logger, context)
        {
            this._siteProvider = siteProvider;
            this._logger = logger;
            this._context = context;
        }

        public void Run()
        {
            Task.WaitAll(RunAsync());
        }

        public async Task RunAsync()
        {
            try
            {
                var providerName = _siteProvider.GetType().Name;
                var urlInfo = GetUrlInfo(providerName);
                var urls = await _siteProvider.GetUrlsAsync();
                var now = DateTime.Now;

                var linksFromDb = _context.Urls.Where(l => l.UrlInfoId == urlInfo.Id).ToArray();
                foreach (var url in urls)
                {
                    if (linksFromDb.All(l => l.Value != url.Value))
                    {
                        _context.Urls.Add(new Url
                        {
                            Value = url.Value,
                            CreateDate = now,
                            UrlInfo = urlInfo
                        });
                    }
                }

                _context.SaveChanges();
                _logger.Info("Urls added successfully");
            }
            catch (NotFoundException ex)
            {
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }

        }
    }
}
