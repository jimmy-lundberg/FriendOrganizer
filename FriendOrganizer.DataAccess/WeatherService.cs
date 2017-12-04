using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using FriendOrganizer.DataAccess.WeatherData;
using FriendOrganizer.Model;

namespace FriendOrganizer.DataAccess
{
    public class WeatherService: IWeatherService
    {
        // 5-day weather forecast for Gothenburg: https://www.metaweather.com/api/location/890869

        string baseUrl;
        RestClient client;

        public WeatherService()
        {
            baseUrl = "https://www.metaweather.com";
            client = new RestClient(baseUrl);
        }

        public Dictionary<int, WeatherInfo> GetWeather()
        {
            var request = new RestRequest("/api/location/890869", Method.GET);
            IRestResponse<Weather> response;

            try
            {
                response = client.Execute<Weather>(request);
            }
            catch (Exception)
            {
                return null;
            }
            
            if (response.Data is null)
            {
                return null;
            }

            var consolidatedWeather = response.Data.consolidated_weather;

            var weatherForecast = new Dictionary<int, WeatherInfo>();
            var key = 0;

            foreach (var weather in consolidatedWeather)
            {
                weatherForecast.Add(key,
                    new WeatherInfo
                    {
                        Date = weather.applicable_date,
                        Temperature = (weather.the_temp == null) ? "Not found" : (Math.Round((double)weather.the_temp)).ToString() + "°C",
                        WeatherStateImageUrl = $"{baseUrl}/static/img/weather/png/64/{weather.weather_state_abbr}.png"
                    });

                key++;
            }

            return weatherForecast;
        }
    }
}
