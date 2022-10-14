using Arrba.Parser.DbContext;
using Arrba.Parser.Logger;
using Arrba.Parser.Mapper;
using Arrba.Parser.Provider;
using Arrba.Parser.Provider.Realization;

namespace Arrba.Parser.Managers.Realization
{
    public class MagazinpricepovRuSaveManager : SaveManager
    {
        public MagazinpricepovRuSaveManager(MagazinpricepovRuProvider siteProvider, ILogService logger, BaseMapper mapper, ParserDbContext context)
            : base(new[] { siteProvider }, logger, mapper, context)
        {
        }
    }
}
