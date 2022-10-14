using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Arrba.Parser.DbContext;
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

        public SaveManager(IEnumerable<ISiteProvider> siteProviders, ILogService logger, BaseMapper mapper, ParserDbContext context)
        : base(logger, context)
        {
            this._siteProviders = siteProviders;
            this._logger = logger;
            this._context = context;
            this._mapper = mapper;
        }

        public void Run()
        {
            // this.RunOne(); return;

            foreach (ISiteProvider siteProvider in _siteProviders)
            {
                var providerName = siteProvider.GetType().Name;
                var urlInfo = GetUrlInfo(providerName);
                var allUrls = _context.Urls
                    .Where(u => u.LinksRequestId == urlInfo.Id)
                    .Where(u => u.Status == Status.WaitToCheck || u.Status == null)
                    .Where(u => u.ExternalId <= 0)
                    .OrderBy(u => u.Id)
                    .Distinct()
                    .ToArray();

                var token = GetToken(providerName);
                var processor = new SaveItemProcessor(siteProvider, _logger, _mapper);

                const int step = 5;
                for (var i = 0; i < allUrls.Length; i = i + step)
                {
                    var urls = allUrls.Skip(i).Take(step);
                    var tasks = urls
                        .Select(url => processor.RunAsync(url, token))
                        .ToArray();

                    try
                    {
                        Task.WaitAll(tasks);
                    }
                    catch (System.Exception ex)
                    {
                        _logger.Error("Critical", ex);                      
                    }

                    _logger.Error($"Bad urls count: {processor.BadUrls.Count()}");           
                }

                // TODO save bad urls
                _context.Urls.AddRange(processor.BadUrls);
                _context.SaveChanges();

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
    }
}
