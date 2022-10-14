using System.Collections.Generic;
using System.Threading.Tasks;
using Arrba.Parser.DbContext.Entities;
using Arrba.Parser.Dto;
using Arrba.Parser.Exceptions;

namespace Arrba.Parser.Provider
{
    public interface ISiteProvider
    {
        /// <summary>
        /// Get item by url or link
        /// </summary>
        /// <param name="url"></param>
        /// <exception cref="NormalizedValueException"></exception>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="ProviderException"></exception>
        /// <exception cref="RequiredFieldException"></exception>
        /// <exception cref="PriceNotFoundException"></exception>
        /// <exception cref="ItemSoldException"></exception>
        /// <returns></returns>
        Task<VehicleRawDto> GetItemAsync(Url url);
        /// <summary>
        /// Get all links on target web-site
        /// to process them later in GetItem method
        /// <exception cref="NotFoundException"></exception>
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Url>> GetUrlsAsync();
    }
}
