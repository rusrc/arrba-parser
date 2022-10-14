using System;
using System.Collections.Generic;
using System.Text;
using Arrba.Parser.DbContext;
using Arrba.Parser.Logger;
using Arrba.Parser.Managers;
using Arrba.Parser.Mapper;
using Arrba.Parser.Provider;
using Arrba.Parser.Provider.Realization;
using Arrba.Parser.Services;

namespace Arrba.Parser
{
    public class ManagerFactory
    {
        private readonly ILogService _logger;
        private readonly ParserDbContext _context;
        private readonly BaseMapper _mapper;

        public ManagerFactory(ILogService logger, ParserDbContext context, BaseMapper mapper)
        {
            this._logger = logger;
            this._context = context;
            this._mapper = mapper;
        }

        public IParserManager GetUrlManager<TProvider>() where TProvider : ISiteProvider
        {
            if (typeof(TProvider) == typeof(RolfProbegRuProvider))
            {
                var p = new RolfProbegRuProvider(new HttpBaseClient());

                return new UrlManager(p, _logger, _context);
            }
            else if (typeof(TProvider) == typeof(MagazinpricepovRuProvider))
            {
                var p = new MagazinpricepovRuProvider(new HttpBaseClient());

                return new UrlManager(p, _logger, _context);
            }

            return null;
        }

        public void UrlRun<TProvider>() where TProvider : ISiteProvider
        {
            if (typeof(TProvider) == typeof(RolfProbegRuProvider))
            {
                var p = new RolfProbegRuProvider(new HttpBaseClient());

                new UrlManager(p, _logger, _context).Run();
            }
            else if (typeof(TProvider) == typeof(MagazinpricepovRuProvider))
            {
                var p = new MagazinpricepovRuProvider(new HttpBaseClient());

                new UrlManager(p, _logger, _context).Run();
            }
        }
    }
}
