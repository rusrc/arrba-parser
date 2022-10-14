using System.Threading.Tasks;

namespace Arrba.Parser.Services
{
    public interface IHttpClient
    {
        Task<string> GetAsync(string url);
    }
}
