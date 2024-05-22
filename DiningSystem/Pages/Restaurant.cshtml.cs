using DiningSystem.Migrations;
using DiningSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DiningSystem.Pages
{
    /*[Authorize]*/
    public class UserModel : PageModel
    {
        public List<RestaurantInfo> listRestaurant = new List<RestaurantInfo>();
        private readonly IConfiguration _configuration;
      

        private readonly UserManager<ApplicationUser> userManager;
        public ApplicationUser? appUser;

        public UserModel(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            _configuration = configuration;
        }




        public void OnGet()
        {
            var task = userManager.GetUserAsync(User);
            task.Wait();
            appUser = task.Result;


      

            try
            {
                string connection = _configuration.GetConnectionString("DefaultConnection");
                
                using(SqlConnection con = new SqlConnection(connection))
                {
                    con.Open();
                    string sql = "select * from restaurant";
                    using (SqlCommand command = new SqlCommand(sql,con))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                RestaurantInfo restaurant = new RestaurantInfo();
                                restaurant.id = reader.GetInt32(0);
                                restaurant.r_name = reader.GetString(1);
                                restaurant.r_description = reader.GetString(2);
                                restaurant.Image = "banner_" + restaurant.id;
                                restaurant.restaurnat_admin = reader.GetString(3);
                                listRestaurant.Add(restaurant);
                            }
                        }
                    }
                }
            }catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
    }   
    

    public class RestaurantInfo
    {
        public int id { get; set; }
        public string r_name { get; set; }
        public string r_description { get; set; }
        public string Image {  get; set; }
        public string restaurnat_admin { get; set; }
    }
}
