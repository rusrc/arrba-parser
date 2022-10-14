using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Arrba.Parser.DbContext;
using Arrba.Parser.Extension;
using Arrba.Parser.Logger;
using Arrba.Parser.Mapper;
using Arrba.Parser.Processors;
using Arrba.Parser.Provider;

namespace Arrba.Parser.Managers
{
    public class SaveManagerTest : BaseManager, IParserManager
    {
        private readonly ISiteProvider _siteProvider;
        private readonly ILogService _logger;
        private readonly BaseMapper _mapper;
        private readonly ParserDbContext _context;

        public SaveManagerTest(ISiteProvider siteProvider, ILogService logger, BaseMapper mapper, ParserDbContext context)
        : base(logger, context)
        {
            this._siteProvider = siteProvider;
            this._logger = logger;
            this._context = context;
            this._mapper = mapper;
        }

        public void Run()
        {
            var providerName = _siteProvider.GetType().Name;
            var urlInfo = GetUrlInfo(providerName);
            var allUrls = _context.Urls
                .Where(u => u.UrlInfoId == urlInfo.Id)
                .Where(u => u.Status == Status.WaitToCheck || u.Status == null)
                .Where(u => u.ExternalId <= 0)
                .OrderBy(u => u.Id)
                .DistinctBy(u => u.Value)
                .ToArray();

            var token = GetToken(providerName);
            var processor = new SaveItemProcessorTest(_siteProvider, _logger, _mapper);

            var tasks = new List<Task>();
            int lastBadUrlsCount = 0;
            foreach (var url in allUrls)
            {
                if(url.Status == Status.Active) continue;

                tasks.Add(processor.RunAsync(url, token));
                tasks.Add(Task.Delay(2000));

                try
                {
                    var sw = Stopwatch.StartNew();

                    Task.WaitAll(tasks.ToArray());

                    sw.Stop();
                    _logger.Info($"Query with time deloy took: {sw.ElapsedMilliseconds} ms");
                }
                catch (Exception ex)
                {
                    _logger.Error("Critical", ex);
                }

                _logger.Error($"Bad urls count: {processor.BadUrls.Count()}");


                if (lastBadUrlsCount < processor.BadUrls.Count())
                {
                    lastBadUrlsCount = processor.BadUrls.Count();
                }
            }
        }
    }
}
