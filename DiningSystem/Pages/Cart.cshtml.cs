using DiningSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;

namespace DiningSystem.Pages
{
    public class CartModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartModel(IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }

        [BindProperty]
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public int TotalAmount { get; set; }

        public void OnGet()
        {
            if (!User.Identity.IsAuthenticated)
            {
                Response.Redirect("/Account/Login");
            }
            string userId = _userManager.GetUserId(User);
            LoadCartItems(userId);
        }

        public IActionResult OnPostRemoveItem(int remove)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("Account/Login");
            }
            string userId = _userManager.GetUserId(User);

            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = "DELETE FROM CartItems WHERE UserId = @UserId AND ItemId = @ItemId";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add(new SqlParameter("@UserId", userId));
                    command.Parameters.Add(new SqlParameter("@ItemId", remove));
                    command.ExecuteNonQuery();
                }
            }
            LoadCartItems(userId);
            return Page();
        }

        private void LoadCartItems(string userId)
        {
            CartItems = new List<CartItem>();
            TotalAmount = 0;

            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = @"
                    SELECT m.menu_id, m.menu_item, m.menu_item_price, c.Quantity
                    FROM CartItems c
                    JOIN RestaurantMenu m ON c.ItemId = m.menu_id
                    WHERE c.UserId = @UserId";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add(new SqlParameter("@UserId", userId));
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var cartItem = new CartItem
                            {
                                ItemId = reader.GetInt32(0),
                                MenuItem = reader.GetString(1),
                                Price = reader.GetInt32(2),
                                Quantity = reader.GetInt32(3)
                            };
                            CartItems.Add(cartItem);
                            TotalAmount += cartItem.Price * cartItem.Quantity;
                        }
                    }
                }
            }
        }
    }

    public class CartItem
    {
        public int ItemId { get; set; }
        public string MenuItem { get; set; }
        public int Price { get; set; }
        public int Quantity { get; set; }
    }
}
