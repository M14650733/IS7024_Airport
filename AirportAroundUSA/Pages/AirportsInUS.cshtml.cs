using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Linq;
using System;

namespace TouristAttractionsJSON.Pages
{

    public class Airport
    {
        [JsonPropertyName("city_code")]
        public string CityCode { get; set; }
        
        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; }

        public string CityName { get; set; }

        public string Name
        {
            get
            {
                return this.NameTranslations["en"];
            }
        }

        [JsonPropertyName("name_translations")]
        public Dictionary<string, string> NameTranslations { get; set; }

    }

    public class City
    {
        public string Name
        {
            get
            {
                return this.NameTranslations["en"];
            }
        }

        [JsonPropertyName("name_translations")]
        public Dictionary<string, string> NameTranslations { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }
    }
    public class AirportsInUSModel : PageModel
    {
        public List<Airport> Airports { get; set; }

        public string SelectedCity { get; set; }

        public async Task OnGetAsync(string? name)
        {
            this.SelectedCity = name;
            this.Airports = await getAirports(name ?? "");
        }

        public async Task<List<City>> getCities()
        {
            var url = "https://api.travelpayouts.com/data/en/cities.json";

            HttpClient client = new HttpClient();
            var cityData = await client.GetAsync(url);

            var cityDataString = await cityData.Content.ReadAsStringAsync();
            var cities = JsonSerializer.Deserialize<List<City>>(cityDataString);
            return cities;
        }

        public async Task<List<Airport>> getAirports(string cityName = "")
        {
            var url = "https://api.travelpayouts.com/data/en/airports.json";
            
            HttpClient client = new HttpClient();
            var data = await client.GetAsync(url);

            var dataString = await data.Content.ReadAsStringAsync();
            var airports = JsonSerializer.Deserialize<List<Airport>>(dataString);
            var cities = await getCities();
            return airports.Where(_ => 
            _.CountryCode.Equals("US", StringComparison.CurrentCultureIgnoreCase)
            )
                .AsParallel()
                .WithDegreeOfParallelism(10)
                .Select(airport => {
                    var cityCode = airport.CityCode;
                    var city = cities.Where(_ => _.Code.Equals(cityCode, StringComparison.CurrentCultureIgnoreCase))
                    .FirstOrDefault();
                    
                    if(city != null)
                    {
                        airport.CityName = city.Name;
                    }

                    return airport;
                }).Where(_ => _.CityName.Trim().Contains(cityName.Trim(), StringComparison.CurrentCultureIgnoreCase))
                .ToList();
        }

        public void OnPost()
        {
        }
    }
}
