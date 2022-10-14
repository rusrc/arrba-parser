using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Arrba.Parser.DbContext;
using Arrba.Parser.DbContext.Entities;
using Arrba.Parser.Dto;
using Arrba.Parser.Exceptions;
using Arrba.Parser.Logger;
using Arrba.Parser.Mapper;
using Arrba.Parser.Provider;
using Arrba.Parser.Services;
using Microsoft.EntityFrameworkCore;

namespace Arrba.Parser.Processors
{
    public class SaveItemProcessor
    {
        private readonly ISiteProvider _siteProvider;
        private readonly ILogService _logger;
        private readonly IMapper<VehicleDto, VehicleRawDto> _mapper;
        private List<Url> _badUrls;
        public IEnumerable<Url> BadUrls => _badUrls;

        public SaveItemProcessor(
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

            var item = null as VehicleDto;

            using (var context = new ParserDbContext())
            {
                try
                {
                    this._logger.Debug($"Start... link: {url.Id} parse url: {url.Value}, provider: {ProviderName}");

                    var rawItem = await _siteProvider.GetItemAsync(url);

                    item = await _mapper.MapAsync(rawItem);
                    item.DealershipId = (await GetDealershipByNameAsync(item.DealershipName)).Id;
                    item.TemporaryImageFolder = await GenereateUniqueItemFolderAsync(token);

                    // TODO in order to save HDD space decrease the count of images to 1
                    await SaveImagesAsync(item.ImageSrcs.Take(1).ToArray(), item.TemporaryImageFolder, token);

                    url.ExternalId = await AddItemAsync(item, token);
                    url.Status = Status.Active;
                    url.StackTrace = url.ErrorMessage = "";

                    context.Entry(url).State = EntityState.Modified;
                    await context.SaveChangesAsync();

                    this._logger.Info($@"Successfully added. ExternalId {url.ExternalId}. Original url: {url.Value}");
                }
                #region Exceptions
                catch (ItemSoldException ex)
                {
                    SaveException(url, ex, context, Status.NotFound);
                }
                catch (DealershipNotFoundException ex)
                {
                    SaveDealership(item, context);
                    SaveException(url, ex, context, Status.WaitToCheck);
                }
                catch (NotFoundException ex)
                {
                    SaveException(url, ex, context, Status.NotFound);
                }
                catch (ProviderException ex)
                {
                    SaveException(url, ex, context, Status.WaitToCheck);
                }
                catch (Exception ex)
                {
                    SaveException(url, ex, context);
                }
                #endregion
                finally
                {
                    await DeleteImagesAsync(item?.TemporaryImageFolder);
                }
            }
        }


        #region Private helpers
        private string ProviderName => _siteProvider.GetType().Name;

        private async Task<string> GenereateUniqueItemFolderAsync(string token)
        {
            return await ArrbaApiService.GenereateUniqueItemFolderAsync(token);
        }

        private async Task SaveImagesAsync(string[] imageSources, string folderName, string jwtToken)
        {
            if (imageSources != null && imageSources.Length > 0)
            {
                foreach (var src in imageSources)
                {
                    var bytes = await ArrbaApiService.GetImageBytesAsync(src);
                    var fileName = Path.GetFileName(src);
                    await ArrbaApiService.UploadImageAsync(bytes, fileName, folderName, jwtToken);
                }
            }
        }

        private async Task DeleteImagesAsync(string uniqueFolderName)
        {
            if (string.IsNullOrEmpty(uniqueFolderName) is false)
            {
                await ArrbaApiService.DeleteImagesAsync(uniqueFolderName);
            }
        }

        private async Task<int> AddItemAsync(VehicleDto item, string token)
        {
            try
            {
                return await ArrbaApiService.AddItemAsync(item, token);
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

        private void SaveDealership(VehicleDto item, ParserDbContext context)
        {
            var dealer = context.Dealerships.FirstOrDefault(d => d.Name == item.DealershipName);

            if (dealer == null)
            {
                context.Dealerships.Add(new Dealership
                {
                    Name = item?.DealershipName,
                    Address = item?.DealershipAddress,
                    PhoneNumber = item?.DealershipNumberPhone,
                    ProviderName = ProviderName
                });
                context.SaveChanges();
            }
        }

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

        protected void SaveException(Url url, Exception ex, ParserDbContext context, Status status = Status.Error)
        {
            url.ErrorMessage = $"{ex.GetType().Name} {ex.Message}";
            url.StackTrace = ex.StackTrace;
            url.Status = status;

            //context.Entry(url).State = EntityState.Modified;
            //context.SaveChanges();
            this._badUrls.Add(url);
            // _logger.Error($"{ex.GetType().Name}. Message: {ex.Message}; Link id: {url.Id}, Link url: {url.Value}. Provider: {_siteProvider.GetType().Name}" /*, ex*/);
        }
        #endregion
    }
}
