using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VkAPITutorial
{
    public class City
    {
        public int id { get; set; }
        public string title { get; set; } = string.Empty;
    }

    public class Country
    {
        public int id { get; set; }
        public string title { get; set; } = string.Empty;
    }

    public class Item
    {
        public int id { get; set; }
        public string first_name { get; set; } = string.Empty;
        public string last_name { get; set; } = string.Empty;
        public int sex { get; set; }
        public City city { get; set; }
        public Country country { get; set; }
        public string photo_id { get; set; } = string.Empty;
        public int has_photo { get; set; }
        public int can_write_private_message { get; set; }
        public int can_send_friend_request { get; set; }
        public int verified { get; set; }
        public string home_town { get; set; } = string.Empty;
        public string bdate { get; set; } = string.Empty;
    }

    public class Response
    {
        public int count { get; set; }
        public List<Item> items { get; set; }
    }

    public class RootObject
    {
        public Response response { get; set; }
    }
}
