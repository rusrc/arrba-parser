using System;
using System.Collections.Generic;
using System.Text;
using Arrba.Parser.Provider.Realization;
using Arrba.Parser.Services;

namespace Arrba.Parser.Provider
{
    public class SiteProviderList : List<ISiteProvider>
    {
        public SiteProviderList()
        {
            // this.Add(new MagazinpricepovRuProvider(new HttpBaseClient()));
            this.Add(new RolfProbegRuProvider(new HttpBaseClient()));
        }
    }
}
