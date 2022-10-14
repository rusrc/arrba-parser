using System;
using System.Threading.Tasks;
using Arrba.Parser.DbContext;
using Arrba.Parser.Logger;
using Arrba.Parser.Managers;
using Arrba.Parser.Mapper;
using Arrba.Parser.Provider;
using Arrba.Parser.Provider.Realization;
using Arrba.Parser.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Arrba.Parser
{
    public class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddTransient<ILogService, DebugLogService>();
            services.AddTransient<BaseMapper, DictionaryMapper>();
            services.AddTransient<ParserDbContext>(x => new ParserDbContext());
            services.AddTransient<IHttpClient, HttpBaseClient>();
            services.AddTransient<ISiteProvider, PricepiVoronezhRuProvider>();
            services.AddTransient<SaveManager>();
            services.AddTransient<UrlManager>();
            services.AddTransient<DeactualizeManager>();

            var provider = services.BuildServiceProvider();
            ILogService logService = provider.GetService<ILogService>();
            const bool getLinksFirst = true;

            try
            {
                var saveManager = provider.GetService<SaveManager>();
                var getAndSaveItemsTask = new Task(async () =>
                {
                    try
                    {
                        saveManager.Run();
                    }
                    catch (Exception ex)
                    {
                        logService.Error(ex.Message, ex); throw;
                    }
                });

                var linksManager = provider.GetService<UrlManager>();
                var getAndSaveLinksTask = new Task(async () =>
                {
                    try
                    {
                        await linksManager.RunAsync();
                    }
                    catch (Exception ex)
                    {
                        logService.Error(ex.Message, ex); throw;
                    }
                });

                if (getLinksFirst)
                {
                    getAndSaveLinksTask.Start();
                    getAndSaveLinksTask.Wait();
                }
                else
                {
                    getAndSaveItemsTask.Start();
                    getAndSaveItemsTask.Wait();
                }

                Console.Write("Finished");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                logService.Error(ex.Message, ex);
            }
        }
    }
}
