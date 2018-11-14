using System.Collections.Generic;
using xNet;
using Newtonsoft.Json;

namespace VkAPITutorial
{
    class VkAPI
    {
        public const string __APPID = "6743784";  //ID приложения
        private const string __VKAPIURL = "https://api.vk.com/method/";  //Ссылка для запросов
        private string _Token;  //Токен, использующийся при запросах

        public VkAPI(string AccessToken)
        {
            _Token = AccessToken;
        }

        public Dictionary<string, string> GetInformation(string UserId, string[] Fields)  //Получение заданной информации о пользователе с заданным ID 
        {
            HttpRequest GetInformation = new HttpRequest();
            GetInformation.AddUrlParam("user_ids", UserId);
            GetInformation.AddUrlParam("access_token", _Token);
            GetInformation.AddUrlParam("version", "5.87");
            string Params = "";
            foreach (string i in Fields)
            {
                Params += i + ",";
            }
            Params = Params.Remove(Params.Length - 1);
            GetInformation.AddUrlParam("fields", Params);
            string Result = GetInformation.Get(__VKAPIURL + "users.get").ToString();
            Result = Result.Substring(13, Result.Length - 15);
            Dictionary<string, string> Dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(Result);
            return Dict;
        }

        public string GetCityById(string CityId)  //Перевод ID города в название
        {
            HttpRequest GetCityById = new HttpRequest();
            GetCityById.AddUrlParam("city_ids", CityId);
            GetCityById.AddUrlParam("access_token", _Token);
            GetCityById.AddUrlParam("version", "5.87");
            string Result = GetCityById.Get(__VKAPIURL + "database.getCitiesById").ToString();
            Result = Result.Substring(13, Result.Length - 15);
            Dictionary<string, string> Dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(Result);
            return Dict["name"];
        }

        int m_random_id = 10;
        public Dictionary<string, string> SendMessage(string text2send, string peer_id)  
        {
            //https://vk.com/dev/messages.send?params[user_id]=271185190&params[random_id]=7&params[peer_id]=271185190&params[chat_id]=222&params[message]=Test%20message&params[dont_parse_links]=0&params[v]=5.87
            HttpRequest request = new HttpRequest();
            request.AddUrlParam("random_id", m_random_id++);
            request.AddUrlParam("peer_id", peer_id);
            request.AddUrlParam("user_id", peer_id);
            request.AddUrlParam("chat_id", 1);
            request.AddUrlParam("message", text2send);
            request.AddUrlParam("v", "5.87");
            request.AddUrlParam("access_token", _Token);
            request.AddUrlParam("redirect_uri", @"https://oauth.vk.com/blank.html");


            string Result = request.Get(__VKAPIURL + "messages.send").ToString();
            
            Dictionary<string, string> Dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(Result);
            return Dict;
        }

        public RootObject users_search(int birth_month, int birth_year)
        {
            HttpRequest request = new HttpRequest();
            request.AddUrlParam("city", "1463");
            request.AddUrlParam("sort", "1");
            request.AddUrlParam("count", 1000);
            request.AddUrlParam("birth_year", birth_year);
            request.AddUrlParam("birth_month", birth_month);
            request.AddUrlParam("fields", "photo_id,verified,sex,bdate,city,country,home_town,has_photo,can_write_private_message,can_send_friend_request,user_id");
            request.AddUrlParam("v", "5.87");
            request.AddUrlParam("access_token", _Token);
            request.AddUrlParam("redirect_uri", @"https://oauth.vk.com/blank.html");


            string Result = request.Get(__VKAPIURL + "users.search").ToString();

            RootObject Dict = JsonConvert.DeserializeObject<RootObject>(Result);
            return Dict;
        }

        public string GetCountryById(string CountryId)  //Перевод ID страны в название
        {
            HttpRequest GetCountryById = new HttpRequest();
            GetCountryById.AddUrlParam("country_ids", CountryId);
            GetCountryById.AddUrlParam("access_token", _Token);
            GetCountryById.AddUrlParam("version", "5.87");
            string Result = GetCountryById.Get(__VKAPIURL + "database.getCountriesById").ToString();
            Result = Result.Substring(13, Result.Length - 15);
            Dictionary<string, string> Dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(Result);
            return Dict["name"];
        }
    }
}