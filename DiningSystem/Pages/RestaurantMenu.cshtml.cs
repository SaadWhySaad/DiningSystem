using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Security.Principal;

namespace DiningSystem.Pages
{

    public class RestaurantMenuModel : PageModel
        
    {
        
        [FromRoute]
        public string id { get; set; }
        public List<RestaurantMenu> listMenu = new List<RestaurantMenu>();
        private readonly IConfiguration _configuration;

        public RestaurantMenuModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        


        public void OnGet()
        {

            string connection = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new SqlConnection(connection))
            {
                con.Open();
                string sql = "select * from restaurantMenu where r_id = " + id;
                using (SqlCommand command = new SqlCommand(sql, con))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            RestaurantMenu menu = new RestaurantMenu();
                            menu.Id = reader.GetInt32(0);
                            menu.menu_item = reader.GetString(1);
                            menu.menu_description = reader.GetString(2);
                            menu.menu_item_price = reader.GetInt32(3);
                            menu.restaurnant_id = reader.GetInt32(4);
                            listMenu.Add(menu);
                        }
                    }
                }
            }
        }

        
    }
       

    public class RestaurantMenu
    {
        public int Id { get; set; }
        public string menu_item { get; set; }
        public string menu_description { get; set; }
        public int menu_item_price { get; set; }
        public int restaurnant_id { get; set; }
    }
}
