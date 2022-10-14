using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arrba.Parser.DbContext;
using Arrba.Parser.Logger;
using Arrba.Parser.Mapper;
using Arrba.Parser.Processors;
using Arrba.Parser.Provider;

namespace Arrba.Parser.Managers
{
    /// <summary>
    /// Update item if can't find the item by url any more,
    /// or the item was sold
    /// </summary>
    public class DeactualizeManager : BaseManager, IParserManager
    {
        private readonly IEnumerable<ISiteProvider> _siteProviders;
        private readonly ILogService _logger;
        private readonly BaseMapper _mapper;
        private readonly ParserDbContext _context;

        public DeactualizeManager(IEnumerable<ISiteProvider> siteProviders, ILogService logger, ParserDbContext context, BaseMapper mapper)
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
                    .Where(l => l.LinksRequestId == urlInfo.Id)
                    .Where(l => l.Status == Status.Active)
                    .Where(l => l.ExternalId > 0)
                    .OrderBy(l => l.Id)
                    .ToArray();

                var token = GetToken(providerName);
                var processor = new DeactualizeItemProcessor(siteProvider, _logger);

                const int step = 10;
                for (var i = 0; i < allUrls.Length; i = i + step)
                {
                    var urls = allUrls.Skip(i).Take(step);
                    var tasks = urls
                        .Select(url => processor.RunAsync(url, token))
                        .ToArray();

                    Task.WaitAll(tasks);
                }
            }


        }
    }
}
