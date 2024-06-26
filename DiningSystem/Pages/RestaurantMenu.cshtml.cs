using DiningSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
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
        private readonly UserManager<ApplicationUser> _userManager;
        public bool IsUserAdmin { get; set; }
        [BindProperty]
        public int ItemId {  get; set; }
        public RestaurantMenuModel(IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }


        public void OnGet()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = _userManager.GetUserAsync(User).Result;
                IsUserAdmin = _userManager.IsInRoleAsync(user, "admin").Result;
            }

            LoadMenu();
        }


        private void LoadMenu()
        {
            string connection = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new SqlConnection(connection))
            {
                con.Open();
                string sql = "select * from restaurantMenu where r_id = @Id";
                using (SqlCommand command = new SqlCommand(sql, con))
                {
                    command.Parameters.AddWithValue("@Id", id);
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

        public IActionResult OnPostAddToCart(int productId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            var user = _userManager.GetUserAsync(User).Result;
            if (_userManager.IsInRoleAsync(user, "admin").Result)
            {
                TempData["ToastMessage"] = "Admins cannot add items to the cart";
                return Page();
            }

            string userId = _userManager.GetUserId(User);
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string checkQuery = "SELECT COUNT(*) FROM CartItems WHERE UserId = @UserId AND ItemId = @ItemId"; //Item Id
                SqlCommand checkCommand = new SqlCommand(checkQuery, connection);
                checkCommand.Parameters.AddWithValue("@UserId", userId);
                checkCommand.Parameters.AddWithValue("@ItemId", productId);
                int existingCount = (int)checkCommand.ExecuteScalar();

                if (existingCount == 0)
                {
                    string insertQuery = "INSERT INTO CartItems (UserId, ItemId, Quantity) VALUES (@UserId, @ItemId, 1)";//item id
                    SqlCommand insertCommand = new SqlCommand(insertQuery, connection);
                    insertCommand.Parameters.AddWithValue("@UserId", userId);
                    insertCommand.Parameters.AddWithValue("@ItemId", productId);
                    insertCommand.ExecuteNonQuery();

                    TempData["ToastMessage"] = "Item added to cart successfully";
                }
                else
                {
                    TempData["ToastMessage"] = "Item already exists in the cart";
                }
            }
            LoadMenu();
            /*return RedirectToPage("/RestaurantMenu");*/
            return Page();
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
