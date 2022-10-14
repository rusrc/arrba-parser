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
        private readonly IEnumerable<ISiteProvider> _siteProviders;
        private readonly ILogService _logger;
        private readonly ParserDbContext _context;

        public UrlManager(IEnumerable<ISiteProvider> siteProviders, ILogService logger, ParserDbContext context)
        :base(logger, context)
        {
            this._siteProviders = siteProviders;
            this._logger = logger;
            this._context = context;
        }

        public void Run()
        {
            Task.WaitAll(RunAsync());
        }

        public async Task RunAsync()
        {
            foreach (ISiteProvider siteProvider in _siteProviders)
            {
                try
                {
                    var providerName = siteProvider.GetType().Name;
                    var linksRequest = GetUrlInfo(providerName);
                    var urls = await siteProvider.GetUrlsAsync();
                    var now = DateTime.Now;

                    var linksFromDb = _context.Urls.Where(l => l.LinksRequestId == linksRequest.Id).ToArray();
                    foreach (var url in urls)
                    {
                        if (linksFromDb.All(l => l.Value != url))
                        {
                            _context.Urls.Add(new Url
                            {
                                Value = url,
                                CreateDate = now,
                                LinksRequest = linksRequest
                            });
                        }
                    }

                    _context.SaveChanges();
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
}
