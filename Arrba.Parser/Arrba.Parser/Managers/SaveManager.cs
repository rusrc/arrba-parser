using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Arrba.Parser.DbContext;
using Arrba.Parser.DbContext.Entities;
using Arrba.Parser.Extension;
using Arrba.Parser.Logger;
using Arrba.Parser.Mapper;
using Arrba.Parser.Processors;
using Arrba.Parser.Provider;

namespace Arrba.Parser.Managers
{
    public class SaveManager : BaseManager, IParserManager
    {
        private readonly IEnumerable<ISiteProvider> _siteProviders;
        private readonly ILogService _logger;
        private readonly BaseMapper _mapper;
        private readonly ParserDbContext _context;

        public SaveManager(
            IEnumerable<ISiteProvider> siteProviders, 
            ILogService logger, 
            BaseMapper mapper, 
            ParserDbContext context)
        : base(logger, context)
        {
            this._siteProviders = siteProviders;
            this._logger = logger;
            this._context = context;
            this._mapper = mapper;
        }

        public void Run()
        {
            foreach (ISiteProvider siteProvider in _siteProviders)
            {
                var providerName = siteProvider.GetType().Name;
                var urlInfo = GetUrlInfo(providerName);
                var allUrls = _context.Urls
                    .Where(u => u.UrlInfoId == urlInfo.Id)
                    .Where(u => u.Status == Status.WaitToCheck || u.Status == null)
                    .Where(u => u.ExternalId <= 0)
                    .OrderBy(u => u.Id)
                    .DistinctBy(u => u.Value)
                    .ToArray();

                var token = GetToken(providerName);
                var processor = new SaveItemProcessor(siteProvider, _logger, _mapper);

                int lastBadUrlsCount = 0;
                int step = 1;
                for (var i = 0; i < allUrls.Length; i = i + step)
                {
                    var urls = allUrls.Skip(i).Take(step);
                    var tasks = urls
                        .Where(url => url.Status != Status.Active)
                        .Select(url => processor.RunAsync(url, token))
                        .ToList();

                    if (!tasks.Any()) continue;

                    tasks.Add(Task.Delay(5000));

                    try
                    {
                        var sw = Stopwatch.StartNew();

                        Task.WaitAll(tasks.ToArray());

                        sw.Stop(); _logger.Info($"Query with time deloy took: {sw.ElapsedMilliseconds} ms");
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Critical", ex);
                    }

                    _logger.Error($"Bad urls count: {processor.BadUrls.Count()}");

                    // TODO add upda if not updated yet
                    /* To avoid error try create new DbContext:
                     * A second operation started on this context before a previous operation completed.
                     * This is usually caused by different threads using the same instance of DbContext,
                     * however instance members are not guaranteed to be thread safe.
                     * This could also be caused by a nested query being evaluated on the client,
                     * if this is the case rewrite the query avoiding nested invocations.
                     */
                    if (lastBadUrlsCount < processor.BadUrls.Count())
                    {
                        var ctx = new ParserDbContext();
                        ctx.AttachRange(processor.BadUrls);
                        ctx.UpdateRange(processor.BadUrls);
                        ctx.SaveChanges();
                        lastBadUrlsCount = processor.BadUrls.Count();
                    }
                }

                // TODO save bad urls
                //Parallel.ForEach(allUrls, new ParallelOptions { MaxDegreeOfParallelism = step }, url =>
                //   {
                //       processor.RunAsync(url, token).GetAwaiter().GetResult();
                //   });
            }
        }

        private void RunOne()
        {
            foreach (ISiteProvider siteProvider in _siteProviders)
            {
                var providerName = siteProvider.GetType().Name;
                var token = GetToken(providerName);
                var processor = new SaveItemProcessor(siteProvider, _logger, _mapper);
                var url = new DbContext.Entities.Url { Value = " https://rolf-probeg.ru/cars/ford/kuga/14691478/" };

                Task.WaitAll(processor.RunAsync(url, token));
            }
        }

        private class UrlComparer : IEqualityComparer<Url>
        {
            public bool Equals(Url x, Url y)
            {
                if (x.Value == y.Value)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public int GetHashCode(Url obj)
            {
                return obj.Value.GetHashCode();
            }
        }
    }
}
