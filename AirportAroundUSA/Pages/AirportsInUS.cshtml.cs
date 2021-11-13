using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using QuickType;
using QuickTypeWeather;
using SunriseSunset;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Microsoft.Extensions.Configuration;



namespace TouristAttractionsJSON.Pages
{

    public class Item
    {

        public double lat { get; set; }
        public double lon { get; set; }

        public string country { get; set; }

        public string name { get; set; }

        public double tem { get; set; }

        public string sr { get; set; }
        public string ss { get; set; }

    }
    public class TouristAttractionsSitesModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public TouristAttractionsSitesModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [BindProperty]
        public string UnescoCountry { get; set; }

        public bool IsSearchCountry { get; set; }
        public bool IsSearchValid = true;

        
        public bool IsSearchEmpty = false;

        public void OnGet()
        {
            IsSearchCountry = false;
        }
        public void OnPost()
        {
            try
            {
                if (string.IsNullOrEmpty(UnescoCountry))
                {
                    IsSearchEmpty = true;
                }
                else
                {
                    string CountrySearched = UnescoCountry.ToLower();

                    using (var webClient = new WebClient())
                    {

                        string key = System.IO.File.ReadAllText("TouristAPIKey.txt");


                        String jsonString = webClient.DownloadString("https://github.com/shivika24/tourism-project/blob/master/db.json");
                        JSchema schema = JSchema.Parse(System.IO.File.ReadAllText("AirportSchema.json"));
                        JObject jsonObject = JObject.Parse(jsonString);
                        QuickType.Welcome welcome = QuickType.Welcome.FromJson(jsonString);
                        List<Item> items = new List<Item>();
                        double t;
                        string sunrise;
                        string sunset;
                        for (int i = 0; i < 1052; i++)
                        {
                            Fields field = welcome.Records[i].Fields;
                            if (CountrySearched == field.CountryEn.ToLower())
                            {

                                {
                                    string weatherString = webClient.DownloadString("https://github.com/shivika24/tourism-project/blob/master/db.json" + field.Latitude + "&lon=" + field.Longitude + "&key=" + key + "&include=minutely");

                                    QuickTypeWeather.Welcome welcomeWeather = QuickTypeWeather.Welcome.FromJson(weatherString);

                                    string SunriseSunsetString = webClient.DownloadString("https://github.com/shivika24/tourism-project/blob/master/db.json" + field.Latitude + "&lng=" + field.Longitude);
                                    SunriseSunset.HighFive RiseSet = SunriseSunset.HighFive.FromJson(SunriseSunsetString);
                                    sunrise = RiseSet.Results.Sunrise;
                                    sunset = RiseSet.Results.Sunset;
                                    var roundedLat = Math.Round(field.Latitude, 5);
                                    var roundedLong = Math.Round(field.Longitude, 5);

                                    t = welcomeWeather.Data[0].Temp;

                                    items.Add(new Item { lat = roundedLat, lon = roundedLong, country = field.CountryEn, name = field.NameEn, tem=t, sr=sunrise, ss=sunset});

                                };
                                if (items.Count != 0)
                                    ViewData["Items"] = items;
                                else
                                    ViewData["IsSearchValid"] = false;
                            }


                        }
                    }

                    IsSearchCountry = true;

                }
            }
            catch (Exception)
            {
                Console.WriteLine("Exception occured while searching the Usites");
            }
        }
    }
}
