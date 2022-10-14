using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Arrba.Parser.DbContext;
using Arrba.Parser.DbContext.Entities;
using Arrba.Parser.Exceptions;
using Arrba.Parser.Logger;
using Arrba.Parser.Provider;
using Arrba.Parser.Services;
using Microsoft.EntityFrameworkCore;

namespace Arrba.Parser.Processors
{
    public class DeactualizeItemProcessor
    {
        private readonly ISiteProvider _siteProvider;
        private readonly ILogService _logger;

        public DeactualizeItemProcessor(ISiteProvider siteProvider, ILogService logger)
        {
            this._siteProvider = siteProvider;
            this._logger = logger;
        }

        public async Task RunAsync(Url url, string token)
        {
            using (var context = new ParserDbContext())
            {
                try
                {
                    this._logger.Debug($"Start... link: {url.Id} parse url: {url.Value}, provider: {ProviderName}");

                    var rawItem = await _siteProvider.GetItemAsync(url);

                    this._logger.Info($"Successfully added. ExternalId {url.ExternalId}.");
                }
                catch (ItemSoldException)
                {
                    await UpdateUrlAsync(url, context);
                    DeactualizeItem(url.ExternalId, token);
                }
                catch (NotFoundException)
                {
                    await UpdateUrlAsync(url, context);
                    DeactualizeItem(url.ExternalId, token);
                }
                catch (ProviderException ex)
                {
                    SaveException(url, ex, context, Status.WaitToCheck);
                }
                catch (Exception ex)
                {
                    SaveException(url, ex, context);
                }
            }
        }

        protected void SaveException(Url url, Exception ex, ParserDbContext context, Status status = Status.Error)
        {
            url.ErrorMessage = $"{ex.GetType().Name} {ex.Message}";
            url.StackTrace = ex.StackTrace;
            url.Status = status;

            context.Entry(url).State = EntityState.Modified;
            context.SaveChanges();
            _logger.Error($"{ex.GetType().Name}. Message: {ex.Message}; Link id: {url.Id}, Link url: {url.Value}. Provider: {_siteProvider.GetType().Name}", ex);
        }

        private void DeactualizeItem(int itemId, string token)
        {
            try
            {
                ArrbaApiService.DeactualizeItem(itemId, token);
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse response && response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    var stream = response.GetResponseStream();
                    if (stream != null)
                    {
                        string responseText;
                        using (var reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            responseText = reader.ReadToEnd();
                        }
                        throw new ServerException(GetType().Name, responseText, ex);
                    }
                }

                throw;
            }
        }

        private async Task UpdateUrlAsync(Url url, ParserDbContext context)
        {
            var urlFromDb = await context.Urls.FindAsync(url.Id);
            urlFromDb.Status = Status.NotFound;
            urlFromDb.ErrorMessage = urlFromDb.StackTrace = string.Empty;
            await context.SaveChangesAsync();
        }

        private string ProviderName => _siteProvider.GetType().Name;
    }
}
