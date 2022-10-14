using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Arrba.Parser.Services
{
    public class GoogleMapService : IMapService
    {
        private const string key = "AIzaSyB6YmI7_4__d_bsZ-w1SCwk7BNoGUKDQ_g";
        public string GetCityNameByAddress(string address)
        {
            using (var client = new WebClient())
            {
                var url = $@"https://maps.googleapis.com/maps/api/geocode/json?address={address}&key={key}";
                var response = client.DownloadString(url);
                var addressResults = Newtonsoft.Json.JsonConvert.DeserializeObject<AddressResults>(response);

                // Moskva, Sankt-Peterburg
                var addressComponents = addressResults.results
                    .SelectMany(r => r.address_components)
                    .Where(ac => ac.types.FirstOrDefault(t => t.Contains("locality")) == "locality")
                    .ToArray();

                //administrative_area_level_1

                return addressComponents.FirstOrDefault()?.long_name;
            }
        }
    }

    internal class AddressComponent
    {
        public string long_name { get; set; }
        public string short_name { get; set; }
        public List<string> types { get; set; }
    }

    internal class Location
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    internal class Northeast
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    internal class Southwest
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    internal class Viewport
    {
        public Northeast northeast { get; set; }
        public Southwest southwest { get; set; }
    }

    internal class Geometry
    {
        public Location location { get; set; }
        public string location_type { get; set; }
        public Viewport viewport { get; set; }
    }

    internal class PlusCode
    {
        public string compound_code { get; set; }
        public string global_code { get; set; }
    }

    internal class Result
    {
        public List<AddressComponent> address_components { get; set; }
        public string formatted_address { get; set; }
        public Geometry geometry { get; set; }
        public string place_id { get; set; }
        public PlusCode plus_code { get; set; }
        public List<string> types { get; set; }
    }

    internal class AddressResults
    {
        public List<Result> results { get; set; }
        public string status { get; set; }
    }
}
