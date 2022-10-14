using Arrba.Parser.DbContext;
using Arrba.Parser.Logger;
using Arrba.Parser.Managers;
using Arrba.Parser.Mapper;
using Arrba.Parser.Provider.Realization;

namespace Arrba.Parser.JobsSave
{
    public class MagazinpricepovRuSaveJob : SaveManager
    {
        public MagazinpricepovRuSaveJob(MagazinpricepovRuProvider siteProvider, ILogService logger, BaseMapper mapper, ParserDbContext context)
            : base(new[] { siteProvider }, logger, mapper, context)
        {
        }
    }
}
