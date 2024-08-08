using DiningSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using static DiningSystem.Pages.webManagerModel;

namespace DiningSystem.Pages
{
    public class EditRestaurantModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly string _connectionString;

        public EditRestaurantModel(IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _userManager = userManager;
        }   

        [BindProperty]
        public Restaurant Restaurant { get; set; }
        [BindProperty]
        public MenuItem NewMenuItem { get; set; }


        public async Task<IActionResult> OnGetAsync(int id)
        {
            await LoadRestaurantAsync(id);
            return Page();
        }

        private async Task LoadRestaurantAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "SELECT * FROM restaurant WHERE r_id = @r_id";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@r_id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            Restaurant = new Restaurant
                            {
                                r_id = (int)reader["r_id"],
                                r_name = reader["r_name"].ToString(),
                                r_description = reader["r_description"].ToString(),
                                AdminUsername = reader["r_admin"].ToString(),
                                max_table_servings = (int)reader["max_table_servings"],
                                open_close_timings = reader["open_close_timing"].ToString()
                            };
                        }
                    }
                }
            }
        }




        public async Task<IActionResult> OnPostSaveRestaurantAsync()
        {
            if (!ModelState.IsValid)
            {
                // Log the errors for debugging
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"Property: {state.Key}, Error: {error.ErrorMessage}");
                    }
                }

                await LoadRestaurantAsync(Restaurant.r_id);
                return Page();
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"
            UPDATE restaurant SET
            r_name = @r_name,
            r_description = @r_description,
            r_admin = @AdminUsername,
            max_table_servings = @max_table_servings,
            open_close_timing = @open_close_timings
            WHERE r_id = @r_id";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@r_id", Restaurant.r_id);
                    command.Parameters.AddWithValue("@r_name", Restaurant.r_name);
                    command.Parameters.AddWithValue("@r_description", Restaurant.r_description);
                    command.Parameters.AddWithValue("@AdminUsername", Restaurant.AdminUsername);
                    command.Parameters.AddWithValue("@max_table_servings", Restaurant.max_table_servings);
                    command.Parameters.AddWithValue("@open_close_timings", Restaurant.open_close_timings);
                    await command.ExecuteNonQueryAsync();
                }
            }

            return RedirectToPage("/Restaurant");
        }





        public async Task<IActionResult> OnPostAddMenuItemAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadRestaurantAsync(Restaurant.r_id);
                return Page();
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"
                    INSERT INTO restaurantMenu (menu_item, menu_description, menu_item_price, r_id)
                    VALUES (@menu_item, @menu_description, @menu_item_price, @r_id)";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@menu_item", NewMenuItem.menu_item);
                    command.Parameters.AddWithValue("@menu_description", NewMenuItem.menu_description);
                    command.Parameters.AddWithValue("@menu_item_price", NewMenuItem.menu_item_price);
                    command.Parameters.AddWithValue("@r_id", Restaurant.r_id);
                    await command.ExecuteNonQueryAsync();
                }
            }

            // Reload restaurant details to reflect changes
            await LoadRestaurantAsync(Restaurant.r_id);

            return RedirectToPage("/EditRestaurant", new { id = Restaurant.r_id });
        }




    }
    public class Restaurant
    {
        public int r_id { get; set; }
        public string r_name { get; set; }
        public string r_description { get; set; }
        public string AdminUsername { get; set; }
        public int max_table_servings { get; set; }
        public string open_close_timings { get; set; }
    }

    public class MenuItem
    {
        public int menu_id { get; set; }
        public string menu_item { get; set; }
        public string menu_description { get; set; }
        public decimal menu_item_price { get; set; }
        public int r_id { get; set; }
    }
}



