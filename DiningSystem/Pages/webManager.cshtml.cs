using DiningSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace DiningSystem.Pages
{
    public class webManagerModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public webManagerModel(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public List<Restaurant> Restaurants { get; set; } = new List<Restaurant>();
        public Dictionary<string, string> AdminUsernames { get; set; } = new Dictionary<string, string>();

        public async Task<IActionResult> OnGetAsync()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string sql = "SELECT r_id, r_name, r_description, r_admin, max_table_servings, open_close_timing FROM restaurant";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var restaurant = new Restaurant
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Description = reader.GetString(2),
                                Admin = reader.GetString(3),
                                MaxTableServings = reader.GetInt32(4),
                                OpenCloseTimings = reader.GetString(5)
                            };
                            Restaurants.Add(restaurant);

                            var admin = await _userManager.FindByIdAsync(restaurant.Admin);
                            if (admin != null)
                            {
                                AdminUsernames[restaurant.Admin] = admin.UserName;
                            }
                        }
                    }
                }
            }

            return Page();
        }
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Step 1: Retrieve the admin ID associated with the restaurant
                        string adminId = null;
                        string getAdminIdSql = "SELECT r_admin FROM restaurant WHERE r_id = @id";
                        using (SqlCommand getAdminIdCommand = new SqlCommand(getAdminIdSql, connection, transaction))
                        {
                            getAdminIdCommand.Parameters.AddWithValue("@id", id);
                            using (SqlDataReader reader = await getAdminIdCommand.ExecuteReaderAsync())
                            {
                                if (reader.Read())
                                {
                                    adminId = reader["r_admin"].ToString();
                                }
                            }
                        }

                        if (adminId == null)
                        {
                            // If no admin ID is found, handle the error
                            ModelState.AddModelError("", "Admin ID not found for the restaurant.");
                            return Page();
                        }

                        // Step 2: Delete the restaurant
                        string sql = "DELETE FROM restaurant WHERE r_id = @id";
                        using (SqlCommand command = new SqlCommand(sql, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@id", id);
                            await command.ExecuteNonQueryAsync();
                        }

                        // Step 3: Role adjustment
                        var adminUser = await _userManager.FindByIdAsync(adminId);
                        if (adminUser != null)
                        {
                            if (await _userManager.IsInRoleAsync(adminUser, "admin"))
                            {
                                await _userManager.RemoveFromRoleAsync(adminUser, "admin");
                            }

                            // Step 4: Optionally add to the "customer" role
                            if (!await _userManager.IsInRoleAsync(adminUser, "customer"))
                            {
                                await _userManager.AddToRoleAsync(adminUser, "customer");
                            }
                        }

                        transaction.Commit(); // Commit the transaction
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback(); // Rollback the transaction on error
                        ModelState.AddModelError("", "An error occurred while deleting the restaurant.");
                        // Optionally log the error or handle it as needed
                    }
                }
            }

            return RedirectToPage("/Restaurant"); // Redirect to a list page or another page
        }
        public class Restaurant
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Admin { get; set; }
            public int MaxTableServings { get; set; }
            public string OpenCloseTimings { get; set; }
        }
    }
}
