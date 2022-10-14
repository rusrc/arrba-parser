using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Arrba.Parser.Services
{
    public class HttpBaseClient : IHttpClient
    {
        public async Task<string> GetAsync(string url)
        {
            //var handler = new HttpClientHandler()
            //{
            //    Proxy = new WebProxy()
            //};

            // Register windows-1254
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using (var client = new HttpClient())
            {    
                var htmlPage = await client.GetStringAsync(url);

                //var response = await client.GetByteArrayAsync(url);
                //var htmlPage = Encoding.UTF8.GetString(response, 0, response.Length - 1);

                return htmlPage;
            }
        }
    }
}
