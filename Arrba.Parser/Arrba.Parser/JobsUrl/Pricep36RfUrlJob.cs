using Arrba.Parser.DbContext;
using Arrba.Parser.Logger;
using Arrba.Parser.Managers;
using Arrba.Parser.Provider;

namespace Arrba.Parser.JobsUrl
{
    public class Pricep36RfUrlJob : UrlManager
    {
        public Pricep36RfUrlJob(ISiteProvider siteProvider, ILogService logger, ParserDbContext context) : base(siteProvider, logger, context)
        {
        }
    }
}
