using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Arrba.Parser.DbContext;
using Arrba.Parser.DbContext.Entities;
using Arrba.Parser.Dto;
using Arrba.Parser.Exceptions;
using Arrba.Parser.Logger;
using Arrba.Parser.Mapper;
using Arrba.Parser.Provider;
using Arrba.Parser.Services;

namespace Arrba.Parser.Processors
{
    public class SaveItemProcessorTest
    {
        private readonly ISiteProvider _siteProvider;
        private readonly ILogService _logger;
        private readonly IMapper<VehicleDto, VehicleRawDto> _mapper;

        private readonly List<Url> _badUrls;

        public IEnumerable<Url> BadUrls => _badUrls;

        public SaveItemProcessorTest(
            ISiteProvider siteProvider,
            ILogService logger,
            IMapper<VehicleDto, VehicleRawDto> mapper)
        {
            this._siteProvider = siteProvider;
            this._logger = logger;
            this._mapper = mapper;

            this._badUrls = new List<Url>();
        }

        public async Task RunAsync(Url url, string token)
        {
            if (url.Status == Status.Active)
            {
                throw new ProcessorException($"You can't put '{nameof(Status.Active)}' status");
            }

            try
            {
                this._logger.Debug($"Start... link: {url.Id} parse url: {url.Value}, provider: {ProviderName}");

                var rawItem = await _siteProvider.GetItemAsync(url);
                var item = await _mapper.MapAsync(rawItem);

                item.DealershipId = (await GetDealershipByNameAsync(item.DealershipName)).Id;

                this._logger.Info($@"Successfully added. Url id {url.Id}. Original url: {url.Value}");
            }
            catch (ItemSoldException ex)
            {
                SaveException(url, ex);
            }
            catch (DealershipNotFoundException ex)
            {
                SaveException(url, ex);
            }
            catch (NotFoundException ex)
            {
                SaveException(url, ex);
            }
            catch (ProviderException ex)
            {
                SaveException(url, ex);
            }
            catch (Exception ex)
            {
                SaveException(url, ex);
            }
        }

        #region Private helpers
        private string ProviderName => _siteProvider.GetType().Name;

        private async Task<DealershipDto> GetDealershipByNameAsync(string name)
        {
            try
            {
                return await ArrbaApiService.GetDealershipAsync(name);
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse r && r.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new DealershipNotFoundException(GetType().Name, $"can't get dealership by name: {name}", ex);
                }

                throw new ProviderException(GetType().Name, ex);
            }
        }

        protected void SaveException(Url url, Exception ex)
        {
            url.ErrorMessage = $"Exception type: {ex.GetType().Name}, message: {ex.Message}";
            url.StackTrace = ex.StackTrace;
            url.Status = Status.Error;

            _badUrls.Add(url);
        }
        #endregion
    }
}
