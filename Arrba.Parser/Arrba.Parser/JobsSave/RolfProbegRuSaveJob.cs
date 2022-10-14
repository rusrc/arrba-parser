using Arrba.Parser.DbContext;
using Arrba.Parser.Logger;
using Arrba.Parser.Managers;
using Arrba.Parser.Mapper;
using Arrba.Parser.Provider.Realization;

namespace Arrba.Parser.JobsSave
{
    public class RolfProbegRuSaveJob : SaveManager
    {
        public RolfProbegRuSaveJob(RolfProbegRuProvider siteProvider, ILogService logger, BaseMapper mapper, ParserDbContext context)
            : base(new[] { siteProvider }, logger, mapper, context)
        {
        }
    }
}
