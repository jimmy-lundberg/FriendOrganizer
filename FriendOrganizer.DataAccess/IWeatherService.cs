using FriendOrganizer.Model;
using System.Collections.Generic;

namespace FriendOrganizer.DataAccess
{
    public interface IWeatherService
    {
        Dictionary<int, WeatherInfo> GetWeather();
    }
}
